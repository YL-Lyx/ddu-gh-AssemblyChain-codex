#!/usr/bin/env python3
"""Repository audit tool for the AssemblyChain Core project.

This script inspects the Core directory and generates
 * a Markdown report summarising architecture, quality metrics and issues
 * a JSON file containing the raw metrics that back the report

The implementation is intentionally lightweight and deterministic so it can be
run inside CI/CD without external dependencies.
"""
from __future__ import annotations

import argparse
import json
import re
import statistics
from collections import defaultdict
from dataclasses import asdict, dataclass, field
from hashlib import md5
from pathlib import Path
from typing import Dict, Iterable, List, Optional, Sequence, Tuple

CORE_EXTENSIONS = {".cs"}

CLASS_RE = re.compile(r"\b(class|struct|record)\s+(?P<name>[A-Za-z_][A-Za-z0-9_]*)")
NAMESPACE_RE = re.compile(r"namespace\s+([A-Za-z0-9_.]+)")
USING_RE = re.compile(r"^using\s+([A-Za-z0-9_.]+)\s*;", re.MULTILINE)
METHOD_RE = re.compile(
    r"(?P<header>\b(?:public|protected|private|internal|static|virtual|override|sealed|async|unsafe|extern|partial)\s+"
    r"(?:[A-Za-z0-9_<>,\[\]?]+\s+)*"
    r"(?P<name>[A-Za-z_][A-Za-z0-9_]*)\s*\((?P<params>[^)]*)\)\s*(?:\{|=>))",
    re.MULTILINE,
)
DOC_COMMENT_RE = re.compile(r"^\s*///", re.MULTILINE)
COMPLEXITY_KEYWORDS = re.compile(r"\b(if|for|foreach|while|case|catch|switch|else if|&&|\|\||\?)\b")
ASYNC_AWAIT_RE = re.compile(r"\bawait\b")


@dataclass
class MethodMetrics:
    name: str
    parameters: int
    doc_present: bool
    complexity: int
    line_span: Tuple[int, int]


@dataclass
class FileMetrics:
    path: Path
    namespace: Optional[str]
    loc: int
    sloc: int
    classes: List[str] = field(default_factory=list)
    methods: List[MethodMetrics] = field(default_factory=list)
    doc_lines: int = 0
    complexity_total: int = 0
    await_count: int = 0

    @property
    def method_count(self) -> int:
        return len(self.methods)

    @property
    def doc_ratio(self) -> float:
        return 0.0 if self.loc == 0 else self.doc_lines / self.loc


@dataclass
class DuplicateBlock:
    hash: str
    lines: List[str]
    occurrences: List[Tuple[Path, int]]


@dataclass
class RepositoryMetrics:
    files: List[FileMetrics]
    dependencies: Dict[str, List[str]]
    duplicate_blocks: List[DuplicateBlock]

    def to_json(self) -> Dict[str, object]:
        return {
            "files": [file_to_json(f) for f in self.files],
            "dependencies": self.dependencies,
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
        "classes": metrics.classes,
        "methods": [asdict(m) for m in metrics.methods],
        "doc_lines": metrics.doc_lines,
        "complexity_total": metrics.complexity_total,
        "await_count": metrics.await_count,
    }


def walk_files(root: Path) -> Iterable[Path]:
    for path in sorted(root.rglob("*")):
        if path.suffix in CORE_EXTENSIONS and path.is_file():
            yield path


def strip_comments_and_strings(text: str) -> str:
    # Remove multiline comments
    text = re.sub(r"/\*.*?\*/", " ", text, flags=re.DOTALL)
    # Remove strings to avoid confusing complexity heuristics
    text = re.sub(r'"(?:\\.|[^"\\])*"', '""', text)
    text = re.sub(r"'\\?.'", "''", text)
    return text


