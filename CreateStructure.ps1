$ErrorActionPreference = "Stop"

$projectRoot = $PSScriptRoot
$sourceRoot  = Join-Path $projectRoot "Source"


function New-Folder {
    param(
        [Parameter(Mandatory)]
        [string] $RelativePath
    )

    $path = Join-Path $sourceRoot $RelativePath

    if (-not (Test-Path $path)) {
        New-Item `
            -ItemType Directory `
            -Path $path `
            -Force | Out-Null

        Write-Host "Ordner erstellt: Source\$RelativePath"
    }
}


function New-SourceFile {
    param(
        [Parameter(Mandatory)]
        [string] $RelativePath,

        [Parameter(Mandatory)]
        [string] $Content
    )

    $path   = Join-Path $sourceRoot $RelativePath
    $folder = Split-Path $path -Parent

    if (-not (Test-Path $folder)) {
        New-Item `
            -ItemType Directory `
            -Path $folder `
            -Force | Out-Null
    }

    if (Test-Path $path) {
        Write-Host `
            "Vorhanden, nicht verändert: Source\$RelativePath" `
            -ForegroundColor Yellow

        return
    }

    Set-Content `
        -Path $path `
        -Value $Content `
        -Encoding UTF8

    Write-Host "Erstellt: Source\$RelativePath"
}


function New-ComInterface {
    param(
        [Parameter(Mandatory)]
        [string] $RelativePath,

        [Parameter(Mandatory)]
        [string] $Namespace,

        [Parameter(Mandatory)]
        [string] $Name,

        [string] $BaseInterface
    )

    $guid = [guid]::NewGuid().ToString().ToUpperInvariant()

    if ([string]::IsNullOrWhiteSpace($BaseInterface)) {
        $declaration = "public interface $Name"
    }
    else {
        $declaration = "public interface $Name : $BaseInterface"
    }

    $content = @"
using System.Runtime.InteropServices;

namespace $Namespace;

[ComVisible(true)]
[Guid("$guid")]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
$declaration
{
}
"@

    New-SourceFile `
        -RelativePath $RelativePath `
        -Content $content
}


function New-ComClass {
    param(
        [Parameter(Mandatory)]
        [string] $RelativePath,

        [Parameter(Mandatory)]
        [string] $Namespace,

        [Parameter(Mandatory)]
        [string] $Name,

        [Parameter(Mandatory)]
        [string] $ProgId,

        [string] $Interface,

        [string[]] $Usings
    )

    $guid = [guid]::NewGuid().ToString().ToUpperInvariant()

    $usingText = "using System.Runtime.InteropServices;"

    if ($Usings) {
        foreach ($using in $Usings) {
            $usingText += "`r`nusing $using;"
        }
    }

    if ([string]::IsNullOrWhiteSpace($Interface)) {
        $declaration = "public sealed class $Name"
    }
    else {
        $declaration = "public sealed class $Name : $Interface"
    }

    $content = @"
$usingText

namespace $Namespace;

[ComVisible(true)]
[Guid("$guid")]
[ProgId("$ProgId")]
[ClassInterface(ClassInterfaceType.AutoDual)]
$declaration
{
    public $Name()
    {
    }
}
"@

    New-SourceFile `
        -RelativePath $RelativePath `
        -Content $content
}


# ============================================================
# Source-Ordner
# ============================================================

New-Item `
    -ItemType Directory `
    -Path $sourceRoot `
    -Force | Out-Null

$folders = @(
    "Interop"
    "Primitives"
    "Predicates"
    "Sequence"
    "Collections"
    "VertiPaq"
    "DataGrid"
)

foreach ($folder in $folders) {
    New-Folder $folder
}


# ============================================================
# Bestehende funktionierende Factory verschieben
# ============================================================

$factoryTarget = Join-Path $sourceRoot "Interop\Factory.cs"

$factoryCandidates = @(
    (Join-Path $projectRoot "Factory.cs")
    (Join-Path $projectRoot "Interop\Factory.cs")
)

if (-not (Test-Path $factoryTarget)) {
    $factorySource = $factoryCandidates |
        Where-Object { Test-Path $_ } |
        Select-Object -First 1

    if ($factorySource) {
        Move-Item `
            -Path $factorySource `
            -Destination $factoryTarget

        Write-Host `
            "Factory verschoben nach Source\Interop\Factory.cs" `
            -ForegroundColor Green
    }
    else {
        throw "Die bestehende Factory.cs wurde nicht gefunden."
    }
}
else {
    Write-Host `
        "Factory bereits vorhanden: Source\Interop\Factory.cs" `
        -ForegroundColor Yellow
}


# ============================================================
# Primitives
# ============================================================

New-ComInterface `
    -RelativePath "Primitives\IValue.cs" `
    -Namespace "Skynet.Primitives" `
    -Name "IValue"

New-ComClass `
    -RelativePath "Primitives\Bool.cs" `
    -Namespace "Skynet.Primitives" `
    -Name "Bool" `
    -ProgId "Skynet.Bool" `
    -Interface "IValue"

New-ComClass `
    -RelativePath "Primitives\Date.cs" `
    -Namespace "Skynet.Primitives" `
    -Name "Date" `
    -ProgId "Skynet.Date" `
    -Interface "IValue"

New-ComClass `
    -RelativePath "Primitives\Float.cs" `
    -Namespace "Skynet.Primitives" `
    -Name "Float" `
    -ProgId "Skynet.Float" `
    -Interface "IValue"

New-ComClass `
    -RelativePath "Primitives\Int.cs" `
    -Namespace "Skynet.Primitives" `
    -Name "Int" `
    -ProgId "Skynet.Int" `
    -Interface "IValue"

New-ComClass `
    -RelativePath "Primitives\Numeric.cs" `
    -Namespace "Skynet.Primitives" `
    -Name "Numeric" `
    -ProgId "Skynet.Numeric" `
    -Interface "IValue"

New-ComClass `
    -RelativePath "Primitives\String.cs" `
    -Namespace "Skynet.Primitives" `
    -Name "String" `
    -ProgId "Skynet.String" `
    -Interface "IValue"


# ============================================================
# Predicates
# Vorerst ausschließlich Interfaces
# ============================================================

New-ComInterface `
    -RelativePath "Predicates\IEquality.cs" `
    -Namespace "Skynet.Predicates" `
    -Name "IEquality"

New-ComInterface `
    -RelativePath "Predicates\IStrictOrder.cs" `
    -Namespace "Skynet.Predicates" `
    -Name "IStrictOrder"


# ============================================================
# Sequence
# ============================================================

New-ComInterface `
    -RelativePath "Sequence\IIterator.cs" `
    -Namespace "Skynet.Sequence" `
    -Name "IIterator"

New-ComInterface `
    -RelativePath "Sequence\ISequence.cs" `
    -Namespace "Skynet.Sequence" `
    -Name "ISequence"


# ============================================================
# Collections
#
# IContainer implementiert ISequence.
# Jede Collection implementiert IContainer.
# ============================================================

New-ComInterface `
    -RelativePath "Collections\IContainer.cs" `
    -Namespace "Skynet.Collections" `
    -Name "IContainer" `
    -BaseInterface "ISequence"

New-ComClass `
    -RelativePath "Collections\List.cs" `
    -Namespace "Skynet.Collections" `
    -Name "List" `
    -ProgId "Skynet.List" `
    -Interface "IContainer"

New-ComClass `
    -RelativePath "Collections\Map.cs" `
    -Namespace "Skynet.Collections" `
    -Name "Map" `
    -ProgId "Skynet.Map" `
    -Interface "IContainer"

New-ComClass `
    -RelativePath "Collections\OrderedMap.cs" `
    -Namespace "Skynet.Collections" `
    -Name "OrderedMap" `
    -ProgId "Skynet.OrderedMap" `
    -Interface "IContainer"

New-ComClass `
    -RelativePath "Collections\Lookup.cs" `
    -Namespace "Skynet.Collections" `
    -Name "Lookup" `
    -ProgId "Skynet.Lookup" `
    -Interface "IContainer"


# ============================================================
# Für IContainer wird noch der Namespace von ISequence benötigt.
# Das wird nach der Erzeugung einmal gezielt ergänzt.
# ============================================================

$containerPath = Join-Path $sourceRoot "Collections\IContainer.cs"

if (Test-Path $containerPath) {
    $containerContent = Get-Content $containerPath -Raw

    if ($containerContent -notmatch "using Skynet\.Sequence;") {
        $containerContent = $containerContent.Replace(
            "using System.Runtime.InteropServices;",
            "using System.Runtime.InteropServices;`r`nusing Skynet.Sequence;"
        )

        Set-Content `
            -Path $containerPath `
            -Value $containerContent `
            -Encoding UTF8

        Write-Host "Namespace-Verweis in IContainer ergänzt."
    }
}


