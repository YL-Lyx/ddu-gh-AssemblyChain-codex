import json
from collections import Counter, defaultdict
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
AUDIT_PATH = ROOT / "reports" / "audit_report.json"
OUTPUT_PATH = ROOT / "reports" / "comprehensive_codebase_analysis.md"

if not AUDIT_PATH.exists():
    raise SystemExit(f"Expected audit report at {AUDIT_PATH}")

audit = json.loads(AUDIT_PATH.read_text(encoding="utf-8"))
summary = audit.get("summary", {})
files = audit.get("data", {}).get("files", [])
dependencies = audit.get("data", {}).get("dependencies", {})
dependency_cycles = audit.get("data", {}).get("dependency_cycles", [])
duplicates = audit.get("data", {}).get("duplicates", [])
issues = audit.get("issues", [])
recs = audit.get("recommendations", {})


def build_tree(base: Path, max_depth: int = 3) -> str:
    """Return a lightweight tree (directories only)."""

    def render(node: Path, depth: int) -> list[str]:
        if depth >= max_depth:
            return []
        children = sorted([p for p in node.iterdir() if p.is_dir()])
        lines: list[str] = []
        for index, child in enumerate(children):
            connector = "└── " if index == len(children) - 1 else "├── "
            prefix = "    " * depth
            if depth == 0:
                lines.append(child.name)
            else:
                lines.append(f"{prefix}{connector}{child.name}")
            if depth + 1 < max_depth:
                for sub_line in render(child, depth + 1):
                    lines.append(f"{prefix}{'    ' if index == len(children) - 1 else '│   '}{sub_line}")
        return lines

    if not base.exists():
        return f"(missing) {base}"

    header = base.name
    body = render(base, 0)
    return "\n".join([header] + body if body else [header])


namespace_metrics: dict[str, dict[str, float]] = {}
for file_info in files:
    namespace = file_info.get("namespace", "") or "<global>"
    bucket = namespace_metrics.setdefault(
        namespace,
        {
            "files": 0,
            "sloc": 0,
            "doc_lines": 0,
            "methods": 0,
            "doc_methods": 0,
            "complexity_sum": 0,
            "max_complexity": 0,
            "max_params": 0,
        },
    )
    bucket["files"] += 1
    bucket["sloc"] += file_info.get("sloc", 0)
    bucket["doc_lines"] += file_info.get("doc_lines", 0)
    methods = file_info.get("methods", [])
    bucket["methods"] += len(methods)
    for method in methods:
        if method.get("doc_present"):
            bucket["doc_methods"] += 1
        complexity = method.get("complexity", 0) or 0
        bucket["complexity_sum"] += complexity
        bucket["max_complexity"] = max(bucket["max_complexity"], complexity)
        bucket["max_params"] = max(bucket["max_params"], method.get("parameters", 0) or 0)

all_methods = []
for file_info in files:
    for method in file_info.get("methods", []):
        all_methods.append(
            {
                "namespace": file_info.get("namespace"),
                "file": file_info.get("path"),
                "name": method.get("name"),
                "parameters": method.get("parameters", 0),
                "complexity": method.get("complexity", 0),
                "length": method.get("length", 0),
                "max_nesting": method.get("max_nesting", 0),
                "doc_present": method.get("doc_present", False),
            }
        )

top_complex_methods = sorted(all_methods, key=lambda m: (-m["complexity"], -m["length"]))[:12]
top_long_methods = sorted(all_methods, key=lambda m: (-m["length"], -m["complexity"]))[:12]

namespace_rows = []
for namespace, bucket in namespace_metrics.items():
    methods = bucket["methods"]
    avg_complexity = (bucket["complexity_sum"] / methods) if methods else 0
    doc_ratio = (bucket["doc_methods"] / methods) if methods else 0
    namespace_rows.append(
        (
            namespace,
            bucket["files"],
            bucket["sloc"],
            methods,
            avg_complexity,
            doc_ratio,
            bucket["max_complexity"],
            bucket["max_params"],
        )
    )

namespace_rows.sort(key=lambda row: row[2], reverse=True)

