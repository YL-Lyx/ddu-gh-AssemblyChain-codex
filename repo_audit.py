#!/usr/bin/env python3
"""Static repository audit tailored for the AssemblyChain source tree.

The tool walks the provided directory (typically ``src``), extracts structural
and quality metrics from every C# file, and produces deterministic Markdown and
JSON reports.  The intent is to automate the discovery of architectural smells,
complexity hotspots, duplication and documentation gaps so they can feed a
refactoring roadmap.

The analyser intentionally avoids external dependencies to keep execution
simple inside CI pipelines.  Its output is deterministic to keep diffs stable
between runs when the underlying code base has not changed.
"""
from __future__ import annotations

import argparse
import json
import re
from collections import defaultdict
from dataclasses import dataclass, field
from hashlib import md5
from pathlib import Path
from typing import Dict, Iterable, List, Optional, Sequence, Tuple

# --- parsing helpers -------------------------------------------------------

CORE_EXTENSIONS = {".cs"}

CLASS_RE = re.compile(r"\b(class|struct|record)\s+(?P<name>[A-Za-z_][A-Za-z0-9_]*)")
NAMESPACE_RE = re.compile(r"namespace\s+([A-Za-z0-9_.]+)")
USING_RE = re.compile(r"^using\s+([A-Za-z0-9_.]+)\s*;", re.MULTILINE)
METHOD_RE = re.compile(
    r"""
    ^\s*
    (?P<signature>
        (?:(?:public|protected|private|internal|static|sealed|abstract|virtual|override|
            async|extern|unsafe|new|partial)\s+)*
        (?:[A-Za-z0-9_<>,\[\]\.?]+\s+)+
        (?P<name>[A-Za-z_][A-Za-z0-9_]*)\s*
        \((?P<params>[^)]*)\)
    )
    \s*(?P<body>\{|=>)
    """,
    re.VERBOSE | re.MULTILINE,
)
DOC_COMMENT_RE = re.compile(r"^\s*///", re.MULTILINE)
ASYNC_AWAIT_RE = re.compile(r"\bawait\b")
COMPLEXITY_KEYWORDS = re.compile(r"\b(if|for|foreach|while|case|catch|switch|else if|&&|\|\||\?)\b")


# --- dataclasses -----------------------------------------------------------


@dataclass
class MethodMetrics:
    """Lightweight metrics for a method or local function."""

    name: str
    parameters: int
    complexity: int
    length: int
    doc_present: bool
    start_line: int
    end_line: int
    max_nesting: int
    is_async: bool
    await_count: int
    is_public: bool


@dataclass
class FileMetrics:
    """Aggregate metrics for a single source file."""

    path: Path
    namespace: Optional[str]
    loc: int
    sloc: int
    doc_lines: int
    classes: List[str]
    methods: List[MethodMetrics]
    await_count: int
    usings: List[str]
    source: str = field(repr=False, compare=False)

    @property
    def method_count(self) -> int:
        return len(self.methods)

    @property
    def complexity_total(self) -> int:
        return sum(m.complexity for m in self.methods)

    @property
    def avg_complexity(self) -> float:
        return 0.0 if not self.methods else self.complexity_total / len(self.methods)

    @property
    def max_complexity(self) -> int:
        return max((m.complexity for m in self.methods), default=0)

    @property
    def doc_ratio(self) -> float:
        return 0.0 if self.loc == 0 else self.doc_lines / self.loc


@dataclass
class DuplicateBlock:
    """Represents a duplicated chunk of code across files."""

    hash: str
    lines: List[str]
    occurrences: List[Tuple[Path, int]]