# ============================================================
# VertiPaq
# ============================================================

New-ComClass `
    -RelativePath "VertiPaq\Storage.cs" `
    -Namespace "Skynet.VertiPaq" `
    -Name "Storage" `
    -ProgId "Skynet.Storage"

New-ComClass `
    -RelativePath "VertiPaq\VertiPaqList.cs" `
    -Namespace "Skynet.VertiPaq" `
    -Name "VertiPaqList" `
    -ProgId "Skynet.VertiPaqList"

New-ComClass `
    -RelativePath "VertiPaq\Table.cs" `
    -Namespace "Skynet.VertiPaq" `
    -Name "Table" `
    -ProgId "Skynet.Table"

New-ComClass `
    -RelativePath "VertiPaq\Filter.cs" `
    -Namespace "Skynet.VertiPaq" `
    -Name "Filter" `
    -ProgId "Skynet.Filter"

New-ComClass `
    -RelativePath "VertiPaq\ContextStore.cs" `
    -Namespace "Skynet.VertiPaq" `
    -Name "ContextStore" `
    -ProgId "Skynet.ContextStore"


# ============================================================
# DataGrid
# ============================================================

New-ComClass `
    -RelativePath "DataGrid\DataGrid.cs" `
    -Namespace "Skynet.DataGrid" `
    -Name "DataGrid" `
    -ProgId "Skynet.DataGrid"


# ============================================================
# Abschluss
# ============================================================

Write-Host ""
Write-Host "Projektstruktur wurde vollständig angelegt." `
    -ForegroundColor Green

Write-Host ""
Write-Host "Vorhandene Dateien und GUIDs wurden nicht überschrieben."

Write-Host ""
Write-Host "Nächster Schritt:"
Write-Host "  .\Build.ps1"