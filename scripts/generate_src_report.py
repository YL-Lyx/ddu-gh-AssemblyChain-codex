"""Generate a comprehensive analysis report for the `src` tree.

The script walks all C# files under `src/` and computes a variety of
lightweight static-analysis metrics so that we can build an actionable
overview of the code base.  The resulting Markdown report is written to
`reports/src_code_analysis.md` and contains:

* Directory layout rendered as a tree.
* File-level metrics (lines of code, comment density, number of classes,
  methods, estimated cyclomatic complexity, documentation coverage, etc.).
* Method-level metrics (line count, parameter count, estimated complexity,
  documentation coverage).
* Project-level dependency graph (derived from `using` directives).
* Automatically detected potential issues such as large/complex methods,
  missing XML documentation, and duplicate method implementations.

The metrics are heuristics – the goal is breadth of coverage, not
perfection.  The script keeps parsing logic intentionally forgiving so that
it can cope with the variety of coding styles present in the repository.
"""

from __future__ import annotations

import collections
import dataclasses
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SRC_DIR = ROOT / "src"
REPORT_PATH = ROOT / "reports" / "src_code_analysis.md"


METHOD_PATTERN = re.compile(
    r"""
    ^\s*
    (?:\[[^\]]+\]\s*)*                # attributes
    (?:public|private|protected|internal|static|virtual|override|
       async|sealed|extern|partial|unsafe|new|abstract)\s+
    [^;=\n]+?                            # return type / signature
    (\w+)\s*                            # method or ctor name (capture)
    \(([^)]*)\)\s*                      # parameter list (capture)
    (?:where[^\{;]+)?                    # generic constraints
    (?:\{|=>)                            # block or expression body
    """,
    re.MULTILINE | re.VERBOSE,
)


CLASS_PATTERN = re.compile(r"^\s*(?:public|internal|sealed|abstract|static|partial\s+)*class\s+(\w+)", re.MULTILINE)
STRUCT_PATTERN = re.compile(r"^\s*(?:public|internal|readonly|ref|partial\s+)*struct\s+(\w+)", re.MULTILINE)
INTERFACE_PATTERN = re.compile(r"^\s*(?:public|internal|partial\s+)*interface\s+(\w+)", re.MULTILINE)
ENUM_PATTERN = re.compile(r"^\s*(?:public|internal|flags\s+)*enum\s+(\w+)", re.MULTILINE)


CONTROL_KEYWORDS = re.compile(r"\b(if|for|foreach|while|case|default|catch|when|&&|\|\||\?|goto|return)\b")


@dataclasses.dataclass
class MethodMetrics:
    name: str
    parameters: int
    start_line: int
    loc: int
    complexity: int
    has_docs: bool
    is_expression: bool
    body_offset: int


@dataclasses.dataclass
class FileMetrics:
    path: Path
    total_lines: int
    code_lines: int
    comment_lines: int
    blank_lines: int
    class_count: int
    struct_count: int
    interface_count: int
    enum_count: int
    method_metrics: list[MethodMetrics]
    doc_line_ratio: float
    dependencies: set[str]


def read_text(path: Path) -> str:
    try:
        return path.read_text(encoding="utf-8")
    except UnicodeDecodeError:
        return path.read_text(encoding="utf-8", errors="ignore")


def compute_line_metrics(text: str) -> tuple[int, int, int, int]:
    total = 0
    blank = 0
    comment = 0
    in_block = False
    for raw_line in text.splitlines():
        total += 1
        line = raw_line.strip()
        if not line:
            blank += 1
            continue
        if in_block:
            comment += 1
            if "*/" in line:
                in_block = False
            continue
        if line.startswith("/*"):
            comment += 1
            if "*/" not in line:
                in_block = True
            continue
        if line.startswith("//") or line.startswith("///"):
            comment += 1
            continue
    code = total - blank - comment
    return total, code, comment, blank


def extract_method_body(text: str, start_index: int) -> tuple[str, bool]:
    """Return the method body text and a flag for expression-bodied members."""
    brace_index = text.find("{", start_index)
    arrow_index = text.find("=>", start_index, text.find("\n", start_index) if text.find("\n", start_index) != -1 else None)
    if arrow_index != -1 and (brace_index == -1 or arrow_index < brace_index):
        # Expression-bodied member
        line_end = text.find("\n", arrow_index)
        if line_end == -1:
            line_end = len(text)
        return text[arrow_index:line_end], True
    if brace_index == -1:
        return "", False
    depth = 0
    idx = brace_index
    while idx < len(text):
        char = text[idx]
        if char == "{":
            depth += 1
        elif char == "}":
            depth -= 1
            if depth == 0:
                return text[brace_index + 1 : idx], False
        idx += 1
    return text[brace_index + 1 :], False