@dataclass
class RepositoryMetrics:
    files: List[FileMetrics]
    dependencies: Dict[str, List[str]]
    dependency_cycles: List[List[str]]
    duplicate_blocks: List[DuplicateBlock]

    def to_json(self) -> Dict[str, object]:
        return {
            "files": [file_to_json(f) for f in self.files],
            "dependencies": self.dependencies,
            "dependency_cycles": self.dependency_cycles,
            "duplicates": [
                {
                    "hash": block.hash,
                    "lines": block.lines,
                    "occurrences": [
                        {"file": str(path), "start_line": line}
                        for path, line in block.occurrences
                    ],
                }
                for block in self.duplicate_blocks
            ],
        }


def file_to_json(metrics: FileMetrics) -> Dict[str, object]:
    return {
        "path": str(metrics.path),
        "namespace": metrics.namespace,
        "loc": metrics.loc,
        "sloc": metrics.sloc,
        "doc_lines": metrics.doc_lines,
        "classes": metrics.classes,
        "methods": [
            {
                "name": method.name,
                "parameters": method.parameters,
                "complexity": method.complexity,
                "length": method.length,
                "doc_present": method.doc_present,
                "start_line": method.start_line,
                "end_line": method.end_line,
                "max_nesting": method.max_nesting,
                "is_async": method.is_async,
                "await_count": method.await_count,
                "is_public": method.is_public,
            }
            for method in metrics.methods
        ],
        "await_count": metrics.await_count,
        "usings": metrics.usings,
    }


# --- core analysis --------------------------------------------------------


def walk_files(root: Path) -> Iterable[Path]:
    for path in sorted(root.rglob("*")):
        if path.suffix in CORE_EXTENSIONS and path.is_file():
            yield path


def strip_comments_and_strings(text: str) -> str:
    text = re.sub(r"/\*.*?\*/", " ", text, flags=re.DOTALL)
    text = re.sub(r"//.*", " ", text)
    text = re.sub(r'@"(?:""|[^"])*"', '""', text)
    text = re.sub(r'"(?:\\.|[^"\\])*"', '""', text)
    text = re.sub(r"'(?:\\.|[^'\\])'", "''", text)
    return text


def count_parameters(param_text: str) -> int:
    if not param_text.strip():
        return 0
    parts: List[str] = []
    depth = 0
    current: List[str] = []
    for ch in param_text:
        if ch == '<':
            depth += 1
        elif ch == '>':
            depth = max(0, depth - 1)
        elif ch == ',' and depth == 0:
            part = ''.join(current).strip()
            if part:
                parts.append(part)
            current = []
            continue
        current.append(ch)
    part = ''.join(current).strip()
    if part:
        parts.append(part)
    return len(parts)


def find_block_end(text: str, brace_index: int) -> int:
    depth = 0
    for idx in range(brace_index, len(text)):
        char = text[idx]
        if char == '{':
            depth += 1
        elif char == '}':
            depth -= 1
            if depth == 0:
                return idx + 1
    return len(text)


def compute_max_nesting(body: str) -> int:
    depth = 0
    max_depth = 0
    for char in body:
        if char == '{':
            depth += 1
            max_depth = max(max_depth, depth)
        elif char == '}':
            depth = max(0, depth - 1)
    return max_depth


def extract_methods(text: str) -> List[MethodMetrics]:
    methods: List[MethodMetrics] = []
    for match in METHOD_RE.finditer(text):
        params_text = match.group("params")
        param_count = count_parameters(params_text)
        signature_start = match.start("signature")
        body_indicator = match.group("body")
        body_start = match.end() - 1 if body_indicator == "{" else match.end()

        if body_indicator == "{":
            body_end = find_block_end(text, body_start)
        else:
            remainder = text[body_start:]
            terminator = remainder.find(";")
            body_end = body_start + terminator + 1 if terminator != -1 else match.end()

        method_source = text[signature_start:body_end]
        body_text = text[body_start:body_end]
        clean_body = strip_comments_and_strings(body_text)

        complexity = 1 + len(COMPLEXITY_KEYWORDS.findall(clean_body))
        await_count = len(ASYNC_AWAIT_RE.findall(body_text))
        max_nesting = compute_max_nesting(clean_body)

        start_line = text[:signature_start].count("\n") + 1
        end_line = text[:body_end].count("\n") + 1
        doc_window = text[:signature_start].splitlines()[-3:]
        doc_present = any(line.strip().startswith("///") for line in doc_window)

        methods.append(
            MethodMetrics(
                name=match.group("name"),
                parameters=param_count,
                complexity=complexity,
                length=max(1, end_line - start_line + 1),
                doc_present=doc_present,
                start_line=start_line,
                end_line=end_line,
                max_nesting=max_nesting,
                is_async="async" in match.group("signature"),
                await_count=await_count,
                is_public="public" in match.group("signature"),
            )
        )
    return methods


