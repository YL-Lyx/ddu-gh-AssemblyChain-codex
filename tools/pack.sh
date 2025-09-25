#!/usr/bin/env bash
set -euo pipefail

export PATH="$HOME/.dotnet:$PATH"

# 确保先 build
bash tools/build.sh

# 打包输出
PACKAGE_DIR="dist"
rm -rf "$PACKAGE_DIR"
mkdir -p "$PACKAGE_DIR"

cp build/bin/AssemblyChain.Gh.gha "$PACKAGE_DIR"/
cp build/bin/*.dll "$PACKAGE_DIR"/

echo "✅ Package ready in $PACKAGE_DIR/"
