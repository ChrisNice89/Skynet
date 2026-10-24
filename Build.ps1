$ErrorActionPreference = "Stop"

$Root = $PSScriptRoot

$CoreProject = Join-Path `
    $Root `
    "Skynet.csproj"

$InteropProject = Join-Path `
    $Root `
    "InteropExport\Skynet.Interop.csproj"

$DllExportProject = Join-Path `
    $Root `
    "DllExport.proj"

$Configuration = "Release"
$TargetFramework = "net481"


function Invoke-Checked {
    param(
        [Parameter(Mandatory)]
        [string] $Action,

        [Parameter(Mandatory)]
        [scriptblock] $Command
    )

    Write-Host ""
    Write-Host "=== $Action ===" `
        -ForegroundColor Cyan

    & $Command

    if ($LASTEXITCODE -ne 0) {
        throw "$Action fehlgeschlagen. Exitcode: $LASTEXITCODE"
    }
}

function Find-Tool {
    param(
        [Parameter(Mandatory)]
        [string] $FileName,

        [Parameter(Mandatory)]
        [string[]] $SearchRoots,

        [Parameter(Mandatory)]
        [string] $RequiredPathPattern
    )

    foreach ($SearchRoot in $SearchRoots) {
        if (-not (Test-Path $SearchRoot)) {
            continue
        }

        $Tool = Get-ChildItem `
            -Path $SearchRoot `
            -Filter $FileName `
            -File `
            -Recurse `
            -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -match $RequiredPathPattern } |
            Sort-Object FullName -Descending |
            Select-Object -First 1

        if ($Tool) {
            return $Tool.FullName
        }
    }

    return $null
}

$FrameworkPath =
    "${env:WINDIR}\Microsoft.NET\Framework\v4.0.30319"

$FrameworkSdkRoots = @(
    "${env:ProgramFiles(x86)}\Microsoft SDKs\Windows",
    "${env:ProgramFiles}\Microsoft SDKs\Windows"
)

$script:TlbExpX86 = Find-Tool `
    -FileName "tlbexp.exe" `
    -SearchRoots $FrameworkSdkRoots `
    -RequiredPathPattern "NETFX(?!.*\\x64\\)"

$script:TlbExpX64 = Find-Tool `
    -FileName "tlbexp.exe" `
    -SearchRoots $FrameworkSdkRoots `
    -RequiredPathPattern "NETFX.*\\x64\\tlbexp\.exe$"

if (-not $script:TlbExpX86) {
    throw "x86 tlbexp.exe wurde nicht gefunden."
}

if (-not $script:TlbExpX64) {
    throw "x64 tlbexp.exe wurde nicht gefunden."
}

$script:SdkPathX86 = Split-Path -Parent $script:TlbExpX86
$script:SdkPathX64 = Split-Path -Parent $script:TlbExpX64

foreach ($ToolPath in @($script:SdkPathX86, $script:SdkPathX64)) {
    if (-not (Test-Path (Join-Path $ToolPath "ildasm.exe"))) {
        throw "ildasm.exe fehlt im SDK-Ordner: $ToolPath"
    }
}

Write-Host "Framework: $FrameworkPath"
Write-Host "SDK x86:   $script:SdkPathX86"
Write-Host "SDK x64:   $script:SdkPathX64"