edge_counter = Counter()
for origin, targets in dependencies.items():
    origin_key = ".".join(origin.split(".")[:3]) or origin
    for target in targets:
        target_key = ".".join(target.split(".")[:3]) or target
        if origin_key == target_key:
            continue
        edge_counter[(origin_key, target_key)] += 1

top_edges = edge_counter.most_common(12)
mermaid_lines = ["graph LR"]
seen_nodes = set()
for (src, dst), weight in top_edges:
    src_id = src.replace(".", "_")
    dst_id = dst.replace(".", "_")
    seen_nodes.update([src_id, dst_id])
    label = f" {weight}" if weight > 1 else ""
    mermaid_lines.append(f"    {src_id}({src}) --> {dst_id}({dst}{label})")
if not top_edges:
    mermaid_lines.append("    A --> B")
mermaid_graph = "\n".join(mermaid_lines)

severity_groups: dict[str, list[dict[str, str]]] = defaultdict(list)
for issue in issues:
    severity_groups[issue.get("priority", "UNSPEC")].append(issue)

for group in severity_groups.values():
    group.sort(key=lambda item: item.get("type", ""))

priority_order = ["P0", "P1", "P2", "UNSPEC"]
ordered_priorities = [p for p in priority_order if p in severity_groups]

duplicates_sorted = sorted(duplicates, key=lambda d: len(d.get("occurrences", [])), reverse=True)[:10]

md_lines: list[str] = []
md_lines.append("# AssemblyChain 代码库综合分析报告")
md_lines.append("")
md_lines.append("> 基于 `reports/audit_report.json` 自动生成，覆盖仓库中的 C# 源码文件。")
md_lines.append("")
md_lines.append("## 1. 目录与模块结构")
md_lines.append("")
md_lines.append("### 1.1 顶层目录概览")
md_lines.append("")
root_dirs = sorted(
    [p for p in ROOT.iterdir() if p.is_dir() and p.name not in {".git", "artifacts", ".config", ".nuget"}],
    key=lambda p: p.name,
)
for directory in root_dirs:
    md_lines.append(f"- `{directory.name}`")
md_lines.append("")
md_lines.append("### 1.2 src 目录结构（深度≤3）")
md_lines.append("")
md_lines.append("```text")
md_lines.append(build_tree(ROOT / "src", max_depth=3))
md_lines.append("```")
md_lines.append("")
if (ROOT / "tests").exists():
    md_lines.append("### 1.3 tests 目录结构（深度≤2）")
    md_lines.append("")
    md_lines.append("```text")
    md_lines.append(build_tree(ROOT / "tests", max_depth=2))
    md_lines.append("```")
    md_lines.append("")

md_lines.append("## 2. 概览指标")
md_lines.append("")
md_lines.append("| 指标 | 数值 | 说明 |")
md_lines.append("| --- | ---: | --- |")
md_lines.append(f"| 源文件数 | {summary.get('files', 'N/A')} | 参与分析的 C# 文件 |")
md_lines.append(
    f"| 总有效代码行 | {summary.get('total_sloc', summary.get('total_loc', 'N/A'))} | 去除空行与注释后的统计 |"
)
md_lines.append(f"| 平均方法复杂度 | {summary.get('avg_complexity', 0):.2f} | McCabe 近似值 |")
md_lines.append(f"| 最大方法复杂度 | {summary.get('max_complexity', 0)} | 重构优先级参考 |")
md_lines.append(f"| 平均文档覆盖率 | {summary.get('avg_doc_ratio', 0) * 100:.1f}% | XML 注释行 / SLOC |")
md_lines.append(f"| 异步方法数 | {summary.get('async_method_count', 0)} | Task/async 相关 |")
md_lines.append("")

md_lines.append("## 3. 模块化与命名空间指标")
md_lines.append("")
md_lines.append("| 命名空间 | 文件数 | SLOC | 方法数 | 平均复杂度 | 文档覆盖率 | 最复杂方法 | 最大参数数 |")
md_lines.append("| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |")
for row in namespace_rows[:20]:
    namespace, files_count, sloc, methods, avg_complexity, doc_ratio, max_complexity, max_params = row
    md_lines.append(
        f"| `{namespace}` | {files_count} | {sloc} | {methods} | {avg_complexity:.2f} | {doc_ratio*100:.1f}% | {max_complexity} | {max_params} |"
    )
