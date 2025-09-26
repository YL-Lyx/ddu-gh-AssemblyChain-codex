# Robot Bridge Toolkit

This note describes how to inspect `process.json` files produced by AssemblyChain and generate a simple URScript preview without accessing hardware.

## Quick Python Preview
```python
import json
from pathlib import Path

def preview_urscript(process_path: str) -> None:
    process = json.loads(Path(process_path).read_text(encoding="utf-8"))
    print(f"# Schema version: {process.get('schemaVersion', 'unknown')}")
    for step in process.get("steps", []):
        part = step.get("partId", "part")
        direction = step.get("direction", [0.0, 0.0, 1.0])
        vector = ", ".join(f"{component:.3f}" for component in direction)
        print(f"# Move for {part}")
        print(f"movej(p[{vector}, 0.0, 0.0], a=1.2, v=0.25)")

if __name__ == "__main__":
    preview_urscript("SAMPLES/Case03/output/process.json")
```

Running the script prints a sequence of `movej` commands that mirror the directions embedded in the process file. Swap the path to point at other exports as needed.

> The script is intentionally lightweight—no UR controller connection or additional dependencies are required.
