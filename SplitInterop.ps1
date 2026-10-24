$ErrorActionPreference = "Stop"

$root = $PSScriptRoot
$coreProject = Join-Path $root "Skynet.csproj"
$factoryFile = Join-Path $root "Source\Interop\Factory.cs"
$interopRoot = Join-Path $root "InteropExport"
$interopProject = Join-Path $interopRoot "Skynet.Interop.csproj"
$exportsFile = Join-Path $interopRoot "Exports.cs"

if (-not (Test-Path $coreProject)) {
    throw "Skynet.csproj wurde nicht gefunden."
}

if (-not (Test-Path $factoryFile)) {
    throw "Source\Interop\Factory.cs wurde nicht gefunden."
}

New-Item `
    -ItemType Directory `
    -Path $interopRoot `
    -Force | Out-Null

# ------------------------------------------------------------
# Sicherung
# ------------------------------------------------------------

$backup = "$factoryFile.bak"

if (-not (Test-Path $backup)) {
    Copy-Item $factoryFile $backup
    Write-Host "Sicherung erstellt: $backup"
}

# ------------------------------------------------------------
# Exportklasse aus Factory.cs entfernen.
# Voraussetzung:
# internal static class Exports steht am Ende der Datei.
# ------------------------------------------------------------

$content = Get-Content `
    -Path $factoryFile `
    -Raw

$marker = "internal static class Exports"
$position = $content.IndexOf($marker)

if ($position -ge 0) {
    $content = $content.Substring(0, $position).TrimEnd()
    $content += [Environment]::NewLine

    Set-Content `
        -Path $factoryFile `
        -Value $content `
        -Encoding UTF8

    Write-Host "Exports wurde aus Factory.cs entfernt."
}
else {
    Write-Host `
        "Exports ist bereits aus Factory.cs entfernt." `
        -ForegroundColor Yellow
}

# ------------------------------------------------------------
# Minimale Exportklasse erzeugen
# ------------------------------------------------------------

@'
using System.Runtime.InteropServices;
using RGiesecke.DllExport;

namespace Skynet.Interop;

internal static class Exports
{
    [DllExport(
        "Factory",
        CallingConvention = CallingConvention.StdCall)]
    [return: MarshalAs(UnmanagedType.IDispatch)]
    public static object CreateFactory()
    {
        return new global::Skynet.Factory();
    }
}
'@ |
Set-Content `
    -Path $exportsFile `
    -Encoding UTF8

Write-Host "Erstellt: InteropExport\Exports.cs"

# ------------------------------------------------------------
# Exportprojekt erzeugen
# AssemblyName bewusst Skynet.Interop
# ------------------------------------------------------------

@'
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net481</TargetFramework>
    <AssemblyName>Skynet.Interop</AssemblyName>
    <RootNamespace>Skynet.Interop</RootNamespace>
    <OutputType>Library</OutputType>

    <Platforms>x86;x64</Platforms>
    <PlatformTarget>$(Platform)</PlatformTarget>
    <Prefer32Bit>false</Prefer32Bit>

    <Nullable>disable</Nullable>
    <ImplicitUsings>disable</ImplicitUsings>
    <LangVersion>latest</LangVersion>

    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Skynet.csproj" />
  </ItemGroup>

  <ItemGroup>
    <Reference Include="RGiesecke.DllExport.Metadata">
      <HintPath>..\packages\UnmanagedExports.1.2.7\lib\net\RGiesecke.DllExport.Metadata.dll</HintPath>
      <Private>true</Private>
    </Reference>
  </ItemGroup>

</Project>
'@ |
Set-Content `
    -Path $interopProject `
    -Encoding UTF8

Write-Host "Erstellt: InteropExport\Skynet.Interop.csproj"

Write-Host ""
Write-Host "Aufsplittung abgeschlossen." `
    -ForegroundColor Green

Write-Host ""
Write-Host "Core:   Skynet.csproj"
Write-Host "Export: InteropExport\Skynet.Interop.csproj"