def compute_file_metrics(path: Path, root: Path) -> FileMetrics:
    text = path.read_text(encoding="utf-8")
    lines = text.splitlines()
    loc = len(lines)
    sloc = sum(1 for line in lines if line.strip())
    namespace_match = NAMESPACE_RE.search(text)
    namespace = namespace_match.group(1) if namespace_match else None
    classes = [m.group("name") for m in CLASS_RE.finditer(text)]
    doc_lines = len(DOC_COMMENT_RE.findall(text))
    methods = extract_methods(text)
    await_count = len(ASYNC_AWAIT_RE.findall(text))
    usings = [u for u in USING_RE.findall(text) if not u.startswith("System")]
    relative_path = path.relative_to(root)

    return FileMetrics(
        path=relative_path,
        namespace=namespace,
        loc=loc,
        sloc=sloc,
        doc_lines=doc_lines,
        classes=classes,
        methods=methods,
        await_count=await_count,
        usings=usings,
        source=text,
    )


def build_dependency_graph(files: Sequence[FileMetrics]) -> Dict[str, List[str]]:
    graph: Dict[str, set] = defaultdict(set)
    for metrics in files:
        if not metrics.namespace:
            continue
        for used in metrics.usings:
            if used == metrics.namespace:
                continue
            graph[metrics.namespace].add(used)
        graph.setdefault(metrics.namespace, set())
    return {node: sorted(edges) for node, edges in sorted(graph.items())}


def detect_dependency_cycles(graph: Dict[str, List[str]]) -> List[List[str]]:
    cycles: List[List[str]] = []
    visited: set[str] = set()
    in_stack: set[str] = set()
    stack: List[str] = []

    def dfs(node: str) -> None:
        visited.add(node)
        in_stack.add(node)
        stack.append(node)
        for neighbour in graph.get(node, []):
            if neighbour not in graph:
                continue
            if neighbour not in visited:
                dfs(neighbour)
            elif neighbour in in_stack:
                start_idx = stack.index(neighbour)
                cycle = stack[start_idx:] + [neighbour]
                if cycle not in cycles:
                    cycles.append(cycle)
        stack.pop()
        in_stack.remove(node)

    for node in graph:
        if node not in visited:
            dfs(node)
    return cycles


def detect_duplicate_blocks(files: Sequence[FileMetrics], window: int = 8) -> List[DuplicateBlock]:
    seen: Dict[str, List[Tuple[Path, int]]] = defaultdict(list)
    block_lines: Dict[str, List[str]] = {}

    for metrics in files:
        original_lines = metrics.source.splitlines()
        normalized: List[str] = []
        line_numbers: List[int] = []
        for index, line in enumerate(original_lines):
            stripped = line.strip()
            if not stripped or stripped.startswith("//"):
                continue
            normalized.append(stripped)
            line_numbers.append(index + 1)

        for idx in range(len(normalized) - window + 1):
            chunk = normalized[idx : idx + window]
            digest = md5("\n".join(chunk).encode("utf-8")).hexdigest()
            seen[digest].append((metrics.path, line_numbers[idx]))
            block_lines.setdefault(digest, chunk)

    duplicates = [
        DuplicateBlock(hash=digest, lines=block_lines[digest], occurrences=occurrences)
        for digest, occurrences in seen.items()
        if len({occ[0] for occ in occurrences}) > 1
    ]
    duplicates.sort(key=lambda block: (-len(block.occurrences), block.hash))
    return duplicates


