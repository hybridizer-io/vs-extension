# Hybridizer Visual Studio Extension - Design Plan

## Overview

A Visual Studio 2022+ (VSIX) extension that integrates the Hybridizer toolchain into the IDE. The extension provides project creation, build integration, and opt-in telemetry.

---

## 1. Project Structure

```
vs-extension/
├── HybridizerExtension.sln
├── src/
│   └── HybridizerExtension/
│       ├── HybridizerExtension.csproj          # VSIX project (net472 + VS SDK)
│       ├── source.extension.vsixmanifest        # Extension metadata
│       ├── HybridizerExtensionPackage.cs        # Main VS package entry point
│       ├── Resources/
│       │   ├── HybridizerExtensionPackage.ico
│       │   └── Altimesh128x128.png
│       ├── Commands/
│       │   ├── HybridizeProjectCommand.cs       # "Hybridize Project" button
│       │   └── CommandIds.cs                    # Command GUIDs/IDs
│       ├── Telemetry/
│       │   ├── TelemetryConsentDialog.xaml       # First-run opt-in dialog
│       │   ├── TelemetryConsentDialog.xaml.cs
│       │   ├── TelemetrySettings.cs              # Read/write telemetry state
│       │   └── TelemetryOptionPage.cs            # VS Tools > Options page
│       ├── Templates/
│       │   └── (build file templates embedded as resources)
│       └── VSCommandTable.vsct                  # Menu/toolbar command definitions
```

## 2. Target Framework & SDK

- **Target**: .NET Framework 4.7.2 (required for VS 2022 VSIX extensions)
- **VS SDK**: `Microsoft.VisualStudio.SDK` (17.0+)
- **Min VS version**: 17.0 (Visual Studio 2022)
- **Project type**: VSIX project using `Microsoft.VSSDK.BuildTools`

## 3. NuGet Package Dependencies

