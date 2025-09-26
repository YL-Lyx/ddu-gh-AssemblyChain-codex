#!/usr/bin/env bash
set -euo pipefail

SLN="AssemblyChain-Core.sln"
SRC="src"
TARGET_ROOT="src/AssemblyChain"

PROJECTS=(
  "AssemblyChain.Core"
  "AssemblyChain.Grasshopper"
  "AssemblyChain.Geometry.Abstractions"
  "AssemblyChain.Geometry"
  "AssemblyChain.Constraints"
  "AssemblyChain.Graphs"
  "AssemblyChain.Planning"
  "AssemblyChain.Analysis"
  "AssemblyChain.IO"
  "AssemblyChain.Robotics"
)

echo "== Verify solution =="
test -f "$SLN" || { echo "Solution not found: $SLN"; exit 1; }

echo "== Create target root =="
mkdir -p "$TARGET_ROOT"

HAS_DOTNET=0
if command -v dotnet >/dev/null 2>&1; then
  HAS_DOTNET=1
else
  echo "Warn: dotnet CLI not found, skipping automatic solution updates and build check."
fi

echo "== Remove old project paths from solution =="
if [ "$HAS_DOTNET" -eq 1 ]; then
  for P in "${PROJECTS[@]}"; do
    if [ -f "$SRC/$P/$P.csproj" ]; then
      dotnet sln "$SLN" remove "$SRC/$P/$P.csproj" || true
    fi
  done
else
  echo "Skipping because dotnet CLI is unavailable."
fi

echo "== Move directories with git mv (preserve history) =="
for P in "${PROJECTS[@]}"; do
  SRC_DIR="$SRC/$P"
  DST_DIR="$TARGET_ROOT/$(echo "$P" | sed 's/^AssemblyChain\.//')"
  if [ -d "$SRC_DIR" ]; then
    mkdir -p "$(dirname "$DST_DIR")"
    echo "Moving $SRC_DIR -> $DST_DIR"
    if ! git mv -k "$SRC_DIR" "$DST_DIR"; then
      echo "git mv failed for $P, fallback to mv"
      mv "$SRC_DIR" "$DST_DIR"
      git add "$DST_DIR"
    fi
  else
    echo "Skip (not found): $SRC_DIR"
  fi
done

echo "== Re-add projects to solution =="
if [ "$HAS_DOTNET" -eq 1 ]; then
  for P in "${PROJECTS[@]}"; do
    NEW_DIR="$TARGET_ROOT/$(echo "$P" | sed 's/^AssemblyChain\.//')"
    CSPROJ="$NEW_DIR/$P.csproj"
    if [ -f "$CSPROJ" ]; then
      dotnet sln "$SLN" add "$CSPROJ"
    else
      echo "Warn: csproj missing -> $CSPROJ"
    fi
  done
else
  echo "Skipping because dotnet CLI is unavailable."
fi

echo "== Build check =="
if [ "$HAS_DOTNET" -eq 1 ]; then
  dotnet build "$SLN" -c Release
else
  echo "Skipping build because dotnet CLI is unavailable."
fi

echo "== Done =="
echo "New layout under: $TARGET_ROOT"