def build_directory_tree(root: Path) -> str:
    def walk(path: Path, prefix: str = "") -> Iterable[str]:
        entries = sorted(path.iterdir(), key=lambda p: (not p.is_dir(), p.name.lower()))
        for index, entry in enumerate(entries):
            connector = "└── " if index == len(entries) - 1 else "├── "
            yield f"{prefix}{connector}{entry.name}"
            if entry.is_dir():
                extension = "    " if index == len(entries) - 1 else "│   "
                yield from walk(entry, prefix + extension)

    root = root.resolve()
    lines = [root.name]
    lines.extend(walk(root))
    return "\n".join(lines)


# --- issue classification --------------------------------------------------


def classify_issues(metrics: RepositoryMetrics) -> List[Dict[str, object]]:
    issues: List[Dict[str, object]] = []

    for cycle in metrics.dependency_cycles:
        issues.append(
            {
                "priority": "P0",
                "type": "CyclicDependency",
                "message": f"Cyclic namespace dependency detected: {' → '.join(cycle)}",
            }
        )

    fan_in: Dict[str, int] = defaultdict(int)
    for namespace, deps in metrics.dependencies.items():
        for dep in deps:
            fan_in[dep] += 1

    for namespace, deps in metrics.dependencies.items():
        fan_out = len(deps)
        if fan_out >= 10:
            issues.append(
                {
                    "priority": "P1",
                    "type": "HighCoupling",
                    "message": f"`{namespace}` has fan-out {fan_out}; consider splitting responsibilities.",
                }
            )
        if fan_in.get(namespace, 0) >= 10:
            issues.append(
                {
                    "priority": "P1",
                    "type": "SharedHotspot",
                    "message": f"`{namespace}` is a dependency hotspot with fan-in {fan_in[namespace]}.",
                }
            )

    for file_metrics in metrics.files:
        if file_metrics.loc >= 600:
            issues.append(
                {
                    "priority": "P1",
                    "type": "LargeFile",
                    "message": f"{file_metrics.path} exceeds 600 LOC ({file_metrics.loc} LOC).",
                }
            )
        if file_metrics.doc_ratio < 0.05 and file_metrics.loc > 200:
            issues.append(
                {
                    "priority": "P2",
                    "type": "LowDocumentation",
                    "message": f"{file_metrics.path} has sparse XML documentation ({file_metrics.doc_ratio:.1%}).",
                }
            )

        for method in file_metrics.methods:
            if method.complexity >= 20:
                issues.append(
                    {
                        "priority": "P0",
                        "type": "ExtremeComplexity",
                        "message": f"{file_metrics.path}:{method.name} complexity {method.complexity}.",
                    }
                )
            elif method.complexity >= 12:
                issues.append(
                    {
                        "priority": "P1",
                        "type": "HighComplexity",
                        "message": f"{file_metrics.path}:{method.name} complexity {method.complexity}.",
                    }
                )
            if method.length >= 120:
                issues.append(
                    {
                        "priority": "P1",
                        "type": "LongMethod",
                        "message": f"{file_metrics.path}:{method.name} spans {method.length} lines.",
                    }
                )
            elif method.length >= 80:
                issues.append(
                    {
                        "priority": "P2",
                        "type": "LongMethod",
                        "message": f"{file_metrics.path}:{method.name} spans {method.length} lines.",
                    }
                )
            if method.parameters >= 6:
                issues.append(
                    {
                        "priority": "P2",
                        "type": "LongParameterList",
                        "message": f"{file_metrics.path}:{method.name} defines {method.parameters} parameters.",
                    }
                )
            if not method.doc_present and method.length >= 30:
                issues.append(
                    {
                        "priority": "P2",
                        "type": "UndocumentedMethod",
                        "message": f"{file_metrics.path}:{method.name} lacks XML documentation.",
                    }
                )

    if metrics.duplicate_blocks:
        issues.append(
            {
                "priority": "P1",
                "type": "Duplication",
                "message": f"Detected {len(metrics.duplicate_blocks)} duplicated fragments across modules.",
            }
        )

    return sorted(issues, key=lambda issue: issue["priority"])