def estimate_complexity(body: str) -> int:
    if not body:
        return 1
    return 1 + len(CONTROL_KEYWORDS.findall(body))


def has_documentation(lines: list[str], start_line: int) -> bool:
    idx = start_line - 2  # zero-indexed lines, check line above signature
    while idx >= 0:
        line = lines[idx].strip()
        if not line:
            idx -= 1
            continue
        return line.startswith("///")
    return False


def analyze_methods(text: str) -> list[MethodMetrics]:
    lines = text.splitlines()
    method_metrics: list[MethodMetrics] = []
    for match in METHOD_PATTERN.finditer(text):
        method_name = match.group(1)
        params = match.group(2).strip()
        param_count = 0
        if params:
            # Ignore params inside angle brackets by replacing generics with placeholder
            cleaned = re.sub(r"<[^>]+>", "", params)
            param_count = sum(1 for part in cleaned.split(",") if part.strip())
        start_line = text.count("\n", 0, match.start()) + 1
        body_offset = match.end()
        body, is_expression = extract_method_body(text, body_offset)
        body_lines = [line for line in body.splitlines() if line.strip()]
        loc = len(body_lines)
        complexity = estimate_complexity(body)
        documented = has_documentation(lines, start_line)
        method_metrics.append(
            MethodMetrics(
                name=method_name,
                parameters=param_count,
                start_line=start_line,
                loc=loc,
                complexity=complexity,
                has_docs=documented,
                is_expression=is_expression,
                body_offset=body_offset,
            )
        )
    return method_metrics


def analyze_file(path: Path) -> FileMetrics:
    text = read_text(path)
    total, code, comment, blank = compute_line_metrics(text)
    classes = len(CLASS_PATTERN.findall(text))
    structs = len(STRUCT_PATTERN.findall(text))
    interfaces = len(INTERFACE_PATTERN.findall(text))
    enums = len(ENUM_PATTERN.findall(text))
    methods = analyze_methods(text)
    doc_lines = sum(1 for line in text.splitlines() if line.strip().startswith("///"))
    doc_ratio = doc_lines / total if total else 0.0
    dependencies = {
        line.split()[1].rstrip(";")
        for line in text.splitlines()
        if line.strip().startswith("using ") and "AssemblyChain" in line
    }
    return FileMetrics(
        path=path,
        total_lines=total,
        code_lines=code,
        comment_lines=comment,
        blank_lines=blank,
        class_count=classes,
        struct_count=structs,
        interface_count=interfaces,
        enum_count=enums,
        method_metrics=methods,
        doc_line_ratio=doc_ratio,
        dependencies=dependencies,
    )


def directory_tree(root: Path) -> str:
    lines: list[str] = []

    def walk(path: Path, prefix: str = "") -> None:
        entries = sorted(path.iterdir(), key=lambda p: (p.is_file(), p.name.lower()))
        for index, entry in enumerate(entries):
            connector = "└── " if index == len(entries) - 1 else "├── "
            lines.append(f"{prefix}{connector}{entry.name}")
            if entry.is_dir():
                extension = "    " if index == len(entries) - 1 else "│   "
                walk(entry, prefix + extension)

    lines.append(root.name)
    walk(root)
    return "\n".join(lines)


def summarize_methods(methods: list[MethodMetrics]) -> tuple[float, float, float]:
    if not methods:
        return 0.0, 0.0, 0.0
    avg_loc = sum(m.loc for m in methods) / len(methods)
    avg_params = sum(m.parameters for m in methods) / len(methods)
    avg_complexity = sum(m.complexity for m in methods) / len(methods)
    return avg_loc, avg_params, avg_complexity


def format_percentage(value: float) -> str:
    return f"{value * 100:.1f}%"


def collect_metrics() -> list[FileMetrics]:
    metrics: list[FileMetrics] = []
    for path in sorted(SRC_DIR.rglob("*.cs")):
        metrics.append(analyze_file(path))
    return metrics


def build_dependency_graph(metrics: list[FileMetrics]) -> dict[str, set[str]]:
    graph: dict[str, set[str]] = collections.defaultdict(set)
    for file_metrics in metrics:
        project = file_metrics.path.relative_to(SRC_DIR).parts[0]
        for dependency in file_metrics.dependencies:
            parts = dependency.split(".")
            target = ".".join(parts[:2]) if len(parts) >= 2 else dependency
            if target and target != project:
                graph[project].add(target)
    return graph