def compute_file_metrics(path: Path) -> FileMetrics:
    text = path.read_text(encoding="utf-8")
    lines = text.splitlines()
    loc = len(lines)
    sloc = sum(1 for line in lines if line.strip())
    namespace_match = NAMESPACE_RE.search(text)
    namespace = namespace_match.group(1) if namespace_match else None
    classes = [m.group("name") for m in CLASS_RE.finditer(text)]
    doc_lines = len(DOC_COMMENT_RE.findall(text))

    methods: List[MethodMetrics] = []
    for match in METHOD_RE.finditer(text):
        params = match.group("params").strip()
        param_count = 0 if not params else len([p for p in params.split(",") if p.strip()])
        start_line = text[: match.start()].count("\n") + 1
        block = text[match.end() :]
        end_line = start_line
        brace_depth = 0
        for idx, ch in enumerate(block):
            if ch == "{" or ch == "}":
                brace_depth += 1 if ch == "{" else -1
                if brace_depth < 0:
                    end_line = start_line + block[: idx].count("\n")
                    break
        if brace_depth >= 0:
            end_line = start_line + block.split("\n", 1)[0:1].count("\n")
        method_body = strip_comments_and_strings(text[match.end() :])
        complexity = len(COMPLEXITY_KEYWORDS.findall(method_body.split("}", 1)[0])) + 1
        leading_lines = text[: match.start()].splitlines()
        doc_present = "///" in "\n".join(leading_lines[-3:]) if leading_lines else False
        methods.append(
            MethodMetrics(
                name=match.group("name"),
                parameters=param_count,
                doc_present=doc_present,
                complexity=complexity,
                line_span=(start_line, max(end_line, start_line)),
            )
        )

    complexity_total = sum(m.complexity for m in methods)
    await_count = len(ASYNC_AWAIT_RE.findall(text))

    return FileMetrics(
        path=path,
        namespace=namespace,
        loc=loc,
        sloc=sloc,
        classes=classes,
        methods=methods,
        doc_lines=doc_lines,
        complexity_total=complexity_total,
        await_count=await_count,
    )


def build_dependency_graph(files: Sequence[FileMetrics]) -> Dict[str, List[str]]:
    graph: Dict[str, set] = defaultdict(set)

    for metrics in files:
        if not metrics.namespace:
            continue
        text = metrics.path.read_text(encoding="utf-8")
        for used in USING_RE.findall(text):
            if used.startswith("System"):
                continue
            graph[metrics.namespace].add(used)
    return {node: sorted(edges) for node, edges in sorted(graph.items())}


def detect_duplicate_blocks(files: Sequence[FileMetrics], window: int = 5) -> List[DuplicateBlock]:
    seen: Dict[str, List[Tuple[Path, int]]] = defaultdict(list)
    chunks: Dict[str, Tuple[str, ...]] = {}
    for metrics in files:
        lines = [line.rstrip() for line in metrics.path.read_text(encoding="utf-8").splitlines()]
        normalized = [line for line in lines if line.strip()]
        for idx in range(len(normalized) - window + 1):
            chunk = tuple(normalized[idx : idx + window])
            digest = md5("\n".join(chunk).encode("utf-8")).hexdigest()
            seen[digest].append((metrics.path, idx + 1))
            chunks.setdefault(digest, chunk)
    duplicates = [
        DuplicateBlock(hash=h, lines=list(chunks[h]), occurrences=occurrences)
        for h, occurrences in seen.items()
        if len({path for path, _ in occurrences}) > 1
    ]
    return duplicates


def build_directory_tree(root: Path) -> str:
    def walk(path: Path, prefix: str = "") -> Iterable[str]:
        entries = sorted(list(path.iterdir()), key=lambda p: (not p.is_dir(), p.name.lower()))
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


