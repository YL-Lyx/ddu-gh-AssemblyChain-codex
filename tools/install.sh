#!/usr/bin/env bash
set -euo pipefail

# 安装 .NET 7 SDK
curl -L https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
bash dotnet-install.sh --channel 7.0
export PATH="$HOME/.dotnet:$PATH"

# 还原依赖
dotnet restore AssemblyChain-Core.sln