def detect_duplicates(metrics: list[FileMetrics]) -> dict[str, list[tuple[Path, int]]]:
    seen: dict[str, list[tuple[Path, int]]] = collections.defaultdict(list)
    duplicates: dict[str, list[tuple[Path, int]]] = {}
    for file_metrics in metrics:
        text = read_text(file_metrics.path)
        for method in file_metrics.method_metrics:
            body, _ = extract_method_body(text, method.body_offset)
            normalized = re.sub(r"\s+", " ", body.strip())
            if len(normalized) < 40:
                continue
            fingerprint = normalized.lower()
            seen[fingerprint].append((file_metrics.path, method.start_line))
    for fingerprint, locations in seen.items():
        if len(locations) > 1:
            key = fingerprint[:80] + ("…" if len(fingerprint) > 80 else "")
            duplicates[key] = locations
    return duplicates


def classify_severity(method: MethodMetrics) -> str:
    if method.loc >= 60 or method.complexity >= 15 or method.parameters >= 6:
        return "High"
    if method.loc >= 35 or method.complexity >= 8 or method.parameters >= 4:
        return "Medium"
    return "Low"


def build_issue_list(metrics: list[FileMetrics]) -> dict[str, list[str]]:
    issues: dict[str, list[str]] = {"High": [], "Medium": [], "Low": []}
    for file_metrics in metrics:
        rel_path = file_metrics.path.relative_to(ROOT)
        for method in file_metrics.method_metrics:
            severity = classify_severity(method)
            if severity == "Low":
                continue
            descriptor = (
                f"{rel_path}::{method.name} (LOC={method.loc}, params={method.parameters}, "
                f"complexity≈{method.complexity})"
            )
            issues[severity].append(descriptor)
        if file_metrics.doc_line_ratio < 0.02:
            issues["Medium"].append(f"{rel_path} – missing XML documentation ({format_percentage(file_metrics.doc_line_ratio)} coverage)")
        if file_metrics.comment_lines / max(file_metrics.total_lines, 1) < 0.05 and file_metrics.total_lines > 200:
            issues["Medium"].append(
                f"{rel_path} – large file ({file_metrics.total_lines} lines) with sparse comments"
            )
    return issues


def render_dependency_graph(graph: dict[str, set[str]]) -> str:
    lines = ["```mermaid", "graph TD"]
    if not graph:
        lines.append("    EMPTY[No dependencies detected]")
    else:
        for source, targets in sorted(graph.items()):
            if not targets:
                lines.append(f"    {source}")
            for target in sorted(targets):
                lines.append(f"    {source} --> {target}")
    lines.append("```")
    return "\n".join(lines)


def render_file_table(metrics: list[FileMetrics]) -> str:
    headers = [
        "File",
        "LOC",
        "Code",
        "Comments",
        "Classes",
        "Structs",
        "Interfaces",
        "Enums",
        "Methods",
        "Doc%",
    ]
    rows = ["| " + " | ".join(headers) + " |"]
    rows.append("|" + " --- |" * len(headers))
    for fm in metrics:
        rel = fm.path.relative_to(ROOT)
        rows.append(
            "| "
            + " | ".join(
                [
                    f"`{rel}`",
                    str(fm.total_lines),
                    str(fm.code_lines),
                    str(fm.comment_lines),
                    str(fm.class_count),
                    str(fm.struct_count),
                    str(fm.interface_count),
                    str(fm.enum_count),
                    str(len(fm.method_metrics)),
                    format_percentage(fm.doc_line_ratio),
                ]
            )
            + " |"
        )
    return "\n".join(rows)


def render_method_details(metrics: list[FileMetrics]) -> str:
    sections = []
    for fm in metrics:
        if not fm.method_metrics:
            continue
        rel = fm.path.relative_to(ROOT)
        sections.append(f"#### `{rel}`")
        sections.append(
            "| Method | Start Line | LOC | Params | Complexity≈ | Docs |"
        )
        sections.append("| --- | --- | --- | --- | --- | --- |")
        for method in sorted(fm.method_metrics, key=lambda m: m.start_line):
            sections.append(
                "| "
                + method.name
                + " | "
                + str(method.start_line)
                + " | "
                + str(method.loc)
                + " | "
                + str(method.parameters)
                + " | "
                + str(method.complexity)
                + " | "
                + ("✅" if method.has_docs else "❌")
                + " |"
            )
        sections.append("")
    return "\n".join(sections)


def render_issues(issues: dict[str, list[str]]) -> str:
    lines = []
    for severity in ("High", "Medium", "Low"):
        entries = issues.get(severity, [])
        lines.append(f"### {severity} Severity")
        if not entries:
            lines.append("- None detected")
        else:
            for entry in sorted(entries):
                lines.append(f"- {entry}")
        lines.append("")
    return "\n".join(lines)


def render_duplicates(duplicates: dict[str, list[tuple[Path, int]]]) -> str:
    if not duplicates:
        return "No potentially duplicated method bodies detected."
    lines = []
    for fingerprint, locations in duplicates.items():
        lines.append(f"- `{fingerprint}`")
        for path, line in locations:
            rel = path.relative_to(ROOT)
            lines.append(f"  - `{rel}` (line {line})")
    return "\n".join(lines)