def classify_issues(metrics: RepositoryMetrics) -> List[Dict[str, object]]:
    issues: List[Dict[str, object]] = []

    for file_metrics in metrics.files:
        if file_metrics.loc > 400 or file_metrics.method_count > 25:
            issues.append(
                {
                    "priority": "P1",
                    "type": "LargeFile",
                    "message": f"{file_metrics.path} is very large ({file_metrics.loc} LOC, {file_metrics.method_count} methods)",
                }
            )
        undocumented = [m for m in file_metrics.methods if not m.doc_present]
        if undocumented and file_metrics.method_count:
            issues.append(
                {
                    "priority": "P2",
                    "type": "Documentation",
                    "message": f"{file_metrics.path} has {len(undocumented)}/{file_metrics.method_count} undocumented methods",
                }
            )
        complex_methods = [m for m in file_metrics.methods if m.complexity > 10]
        for method in complex_methods:
            issues.append(
                {
                    "priority": "P0",
                    "type": "HighComplexity",
                    "message": f"{file_metrics.path}:{method.name} has cyclomatic complexity ~{method.complexity}",
                }
            )

    if metrics.duplicate_blocks:
        issues.append(
            {
                "priority": "P1",
                "type": "Duplication",
                "message": f"Detected {len(metrics.duplicate_blocks)} duplicated code fragments",
            }
        )

    dependency_edges = sum(len(v) for v in metrics.dependencies.values())
    if dependency_edges > len(metrics.dependencies) * 3:
        issues.append(
            {
                "priority": "P1",
                    "type": "Coupling",
                    "message": "Namespace dependency graph shows high fan-out; consider modular boundaries.",
            }
        )

    return issues


def summarise_metrics(metrics: RepositoryMetrics) -> Dict[str, object]:
    loc_values = [f.loc for f in metrics.files]
    method_complexities = [m.complexity for f in metrics.files for m in f.methods]
    avg_complexity = statistics.mean(method_complexities) if method_complexities else 0.0
    max_complexity = max(method_complexities) if method_complexities else 0
    avg_doc_ratio = statistics.mean(f.doc_ratio for f in metrics.files) if metrics.files else 0.0
    duplicate_rate = len(metrics.duplicate_blocks)
    return {
        "files": len(metrics.files),
        "total_loc": sum(loc_values),
        "avg_loc": statistics.mean(loc_values) if loc_values else 0,
        "avg_complexity": avg_complexity,
        "max_complexity": max_complexity,
        "avg_doc_ratio": avg_doc_ratio,
        "duplicate_fragments": duplicate_rate,
    }


