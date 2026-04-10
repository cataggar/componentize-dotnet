# componentize-dotnet — AI branch

This is the default branch containing CI workflows.
The source code is on the `zig` branch.

## Branches

| Branch | Purpose |
|--------|---------|
| `ai` (default) | CI workflows only |
| `zig` | Source code with zig toolchain integration |
| `zig-toolchain` | Earlier zig integration work |

## CI Workflow

The `zig-package.yml` workflow:
1. Downloads zig from [ctaggart/zig](https://github.com/ctaggart/zig/releases/tag/v0.15.2)
2. Checks out the `zig` branch source code
3. Builds NativeAOT runtime packages using zig
4. Packages zig as runtime-specific NuGet packages
5. Creates a GitHub release with the packages

**Trigger:** Push a version tag (e.g., `v0.1.0-preview.1`) to create packages.