md_lines.append("")
md_lines.append("> 注：仅展示 SLOC 前 20 的命名空间，完整数据可在 `reports/audit_report.json` 中获取。")
md_lines.append("")

md_lines.append("## 4. 关键文件与函数")
md_lines.append("")
md_lines.append("### 4.1 高复杂度函数 (Top 12)")
md_lines.append("")
md_lines.append("| 函数 | 所在文件 | 复杂度 | 行数 | 最大嵌套 | 参数数 | 文档 |")
md_lines.append("| --- | --- | ---: | ---: | ---: | ---: | --- |")
for method in top_complex_methods:
    md_lines.append(
        f"| `{method['name']}` (`{method['namespace']}`) | `{method['file']}` | {method['complexity']} | {method['length']} | {method['max_nesting']} | {method['parameters']} | {'✅' if method['doc_present'] else '❌'} |"
    )
md_lines.append("")

md_lines.append("### 4.2 超长函数 (Top 12)")
md_lines.append("")
md_lines.append("| 函数 | 所在文件 | 行数 | 复杂度 | 最大嵌套 | 参数数 | 文档 |")
md_lines.append("| --- | --- | ---: | ---: | ---: | ---: | --- |")
for method in top_long_methods:
    md_lines.append(
        f"| `{method['name']}` (`{method['namespace']}`) | `{method['file']}` | {method['length']} | {method['complexity']} | {method['max_nesting']} | {method['parameters']} | {'✅' if method['doc_present'] else '❌'} |"
    )
md_lines.append("")

md_lines.append("## 5. 依赖关系分析")
md_lines.append("")
md_lines.append("### 5.1 主要命名空间依赖图 (Top 12 边)")
md_lines.append("")
md_lines.append("```mermaid")
md_lines.append(mermaid_graph)
md_lines.append("```")
md_lines.append("")

md_lines.append("### 5.2 循环依赖")
md_lines.append("")
if dependency_cycles:
    for cycle in dependency_cycles:
        md_lines.append(f"- {' → '.join(cycle)}")
else:
    md_lines.append("- 未检测到循环依赖")
md_lines.append("")

md_lines.append("## 6. 重复与可维护性风险")
md_lines.append("")
if duplicates_sorted:
    md_lines.append("### 6.1 代码重复片段")
    md_lines.append("")
    md_lines.append("| 片段哈希 | 出现次数 | 示例文件 | 上下文预览 |")
    md_lines.append("| --- | ---: | --- | --- |")
    for dup in duplicates_sorted:
        snippet = "<br/>".join(dup.get("lines", [])[:6])
        occurrences = dup.get("occurrences", [])
        example = occurrences[0] if occurrences else {}
        file_path = example.get("file", "-")
        count = len(occurrences)
        md_lines.append(f"| `{dup.get('hash')}` | {count} | `{file_path}` | `{snippet}` |")
    md_lines.append("")
else:
    md_lines.append("### 6.1 代码重复片段")
    md_lines.append("")
    md_lines.append("- 未检测到显著重复。")
    md_lines.append("")

md_lines.append("## 7. 问题清单")
md_lines.append("")
for index, severity in enumerate(ordered_priorities, start=1):
    group = severity_groups[severity]
    md_lines.append(f"### 7.{index} 严重级别 {severity}")
    md_lines.append("")
    for issue in group:
        md_lines.append(f"- **{issue.get('type')}**：{issue.get('message')}")
    md_lines.append("")

md_lines.append("## 8. 建议与下一步")
md_lines.append("")
for idx, (category, items) in enumerate(recs.items(), start=1):
    md_lines.append(f"### 8.{idx} {category.title()} 层面")
    md_lines.append("")
    for item in items:
        md_lines.append(f"- {item}")
    md_lines.append("")

OUTPUT_PATH.write_text("\n".join(md_lines), encoding="utf-8")
print(f"Report written to {OUTPUT_PATH}")
