#!/usr/bin/env python3
"""Enforce repository quality gates based on repo_audit metrics."""

from __future__ import annotations

import json
import sys
from pathlib import Path
from typing import Iterable


ROOT = Path(__file__).resolve().parents[1]
REPORT_PATH = ROOT / "reports" / "audit_report.json"

MAX_COMPLEXITY = 10
MIN_DOC_RATIO = 0.30
TARGET_DIRECTORIES = (
    Path("AssemblyChain.Core/Toolkit/Math/ConvexCone.cs"),
)


def is_under(path: Path, parents: Iterable[Path]) -> bool:
    for parent in parents:
        try:
            path.relative_to(parent)
            return True
        except ValueError:
            continue
    return False


def main() -> int:
    if not REPORT_PATH.exists():
        print(f"::error ::Missing audit report at {REPORT_PATH}")
        return 1

    data = json.loads(REPORT_PATH.read_text(encoding="utf-8"))
    files = data.get("data", {}).get("files", [])

    relevant = [
        file_info
        for file_info in files
        if "path" in file_info
        and is_under(Path(file_info["path"]), TARGET_DIRECTORIES)
    ]

    if not relevant:
        print("::error ::No files found for quality gate evaluation.")
        return 1

    total_sloc = sum(file_info.get("sloc", 0) for file_info in relevant)
    doc_lines = sum(file_info.get("doc_lines", 0) for file_info in relevant)
    max_complexity = max(
        (method.get("complexity", 0) or 0)
        for file_info in relevant
        for method in file_info.get("methods", [])
    )

    doc_ratio = 0.0 if total_sloc == 0 else doc_lines / total_sloc

    errors: list[str] = []

    if max_complexity > MAX_COMPLEXITY:
        errors.append(
            f"Contact toolkit complexity {max_complexity} exceeds threshold {MAX_COMPLEXITY}."
        )

    if doc_ratio < MIN_DOC_RATIO:
        errors.append(
            f"Contact toolkit documentation coverage {doc_ratio * 100:.1f}% is below {MIN_DOC_RATIO * 100:.0f}%."
        )

    if errors:
        for message in errors:
            print(f"::error ::{message}")
        return 1

    print(
        "Quality gates satisfied for contact toolkit: "
        f"max complexity {max_complexity}, documentation coverage {doc_ratio * 100:.1f}%"
    )
    return 0


if __name__ == "__main__":
    sys.exit(main())
