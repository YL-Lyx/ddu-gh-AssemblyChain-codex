#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")"/.. && pwd)"
SOLUTION="$ROOT_DIR/AssemblyChain-Core.sln"

echo "Running dotnet format on $SOLUTION"
dotnet format "$SOLUTION" "$@"