def summarise_metrics(metrics: RepositoryMetrics) -> Dict[str, object]:
    loc_values = [f.loc for f in metrics.files]
    method_complexities = [m.complexity for f in metrics.files for m in f.methods]
    doc_ratios = [f.doc_ratio for f in metrics.files]
    async_methods = [m for f in metrics.files for m in f.methods if m.is_async]

    return {
        "files": len(metrics.files),
        "total_loc": sum(loc_values),
        "total_sloc": sum(f.sloc for f in metrics.files),
        "avg_loc": (sum(loc_values) / len(loc_values)) if loc_values else 0,
        "avg_complexity": (sum(method_complexities) / len(method_complexities)) if method_complexities else 0,
        "max_complexity": max(method_complexities) if method_complexities else 0,
        "avg_doc_ratio": (sum(doc_ratios) / len(doc_ratios)) if doc_ratios else 0,
        "async_method_count": len(async_methods),
        "duplicate_fragment_count": len(metrics.duplicate_blocks),
    }


def generate_recommendations(metrics: RepositoryMetrics, issues: List[Dict[str, object]]) -> Dict[str, List[str]]:
    recommendations: Dict[str, List[str]] = {
        "architecture": [],
        "module": [],
        "function": [],
        "engineering": [],
    }

    if metrics.dependency_cycles:
        recommendations["architecture"].append(
            "Break cyclic namespace dependencies via inversion (interfaces) or mediator services and enforce one-way references."
        )
    if any(issue["type"] == "HighCoupling" for issue in issues):
        recommendations["architecture"].append(
            "Review high fan-out namespaces and split responsibilities into cohesive modules with explicit APIs."
        )
    if any(issue["type"] == "SharedHotspot" for issue in issues):
        recommendations["architecture"].append(
            "Introduce façade services around shared hotspots to reduce direct dependencies and protect domain boundaries."
        )

    if any(issue["type"] == "LargeFile" for issue in issues):
        recommendations["module"].append(
            "Decompose oversized files into focused classes following single-responsibility principles."
        )
    if metrics.duplicate_blocks:
        recommendations["module"].append(
            "Factor repeated fragments into shared utilities or generics to eliminate duplication across modules."
        )

    if any(issue["type"] in {"HighComplexity", "ExtremeComplexity"} for issue in issues):
        recommendations["function"].append(
            "Refactor high-complexity methods using guard clauses, extraction, and descriptive helpers to flatten nesting."
        )
    if any(issue["type"] == "LongMethod" for issue in issues):
        recommendations["function"].append(
            "Split long methods around distinct responsibilities and favour pipelines or smaller private helpers."
        )
    if any(issue["type"] == "LongParameterList" for issue in issues):
        recommendations["function"].append(
            "Introduce parameter objects or configuration records to shrink long parameter lists."
        )

    if any(issue["type"] in {"LowDocumentation", "UndocumentedMethod"} for issue in issues):
        recommendations["engineering"].append(
            "Raise documentation coverage by requiring XML summaries for public APIs and critical workflows."
        )
    recommendations["engineering"].append(
        "Automate this audit via CI to monitor metric drift and enforce agreed quality gates."
    )
    recommendations["engineering"].append(
        "Add regression tests around identified hotspots before refactoring to protect behaviour."
    )

    return {section: sorted(set(items)) for section, items in recommendations.items() if items}


# --- reporting ------------------------------------------------------------


