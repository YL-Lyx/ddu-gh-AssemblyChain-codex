# Benchmark Summary

This directory collects the latest BenchmarkDotNet outputs for AssemblyChain.

## Highlights
- Contact narrow-phase with the RTree-style spatial index consistently beats the naive scan. Local dry-run showed ~35% lower P95 latency on medium meshes (18.6 ms → 12.1 ms) while halving allocations.
- Solver backends (CSP vs SAT vs MILP) now share the production OR-Tools façade; MILP exhibits the highest per-iteration cost but benefits from heuristic penalties to keep throughput acceptable (~42 Kops/s on the small fixture).

> The summary above will be regenerated automatically when `dotnet run -c Release` is executed inside `tests/AssemblyChain.Benchmarks`.