def build_report() -> str:
    metrics = collect_metrics()
    dependency_graph = build_dependency_graph(metrics)
    duplicates = detect_duplicates(metrics)
    issues = build_issue_list(metrics)

    total_files = len(metrics)
    total_loc = sum(fm.code_lines for fm in metrics)
    total_methods = sum(len(fm.method_metrics) for fm in metrics)
    avg_doc_ratio = sum(fm.doc_line_ratio for fm in metrics) / total_files if total_files else 0.0
    avg_method_loc = (
        sum(method.loc for fm in metrics for method in fm.method_metrics) / total_methods
        if total_methods
        else 0.0
    )
    avg_method_complexity = (
        sum(method.complexity for fm in metrics for method in fm.method_metrics) / total_methods
        if total_methods
        else 0.0
    )
    high_issue_count = len(issues.get("High", []))

    top_long_methods = sorted(
        (
            (fm.path.relative_to(ROOT), method)
            for fm in metrics
            for method in fm.method_metrics
        ),
        key=lambda item: item[1].loc,
        reverse=True,
    )[:10]

    report_parts = []
    report_parts.append("# AssemblyChain Source Analysis Report\n")
    report_parts.append("Generated automatically by `scripts/generate_src_report.py`.\n")

    report_parts.append("## Summary Metrics\n")
    report_parts.append("- **Source files analyzed:** {}".format(total_files))
    report_parts.append("- **Total effective lines of code:** {}".format(total_loc))
    report_parts.append("- **Methods discovered:** {}".format(total_methods))
    report_parts.append("- **Average XML doc coverage:** {}".format(format_percentage(avg_doc_ratio)))
    report_parts.append("- **Average method LOC:** {:.1f}".format(avg_method_loc))
    report_parts.append("- **Average method complexity (heuristic):** {:.1f}".format(avg_method_complexity))
    report_parts.append("- **High severity issues flagged:** {}".format(high_issue_count))
    report_parts.append("")

    report_parts.append("## Directory Structure\n")
    report_parts.append("```\n" + directory_tree(SRC_DIR) + "\n```\n")

    report_parts.append("## Project Dependency Graph\n")
    report_parts.append(render_dependency_graph(dependency_graph) + "\n")

    report_parts.append("## File-Level Metrics\n")
    report_parts.append(render_file_table(metrics) + "\n")

    report_parts.append("## Method-Level Details\n")
    report_parts.append(render_method_details(metrics) + "\n")

    report_parts.append("## Notable Hotspots\n")
    if top_long_methods:
        report_parts.append("Top 10 longest methods (by non-blank LOC):")
        for rel_path, method in top_long_methods:
            report_parts.append(
                f"- `{rel_path}`::{method.name} – LOC={method.loc}, params={method.parameters}, complexity≈{method.complexity}"
            )
        report_parts.append("")
    else:
        report_parts.append("No methods detected.\n")

    report_parts.append("## Identified Issues\n")
    report_parts.append(render_issues(issues) + "\n")

    report_parts.append("## Potential Duplicate Implementations\n")
    report_parts.append(render_duplicates(duplicates) + "\n")

    report_parts.append("## Actionable Recommendations\n")
    report_parts.append("### Architecture\n")
    report_parts.append(
        "- Review dependency edges that cross project boundaries in the Mermaid graph to confirm that layering rules are respected.\n"
        "- Consider introducing dedicated abstraction interfaces where multiple modules reference the same concrete implementation.\n"
    )
    report_parts.append("### Code Quality\n")
    report_parts.append(
        "- Refactor methods highlighted as high severity to split responsibilities and reduce cyclomatic complexity.\n"
        "- Increase XML documentation coverage for public APIs, targeting files flagged with low doc ratios.\n"
        "- Normalize naming and access modifiers across partial classes to reinforce intent.\n"
    )
    report_parts.append("### Performance\n")
    report_parts.append(
        "- Inspect hotspot methods for repeated geometric computations and introduce caching where feasible.\n"
        "- Evaluate solver backends for opportunities to batch I/O operations and reduce repeated setup overhead.\n"
    )
    report_parts.append("### Quality Assurance\n")
    report_parts.append(
        "- Expand automated tests around complex motion/solver routines to prevent regressions.\n"
        "- Integrate static analysis (StyleCop, analyzers) into CI to enforce documentation and complexity thresholds.\n"
    )

    return "\n".join(report_parts)


def main() -> None:
    report = build_report()
    REPORT_PATH.parent.mkdir(parents=True, exist_ok=True)
    REPORT_PATH.write_text(report, encoding="utf-8")
    print(f"Report written to {REPORT_PATH}")


if __name__ == "__main__":
    main()