def render_dependency_section(metrics: RepositoryMetrics) -> List[str]:
    lines = ["## Architecture & Dependencies", ""]
    if metrics.dependencies:
        lines.append("| Namespace | Fan-out | Fan-in | Dependencies |")
        lines.append("| --- | --- | --- | --- |")
        fan_in: Dict[str, int] = defaultdict(int)
        for ns, deps in metrics.dependencies.items():
            for dep in deps:
                fan_in[dep] += 1
        for namespace, deps in metrics.dependencies.items():
            lines.append(
                f"| `{namespace}` | {len(deps)} | {fan_in.get(namespace, 0)} | {', '.join(f'`{d}`' for d in deps) if deps else '∅'} |"
            )
    else:
        lines.append("No non-System namespace dependencies discovered.")

    if metrics.dependency_cycles:
        lines.append("")
        lines.append("**Cycles detected:**")
        for cycle in metrics.dependency_cycles:
            lines.append(f"* {' → '.join(f'`{node}`' for node in cycle)}")
    else:
        lines.append("")
        lines.append("No cyclic dependencies detected.")
    return lines


def render_hotspots_section(metrics: RepositoryMetrics) -> List[str]:
    lines = ["## Hotspots", ""]

    complex_methods = sorted(
        (m for f in metrics.files for m in f.methods),
        key=lambda m: m.complexity,
        reverse=True,
    )
    long_methods = sorted(
        (m for f in metrics.files for m in f.methods),
        key=lambda m: m.length,
        reverse=True,
    )

    if complex_methods:
        lines.append("### Top complexity methods")
        lines.append("")
        lines.append("| Method | Complexity | Length | File |")
        lines.append("| --- | --- | --- | --- |")
        for method in complex_methods[:10]:
            file = next(f for f in metrics.files if method in f.methods)
            lines.append(
                f"| `{method.name}` | {method.complexity} | {method.length} | `{file.path}` |"
            )
        lines.append("")

    if long_methods:
        lines.append("### Longest methods")
        lines.append("")
        lines.append("| Method | Length | Complexity | File |")
        lines.append("| --- | --- | --- | --- |")
        for method in long_methods[:10]:
            file = next(f for f in metrics.files if method in f.methods)
            lines.append(
                f"| `{method.name}` | {method.length} | {method.complexity} | `{file.path}` |"
            )
        lines.append("")

    if metrics.duplicate_blocks:
        lines.append("### Duplicate fragments")
        lines.append("")
        for block in metrics.duplicate_blocks[:5]:
            lines.append("```csharp")
            lines.extend(block.lines)
            lines.append("```")
            for path, start in block.occurrences:
                lines.append(f"* `{path}` @ line {start}")
            lines.append("")
    else:
        lines.append("No notable duplication detected beyond the configured threshold.")
        lines.append("")

    return lines


def render_file_metrics_section(metrics: RepositoryMetrics) -> List[str]:
    lines = ["## File Metrics", ""]
    lines.append("| File | LOC | SLOC | Methods | Avg Complexity | Max Complexity | Doc % | Await Count |")
    lines.append("| --- | --- | --- | --- | --- | --- | --- | --- |")
    for file_metrics in sorted(metrics.files, key=lambda f: str(f.path)):
        lines.append(
            "| `{}` | {} | {} | {} | {:.2f} | {} | {:.1%} | {} |".format(
                file_metrics.path,
                file_metrics.loc,
                file_metrics.sloc,
                file_metrics.method_count,
                file_metrics.avg_complexity,
                file_metrics.max_complexity,
                file_metrics.doc_ratio,
                file_metrics.await_count,
            )
        )
    lines.append("")
    return lines


def render_issues_section(issues: List[Dict[str, object]]) -> List[str]:
    lines = ["## Issues & Risks", ""]
    if issues:
        for issue in issues:
            lines.append(f"* **{issue['priority']}** – {issue['type']}: {issue['message']}")
    else:
        lines.append("Automated heuristics did not flag notable issues.")
    lines.append("")
    return lines


