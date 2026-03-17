' Last Edit: 2026-03-16 - Added ComVisible, CLSCompliant, and AssemblyMetadata (repository URL/type).
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Windows

' WPF theme dictionary locations (required by WPF infrastructure).
' Generic dictionary is embedded in this assembly; no per-theme dictionaries.
<Assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)>

' This assembly exposes no COM-callable types.
<Assembly: ComVisible(False)>

' All public APIs comply with the Common Language Specification.
<Assembly: CLSCompliant(True)>

' Source repository metadata — consumed by NuGet, dotnet tooling, and IDEs.
<Assembly: AssemblyMetadata("RepositoryUrl", "https://github.com/dmaidon/KenoWpf")>
<Assembly: AssemblyMetadata("RepositoryType", "git")>
