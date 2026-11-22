# vs-extension
Visual Studio Extension

## Supported Visual Studio versions

The extension installs in Visual Studio 2022 (17.x) and Visual Studio 2026 (18.x),
Community, Professional and Enterprise, x64.

## Building

```
msbuild HybridizerExtension.sln /restore /p:Configuration=Release
```

Use the MSBuild of Visual Studio 2022 or 2026, with the "Visual Studio extension
development" workload. The VSIX lands in `src/HybridizerExtension/bin/Release/net472/`.
CI builds it on `windows-2022` (VS 2022) and `windows-2025` (VS 2026).

## C++ compiler used for CUDA

"Hybridize Project" writes a `Directory.Build.targets` that runs nvcc with a
Visual Studio C++ toolset. nvcc refuses an MSVC newer than its CUDA release
knows: CUDA 13.0 takes the VS 2022 toolset (MSVC 14.4x) but not the one
Visual Studio 2026 installs by default (14.5x).

The build reads that limit from `%CUDA_PATH%\include\crt\host_config.h` and
loads the newest installed toolset under it, from any Visual Studio 2022 or
later. With Visual Studio 2026 only, add the component
"MSVC v143 - VS 2022 C++ x64/x86 build tools".

Properties a project can set:

| Property | Effect |
|---|---|
| `VcVarsAllBat` | Use this `vcvarsall.bat` instead of looking for one |
| `VcVarsVer` | Toolset passed as `-vcvars_ver` (for example `14.44`) |
| `HybridizerAllowUnsupportedCompiler` | `true`: when no toolset is under the limit, use the newest one and pass `-allow-unsupported-compiler` to nvcc |
