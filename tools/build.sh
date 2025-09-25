#!/usr/bin/env bash
set -euo pipefail

export PATH="$HOME/.dotnet:$PATH"

# 构建解决方案，输出到 build/bin
dotnet build AssemblyChain-Core.sln -c Release -v minimal -o build/bin