def render_report(root: Path, metrics: RepositoryMetrics, issues: List[Dict[str, object]]) -> str:
    summary = summarise_metrics(metrics)
    tree = build_directory_tree(root)

    report_lines = [
        "# AssemblyChain Core Audit Report",
        "",
        "## Repository Overview",
        "",
        f"* Analysed directory: `{root}`",
        f"* Total files analysed: {summary['files']}",
        f"* Total LOC: {summary['total_loc']}",
        f"* Average LOC per file: {summary['avg_loc']:.1f}",
        f"* Average method complexity: {summary['avg_complexity']:.2f}",
        f"* Maximum method complexity: {summary['max_complexity']}",
        f"* Average documentation density: {summary['avg_doc_ratio']:.2%}",
        f"* Duplicate fragments detected: {summary['duplicate_fragments']}",
        "",
        "### Directory structure",
        "",
        "```",
        tree,
        "```",
        "",
        "## Namespace Dependencies",
        "",
    ]

    if metrics.dependencies:
        for namespace, deps in metrics.dependencies.items():
            report_lines.append(f"* `{namespace}` → {', '.join(f'`{d}`' for d in deps) if deps else '∅'}")
    else:
        report_lines.append("No namespace dependencies detected beyond the standard library.")

    report_lines.extend(["", "## File Metrics", ""])

    for file_metrics in metrics.files:
        report_lines.extend(
            [
                f"### `{file_metrics.path}`",
                "",
                f"* LOC / SLOC: {file_metrics.loc} / {file_metrics.sloc}",
                f"* Classes: {', '.join(file_metrics.classes) if file_metrics.classes else '—'}",
                f"* Methods: {file_metrics.method_count}",
                f"* Total complexity: {file_metrics.complexity_total}",
                f"* Await count: {file_metrics.await_count}",
                f"* Documentation lines: {file_metrics.doc_lines}",
            ]
        )

        if file_metrics.methods:
            report_lines.append("\n| Method | Params | Complexity | Docs | Span |")
            report_lines.append("| --- | --- | --- | --- | --- |")
            for method in file_metrics.methods:
                report_lines.append(
                    f"| {method.name} | {method.parameters} | {method.complexity} | {'✅' if method.doc_present else '⚠️'} | {method.line_span[0]}-{method.line_span[1]} |"
                )
        report_lines.append("")

    report_lines.extend(["## Issues and Risks", ""])

    if issues:
        for issue in issues:
            report_lines.append(f"* **{issue['priority']}** – {issue['type']}: {issue['message']}")
    else:
        report_lines.append("No significant issues detected by automated heuristics.")

    if metrics.duplicate_blocks:
        report_lines.extend(["", "## Duplicate Fragments", ""])
        for block in metrics.duplicate_blocks[:10]:
            report_lines.append("```csharp")
            report_lines.extend(block.lines)
            report_lines.append("```")
            for path, line in block.occurrences:
                report_lines.append(f"  * {path} @ line {line}")
            report_lines.append("")

    report_lines.extend(["## Recommendations", "", "### Architecture", ""])
    report_lines.extend(
        [
            "* Consolidate namespace dependencies to reduce fan-out; enforce clear boundaries between Domain, Graph and Motion layers.",
            "* Introduce interfaces or abstractions where namespaces depend on each other cyclically to break potential cycles.",
        ]
    )

    report_lines.extend(["", "### Module-Level", ""])
    report_lines.extend(
        [
            "* Split oversized files into focused components aligning with single responsibility.",
            "* Share reusable math/helpers via Toolkit namespace to avoid duplication.",
        ]
    )

    report_lines.extend(["", "### Function-Level", ""])
    report_lines.extend(
        [
            "* Refactor high-complexity methods (>10) using guard clauses or extracting helpers.",
            "* Document public APIs with XML summaries to improve discoverability and maintainability.",
        ]
    )

    report_lines.extend(["", "### Engineering", ""])
    report_lines.extend(
        [
            "* Integrate this audit in CI to track metric drift and enforce quality gates.",
            "* Add unit tests for hotspots before refactoring to preserve behaviour.",
        ]
    )

    report_lines.extend(["", "## Roadmap", ""])
    report_lines.extend(
        [
            "1. **Phase 1 – Visibility**: Run audit in CI, baseline metrics, triage P0/P1 issues.",
            "2. **Phase 2 – Stabilise**: Address high-complexity methods and eliminate duplication hotspots.",
            "3. **Phase 3 – Optimise**: Revisit architecture boundaries and implement caching/performance enhancements informed by profiling.",
        ]
    )

    return "\n".join(report_lines)


def run_audit(core_dir: Path, output_dir: Path) -> RepositoryMetrics:
    files = [compute_file_metrics(path) for path in walk_files(core_dir)]
    dependencies = build_dependency_graph(files)
    duplicates = detect_duplicate_blocks(files)
    metrics = RepositoryMetrics(files=files, dependencies=dependencies, duplicate_blocks=duplicates)

    issues = classify_issues(metrics)
    report = render_report(core_dir, metrics, issues)

    output_dir.mkdir(parents=True, exist_ok=True)
    (output_dir / "audit_report.md").write_text(report, encoding="utf-8")
    (output_dir / "audit_report.json").write_text(
        json.dumps(
            {
                "summary": summarise_metrics(metrics),
                "issues": issues,
                "data": metrics.to_json(),
            },
            indent=2,
        )
        + "\n",
        encoding="utf-8",
    )
    return metrics


def parse_args(argv: Optional[Sequence[str]] = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Audit the AssemblyChain Core directory")
    parser.add_argument("core_dir", type=Path, help="Path to the Core directory")
    parser.add_argument(
        "--output",
        type=Path,
        default=Path("reports"),
        help="Directory where the report files will be written",
    )
    return parser.parse_args(argv)


def main(argv: Optional[Sequence[str]] = None) -> None:
    args = parse_args(argv)
    run_audit(args.core_dir, args.output)


if __name__ == "__main__":
    main()
