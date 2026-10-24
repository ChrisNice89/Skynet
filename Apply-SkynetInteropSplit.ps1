$ErrorActionPreference = "Stop"

$Root = $PSScriptRoot
$InteropDir = Join-Path $Root "InteropExport"
$ExportsFile = Join-Path $InteropDir "Exports.cs"
$InteropProject = Join-Path $InteropDir "Skynet.Interop.csproj"
$CoreProject = Join-Path $Root "Skynet.csproj"

if (-not (Test-Path $CoreProject)) {
    throw "Skynet.csproj wurde nicht gefunden. Das Skript muss im Skynet-Projektstamm liegen."
}

New-Item -ItemType Directory -Path $InteropDir -Force | Out-Null

function Backup-File {
    param(
        [Parameter(Mandatory)]
        [string] $Path
    )

    if (Test-Path $Path) {
        $Backup = "$Path.bak"

        if (-not (Test-Path $Backup)) {
            Copy-Item -Path $Path -Destination $Backup -Force
            Write-Host "Sicherung: $Backup" -ForegroundColor DarkGray
        }
    }
}

Backup-File $ExportsFile
Backup-File $InteropProject
Backup-File $CoreProject

@'
using System;
using System.IO;
using System.Reflection;
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
        try
        {
            string directory =
                Path.GetDirectoryName(
                    typeof(Exports).Assembly.Location);

            string corePath =
                Path.Combine(
                    directory,
                    "Skynet.dll");

            Assembly core =
                Assembly.LoadFrom(corePath);

            Type factoryType =
                core.GetType(
                    "Skynet.Factory",
                    throwOnError: true);

            return Activator.CreateInstance(factoryType);
        }
        catch (Exception exception)
        {
            TryWriteError(exception);
            return null;
        }
    }

    private static void TryWriteError(Exception exception)
    {
        try
        {
            string directory =
                Path.GetDirectoryName(
                    typeof(Exports).Assembly.Location);

            string logPath =
                Path.Combine(
                    directory,
                    "Skynet.Interop.log");

            File.WriteAllText(
                logPath,
                exception.ToString());
        }
        catch
        {
        }
    }
}
'@ | Set-Content -Path $ExportsFile -Encoding UTF8

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
    <Reference Include="RGiesecke.DllExport.Metadata">
      <HintPath>..\packages\UnmanagedExports.1.2.7\lib\net\RGiesecke.DllExport.Metadata.dll</HintPath>
      <Private>true</Private>
    </Reference>
  </ItemGroup>

</Project>
'@ | Set-Content -Path $InteropProject -Encoding UTF8

[xml] $ProjectXml = Get-Content -Path $CoreProject -Raw
$Namespace = $ProjectXml.Project.NamespaceURI
$Manager = New-Object System.Xml.XmlNamespaceManager($ProjectXml.NameTable)
$Manager.AddNamespace("m", $Namespace)

$ExistingRemove = $ProjectXml.SelectSingleNode(
    "//m:Compile[@Remove='InteropExport\**\*.cs']",
    $Manager)

if ($null -eq $ExistingRemove) {
    $ItemGroup = $ProjectXml.CreateElement("ItemGroup", $Namespace)
    $Compile = $ProjectXml.CreateElement("Compile", $Namespace)
    $Compile.SetAttribute("Remove", "InteropExport\**\*.cs")
    [void] $ItemGroup.AppendChild($Compile)
    [void] $ProjectXml.Project.AppendChild($ItemGroup)
    $ProjectXml.Save($CoreProject)
}

Remove-Item (Join-Path $Root "bin") -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $Root "obj") -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $InteropDir "bin") -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $InteropDir "obj") -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "" 
Write-Host "Anpassungen abgeschlossen:" -ForegroundColor Green
Write-Host "  $ExportsFile"
Write-Host "  $InteropProject"
Write-Host "  $CoreProject"
Write-Host ""
Write-Host "Naechster Befehl: .\Build.ps1" -ForegroundColor Cyan