def render_recommendations_section(recommendations: Dict[str, List[str]]) -> List[str]:
    lines = ["## Recommendations", ""]
    for section, items in recommendations.items():
        title = section.capitalize()
        lines.append(f"### {title}")
        lines.append("")
        for item in items:
            lines.append(f"* {item}")
        lines.append("")
    return lines


def render_roadmap_section() -> List[str]:
    return [
        "## Roadmap",
        "",
        "1. **Phase 1 – Baseline**: Run automated audit in CI, validate metrics with the architecture team, and agree on target gates.",
        "2. **Phase 2 – Stabilise**: Address P0/P1 issues (cycles, extreme complexity, duplication) while adding guard tests around hotspots.",
        "3. **Phase 3 – Optimise**: Refine module boundaries, pursue performance improvements (profiling-driven) and enforce documentation standards.",
        "",
    ]


def render_report(root: Path, metrics: RepositoryMetrics, issues: List[Dict[str, object]], recommendations: Dict[str, List[str]]) -> str:
    summary = summarise_metrics(metrics)
    tree = build_directory_tree(root)

    report_lines = [
        "# AssemblyChain Source Audit Report",
        "",
        "## Repository Overview",
        "",
        f"* Analysed directory: `{root}`",
        f"* Total files analysed: {summary['files']}",
        f"* Total LOC / SLOC: {summary['total_loc']} / {summary['total_sloc']}",
        f"* Average LOC per file: {summary['avg_loc']:.1f}",
        f"* Average method complexity: {summary['avg_complexity']:.2f}",
        f"* Maximum method complexity: {summary['max_complexity']}",
        f"* Average documentation density: {summary['avg_doc_ratio']:.2%}",
        f"* Async methods detected: {summary['async_method_count']}",
        f"* Duplicate fragments detected: {summary['duplicate_fragment_count']}",
        "",
        "### Directory structure",
        "",
        "```",
        tree,
        "```",
        "",
    ]

    report_lines.extend(render_dependency_section(metrics))
    report_lines.append("")
    report_lines.extend(render_hotspots_section(metrics))
    report_lines.extend(render_file_metrics_section(metrics))
    report_lines.extend(render_issues_section(issues))
    report_lines.extend(render_recommendations_section(recommendations))
    report_lines.extend(render_roadmap_section())

    return "\n".join(report_lines)


# --- orchestration --------------------------------------------------------


def run_audit(core_dir: Path, output_dir: Path) -> RepositoryMetrics:
    files = [compute_file_metrics(path, core_dir) for path in walk_files(core_dir)]
    dependencies = build_dependency_graph(files)
    dependency_cycles = detect_dependency_cycles(dependencies)
    duplicates = detect_duplicate_blocks(files)
    metrics = RepositoryMetrics(
        files=files,
        dependencies=dependencies,
        dependency_cycles=dependency_cycles,
        duplicate_blocks=duplicates,
    )

    issues = classify_issues(metrics)
    recommendations = generate_recommendations(metrics, issues)
    report = render_report(core_dir, metrics, issues, recommendations)

    summary = summarise_metrics(metrics)

    output_dir.mkdir(parents=True, exist_ok=True)
    (output_dir / "audit_report.md").write_text(report, encoding="utf-8")
    (output_dir / "audit_report.json").write_text(
        json.dumps(
            {
                "summary": summary,
                "issues": issues,
                "recommendations": recommendations,
                "data": metrics.to_json(),
            },
            indent=2,
        )
        + "\n",
        encoding="utf-8",
    )

    return metrics


def parse_args(argv: Optional[Sequence[str]] = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Audit the AssemblyChain source directory")
    parser.add_argument("source_dir", type=Path, help="Path to the source directory (e.g. src)")
    parser.add_argument(
        "--output",
        type=Path,
        default=Path("reports"),
        help="Directory where the report files will be written",
    )
    return parser.parse_args(argv)


def main(argv: Optional[Sequence[str]] = None) -> None:
    args = parse_args(argv)
    run_audit(args.source_dir, args.output)


if __name__ == "__main__":
    main()
