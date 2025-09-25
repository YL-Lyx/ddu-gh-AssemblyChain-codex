#!/usr/bin/env bash
set -euo pipefail

export PATH="$HOME/.dotnet:$PATH"

# 运行单元测试（不重新 build）
dotnet test tests/AssemblyChain.Core.Tests/AssemblyChain.Core.Tests.csproj -c Release --no-build