function Build-Platform {
    param(
        [Parameter(Mandatory)]
        [ValidateSet("x86", "x64")]
        [string] $Platform
    )

    $CoreOutput = Join-Path `
        $Root `
        "bin\$Platform\$Configuration\$TargetFramework"

    $InteropOutput = Join-Path `
        $Root `
        "InteropExport\bin\$Platform\$Configuration\$TargetFramework"

    $CoreDll = Join-Path `
        $CoreOutput `
        "Skynet.dll"

    $CoreTlb = Join-Path `
        $CoreOutput `
        "Skynet.tlb"

    $UnsafeDll = Join-Path `
        $CoreOutput `
        "System.Runtime.CompilerServices.Unsafe.dll"

    $InteropDll = Join-Path `
        $InteropOutput `
        "Skynet.Interop.dll"

    Invoke-Checked `
        "Skynet Core $Platform bauen" `
        {
            dotnet build `
                $CoreProject `
                -c $Configuration `
                -p:Platform=$Platform `
                --no-restore
        }

    if (-not (Test-Path $CoreDll)) {
        throw "Core-DLL nicht gefunden: $CoreDll"
    }

    if (-not (Test-Path $UnsafeDll)) {
        throw "Unsafe-Abhängigkeit nicht gefunden: $UnsafeDll"
    }

    Invoke-Checked `
        "Skynet Interop $Platform bauen" `
        {
            dotnet build `
                $InteropProject `
                -c $Configuration `
                -p:Platform=$Platform `
                --no-restore
        }

    if (-not (Test-Path $InteropDll)) {
        throw "Interop-DLL nicht gefunden: $InteropDll"
    }

    $PlatformSdkPath = $script:SdkPathX86
    $PlatformTlbExp = $script:TlbExpX86
    $TlbArchitecture = "/win32"

    if ($Platform -eq "x64") {
        $PlatformSdkPath = $script:SdkPathX64
        $PlatformTlbExp = $script:TlbExpX64
        $TlbArchitecture = "/win64"
    }

Invoke-Checked `
    "Native Exporte fuer $Platform erzeugen" `
    {
        & $script:MSBuild `
            $DllExportProject `
            /t:RGieseckeDllExport `
            /p:Configuration=$Configuration `
            /p:Platform=$Platform `
            /p:PlatformTarget=$Platform `
            /p:DllExportPlatform=$Platform `
            /p:DllExportPlatformTarget=$Platform `
            /p:DllExportInputFileName="$InteropDll" `
            /p:DllExportProjectDirectory="$InteropOutput" `
            /p:TargetFrameworkVersion= `
            /p:DllExportTargetFrameworkVersion= `
            /p:DllExportFrameworkPath="$FrameworkPath" `
            /p:DllExportSdkPath="$PlatformSdkPath" `
            /nologo
    }

    Invoke-Checked `
        "Skynet TLB fuer $Platform erzeugen" `
        {
            & $PlatformTlbExp `
                $CoreDll `
                "/out:$CoreTlb" `
                "/asmpath:$CoreOutput" `
                $TlbArchitecture `
                /nologo
        }

    if (-not (Test-Path $CoreTlb)) {
        throw "Skynet.tlb wurde nicht erzeugt: $CoreTlb"
    }

    Copy-Item `
        -Path $CoreDll `
        -Destination $InteropOutput `
        -Force

    Copy-Item `
        -Path $CoreTlb `
        -Destination $InteropOutput `
        -Force

    Copy-Item `
        -Path $UnsafeDll `
        -Destination $InteropOutput `
        -Force

    $FinalFiles = @(
        (Join-Path $InteropOutput "Skynet.Interop.dll"),
        (Join-Path $InteropOutput "Skynet.dll"),
        (Join-Path $InteropOutput "Skynet.tlb"),
        (Join-Path $InteropOutput "System.Runtime.CompilerServices.Unsafe.dll")
    )

    foreach ($FinalFile in $FinalFiles) {
        if (-not (Test-Path $FinalFile)) {
            throw "Finale Ausgabedatei fehlt: $FinalFile"
        }
    }

    Write-Host ""
    Write-Host ("Ausgabe {0}:" -f $Platform) `
        -ForegroundColor Green

    Write-Host "  $InteropDll"
    Write-Host "  $(Join-Path $InteropOutput 'Skynet.dll')"
    Write-Host "  $(Join-Path $InteropOutput 'Skynet.tlb')"
    Write-Host "  $(Join-Path $InteropOutput 'System.Runtime.CompilerServices.Unsafe.dll')"
}


# ------------------------------------------------------------
# MSBuild ermitteln
# ------------------------------------------------------------

$MSBuildCandidates = @(
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
    "${env:WINDIR}\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
)

$script:MSBuild = $MSBuildCandidates |
    Where-Object { Test-Path $_ } |
    Select-Object -First 1

if (-not $script:MSBuild) {
    throw "MSBuild.exe wurde nicht gefunden."
}

Write-Host "MSBuild: $script:MSBuild"


# ------------------------------------------------------------
# Projektdateien pruefen
# ------------------------------------------------------------

if (-not (Test-Path $CoreProject)) {
    throw "Core-Projekt nicht gefunden: $CoreProject"
}

if (-not (Test-Path $InteropProject)) {
    throw "Interop-Projekt nicht gefunden: $InteropProject"
}

if (-not (Test-Path $DllExportProject)) {
    throw "DllExport-Projekt nicht gefunden: $DllExportProject"
}


# ------------------------------------------------------------
# Alte Ausgaben entfernen
# ------------------------------------------------------------

Write-Host ""
Write-Host "=== Alte Build-Ausgaben entfernen ===" `
    -ForegroundColor Cyan

Remove-Item `
    (Join-Path $Root "bin") `
    -Recurse `
    -Force `
    -ErrorAction SilentlyContinue

Remove-Item `
    (Join-Path $Root "obj") `
    -Recurse `
    -Force `
    -ErrorAction SilentlyContinue

Remove-Item `
    (Join-Path $Root "InteropExport\bin") `
    -Recurse `
    -Force `
    -ErrorAction SilentlyContinue

Remove-Item `
    (Join-Path $Root "InteropExport\obj") `
    -Recurse `
    -Force `
    -ErrorAction SilentlyContinue


# ------------------------------------------------------------
# Restore
# ------------------------------------------------------------

Invoke-Checked `
    "Skynet Core wiederherstellen" `
    {
        dotnet restore $CoreProject
    }

Invoke-Checked `
    "Skynet Interop wiederherstellen" `
    {
        dotnet restore $InteropProject
    }


# ------------------------------------------------------------
# x86 und x64 bauen
# ------------------------------------------------------------

Build-Platform "x86"
Build-Platform "x64"


Write-Host ""
Write-Host "=== Build erfolgreich ===" `
    -ForegroundColor Green