The extension itself does **not** reference Hybridizer NuGet packages at build time (it's a VS extension, not a Hybridizer app). Instead, the extension:
- Ensures the `hybridizer` dotnet tool is installed/available when "Hybridize Project" is used
- Adds `Hybridizer.Runtime.CUDAImports` PackageReference when hybridizing a project
- The `Hybridizer.App.Template` package is a `dotnet new` template — it is installed separately via `dotnet new install Hybridizer.App.Template` and provides the "New Project" template in VS automatically

### How templates work with VS 2022
When `Hybridizer.App.Template` is installed via `dotnet new install`, Visual Studio 2022 automatically discovers it and shows it in the **File > New Project** dialog. No extension code is needed for this — just document the install step for users.

## 4. Features

### 4.1 "Hybridize Project" Command

**What it does**: Converts an existing .NET project into a Hybridizer-enabled project by creating MSBuild integration files.

**Location in UI**:
- Project context menu in Solution Explorer: `Project > Hybridize Project`
- Also available via `Tools > Hybridizer > Hybridize Project`

**When invoked, the command will**:

1. **Validate prerequisites**:
   - Check the selected project is a C# project targeting net8.0+
   - Check `hybridizer` dotnet tool is accessible (`dotnet tool list -g` or `.config/dotnet-tools.json`)
   - Check `Hybridizer.Runtime.CUDAImports` is referenced (or offer to add it)

2. **Generate `Directory.Build.props`** in the project directory:
   ```xml
   <Project>
     <PropertyGroup>
       <HybridizerEnabled>true</HybridizerEnabled>
     </PropertyGroup>
   </Project>
   ```

3. **Generate `Directory.Build.targets`** in the project directory containing the MSBuild targets extracted from the template:
   - `DetectVisualStudio` target — locates vcvarsall.bat
   - `GenerateCUDA` target — runs `hybridizer` CLI to generate CUDA code from .NET assemblies
   - `DetectGPUArch` target — uses nvidia-smi to detect GPU compute capability
   - `CompileCUDA` target — invokes nvcc to compile generated CUDA into a native DLL

   These are extracted from the existing template at `/mnt/d/hybridizer-software-suite/dotnet-tool/Hybridizer.Template.Package/content/HybTemplate/HybridizerApp.csproj` (lines 14-133), parameterized for the actual project name.

4. **Add PackageReference** for `Hybridizer.Runtime.CUDAImports` if not already present.

5. **Show a results dialog** summarizing:
   - Files created
   - Any manual steps needed (e.g., "Install CUDA Toolkit", "Install hybridizer dotnet tool: `dotnet tool install -g hybridizer`", "Ensure VS C++ workload is installed")

### 4.2 Telemetry System

#### 4.2.1 First-Run Consent Dialog

On first activation of the extension (no prior consent decision stored), show a **modal WPF dialog**:

```
┌─────────────────────────────────────────────────────────────┐
│  Hybridizer - Help Us Improve                               │
│                                                             │
│  Hybridizer collects anonymous usage metrics to help us     │
│  improve the tool. Here is exactly what we collect:         │
│                                                             │
│  • Compilation time (how long hybridizer takes to run)      │
│  • Amount of generated code (number of lines/files)         │
│  • Code generation time                                     │
│  • GPU architecture detected                                │
│  • Success/failure status of builds                         │
│                                                             │
│  What we DO NOT collect:                                    │
│  • Your source code — never, under any circumstances        │
│  • File names, project names, or directory paths            │
│  • Any personally identifiable information                  │
│                                                             │
│  This data is used solely by Hybridizer to understand       │
│  usage patterns and improve the product. It is never        │
│  shared with any third party.                               │
│                                                             │
│  You can change this setting at any time in:                │
│  Tools > Options > Hybridizer > Telemetry                   │
│                                                             │
│        [Enable Telemetry]    [No Thanks]                    │
└─────────────────────────────────────────────────────────────┘
```

#### 4.2.2 Storage Mechanism

The telemetry consent state is stored in **two places** so external binaries can read it:

1. **Windows Registry** (primary, readable by native binaries):
   - Key: `HKEY_CURRENT_USER\Software\ALTIMESH\HYBRIDIZER`
     (matches existing convention from `template_CUDA.vcxproj`)
   - Value: `TelemetryEnabled` (DWORD): `1` = enabled, `0` = disabled

2. **Environment variable** (process-level, for the current VS session):
   - `HYBRIDIZER_TELEMETRY_ENABLED` = `"1"` or `"0"`
   - Set on VS startup based on registry value
   - This allows child processes (hybridizer CLI, nvcc wrappers) to detect telemetry state

#### 4.2.3 VS Options Page

Under **Tools > Options > Hybridizer**:

| Setting | Type | Description |
|---------|------|-------------|
| Enable Telemetry | Checkbox | Toggle telemetry on/off |

This is implemented as a `DialogPage` subclass registered with `[ProvideOptionPage]`. Changes immediately update both the registry key and environment variable.

### 4.3 New Project Template

The `Hybridizer.App.Template` NuGet package is a standard `dotnet new` template. VS 2022 discovers these automatically. The extension's role is:

1. On first load, check if the template is installed (`dotnet new list hybridizer-app`)
2. If not installed, show an info bar in VS: *"Hybridizer project template not found. Install it with: `dotnet new install Hybridizer.App.Template`"*
3. Provide a button in the info bar to auto-install it

## 5. VSCT Command Table

```
Menu hierarchy:
├── Tools
│   └── Hybridizer
│       └── Hybridize Project
├── Solution Explorer Context Menu (Project node)
│   └── Hybridize Project
```

## 6. Extension Manifest (source.extension.vsixmanifest)

- **ID**: `HybridizerExtension.<guid>`
- **Name**: Hybridizer
- **Author**: Hybridizer Engineers
- **Description**: Integrates the Hybridizer CUDA code generation toolchain into Visual Studio
- **Supported VS**: 17.0+ (VS 2022+)
- **License**: MIT
- **Icon**: Altimesh128x128.png
- **Tags**: CUDA, GPU, Hybridizer, HPC, C#

## 7. Implementation Order

1. **Phase 1 — Scaffolding**: Create solution, VSIX project, manifest, package class
2. **Phase 2 — Telemetry**: Consent dialog, registry/env storage, Options page
3. **Phase 3 — Hybridize Command**: Command registration, Directory.Build.props/targets generation, prerequisite checks
4. **Phase 4 — Template Integration**: Info bar for template installation check
5. **Phase 5 — Polish**: Icons, error handling, testing on VS 2022

## 8. Key Design Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Telemetry storage | Registry + env var | Registry persists across sessions; env var is inherited by child processes (hybridizer CLI). Uses existing `ALTIMESH\HYBRIDIZER` registry path convention. |
| Build targets location | `Directory.Build.targets` | MSBuild convention; auto-imported by all projects in the directory tree; keeps .csproj files clean |
| Template installation | User-driven (info bar) | Can't bundle `dotnet new` templates inside a VSIX; best to guide the user |
| Framework | net472 | Required by VS 2022 extensibility SDK |
| NuGet package addition | Via VS DTE/NuGet APIs | Proper integration with solution-level package management |

## 9. How External Binaries Read Telemetry State

The `hybridizer` CLI tool and runtime can check telemetry by:

```csharp
// Option 1: Environment variable (fastest, set by VS extension at startup)
string telemetry = Environment.GetEnvironmentVariable("HYBRIDIZER_TELEMETRY_ENABLED");

// Option 2: Registry (works outside VS)
using var key = Registry.CurrentUser.OpenSubKey(@"Software\ALTIMESH\HYBRIDIZER");
int enabled = (int)(key?.GetValue("TelemetryEnabled") ?? 0);
```

This follows the existing pattern where the runtime already reads `HYBRIDIZER_CUDA_VERSION` and `HYBRIDIZER_FLAVOR` from environment variables, and uses the `ALTIMESH\HYBRIDIZER` registry path.
