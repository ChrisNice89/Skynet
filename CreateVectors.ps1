$ErrorActionPreference = "Stop"

$vectorRoot = Join-Path `
    $PSScriptRoot `
    "Source\Collections\Vector"


function New-VectorFile {
    param(
        [Parameter(Mandatory)]
        [string] $FileName,

        [Parameter(Mandatory)]
        [string] $Content
    )

    $path = Join-Path $vectorRoot $FileName

    if (Test-Path $path) {
        Write-Host `
            "Vorhanden, nicht verändert: $FileName" `
            -ForegroundColor Yellow

        return
    }

    Set-Content `
        -Path $path `
        -Value $Content `
        -Encoding UTF8

    Write-Host "Erstellt: $FileName"
}


if (-not (Test-Path $vectorRoot)) {
    New-Item `
        -ItemType Directory `
        -Path $vectorRoot `
        -Force | Out-Null
}


# ============================================================
# BoolVector
# Interner Speicher: bool
# VBA-Wrapper: Skynet.Primitives.Bool
# ============================================================

New-VectorFile "BoolVector.cs" @'
using Skynet.Primitives;

namespace Skynet.Collections;

internal sealed class BoolVector :
    AbstractVectorPrimitive<bool>
{
    internal BoolVector(int length)
        : base(length)
    {
    }

    internal override object UnsafeGetAt(int index)
    {
        return new Bool(
            UnsafeGetValue(index));
    }

    internal override void UnsafePutAt(
        int index,
        object value)
    {
        UnsafePutValue(
            index,
            ((Bool)value).Value);
    }
}
'@


# ============================================================
# FloatVector
# Interner Speicher: double
# VBA-Wrapper: Skynet.Primitives.Float
# ============================================================

New-VectorFile "FloatVector.cs" @'
using Skynet.Primitives;

namespace Skynet.Collections;

internal sealed class FloatVector :
    AbstractVectorPrimitive<double>
{
    internal FloatVector(int length)
        : base(length)
    {
    }

    internal override object UnsafeGetAt(int index)
    {
        return new Float(
            UnsafeGetValue(index));
    }

    internal override void UnsafePutAt(
        int index,
        object value)
    {
        UnsafePutValue(
            index,
            ((Float)value).Value);
    }
}
'@


# ============================================================
# NumericVector
# Interner Speicher: decimal
# VBA-Wrapper: Skynet.Primitives.Numeric
# ============================================================

New-VectorFile "NumericVector.cs" @'
using Skynet.Primitives;

namespace Skynet.Collections;

internal sealed class NumericVector :
    AbstractVectorPrimitive<decimal>
{
    internal NumericVector(int length)
        : base(length)
    {
    }

    internal override object UnsafeGetAt(int index)
    {
        return new Numeric(
            UnsafeGetValue(index));
    }

    internal override void UnsafePutAt(
        int index,
        object value)
    {
        UnsafePutValue(
            index,
            ((Numeric)value).Value);
    }
}
'@


# ============================================================
# DateVector
# Interner Speicher: DateTime
# VBA-Wrapper: Skynet.Primitives.Date
# ============================================================

New-VectorFile "DateVector.cs" @'
using System;
using Skynet.Primitives;

namespace Skynet.Collections;

internal sealed class DateVector :
    AbstractVectorPrimitive<DateTime>
{
    internal DateVector(int length)
        : base(length)
    {
    }

    internal override object UnsafeGetAt(int index)
    {
        return new Date(
            UnsafeGetValue(index));
    }

    internal override void UnsafePutAt(
        int index,
        object value)
    {
        UnsafePutValue(
            index,
            ((Date)value).Value);
    }
}
'@


# ============================================================
# CharsVector
#
# string ist ein verwalteter Referenztyp und kann daher nicht
# in FixedSizeMemory<T> mit "where T : unmanaged" liegen.
# ============================================================

New-VectorFile "CharsVector.cs" @'
using System;
using Skynet.Primitives;

namespace Skynet.Collections;

internal sealed class CharsVector : Vector
{
    private readonly string[] _values;

    internal CharsVector(int length)
        : base(length)
    {
        _values = new string[length];
    }

    internal override object UnsafeGetAt(int index)
    {
        return new Chars(
            _values[index]);
    }

    internal override void UnsafePutAt(
        int index,
        object value)
    {
        _values[index] =
            ((Chars)value).Value;
    }

    internal override void UnsafeCopyTo(
        int sourceIndex,
        Vector destination,
        int destinationIndex,
        int count)
    {
        if (destination is not CharsVector target)
        {
            throw new ArgumentException(
                "Das Ziel muss ein CharsVector sein.",
                nameof(destination));
        }

        Array.Copy(
            _values,
            sourceIndex,
            target._values,
            destinationIndex,
            count);
    }

    public override void Dispose()
    {
        Array.Clear(
            _values,
            0,
            _values.Length);
    }
}
'@


# ============================================================
# ObjectVector
#
# object[] bleibt managed, damit der Garbage Collector
# Referenzen und Write Barriers korrekt verwalten kann.
# ============================================================

New-VectorFile "ObjectVector.cs" @'
using System;

namespace Skynet.Collections;

internal sealed class ObjectVector : Vector
{
    private readonly object[] _values;

    internal ObjectVector(int length)
        : base(length)
    {
        _values = new object[length];
    }

    internal override object UnsafeGetAt(int index)
    {
        return _values[index];
    }

    internal override void UnsafePutAt(
        int index,
        object value)
    {
        _values[index] = value;
    }

    internal override void UnsafeCopyTo(
        int sourceIndex,
        Vector destination,
        int destinationIndex,
        int count)
    {
        if (destination is not ObjectVector target)
        {
            throw new ArgumentException(
                "Das Ziel muss ein ObjectVector sein.",
                nameof(destination));
        }

        Array.Copy(
            _values,
            sourceIndex,
            target._values,
            destinationIndex,
            count);
    }

    public override void Dispose()
    {
        Array.Clear(
            _values,
            0,
            _values.Length);
    }
}
'@


Write-Host ""
Write-Host "Vector-Klassen wurden angelegt." `
    -ForegroundColor Green

Write-Host ""
Write-Host "Vorhandene Dateien wurden nicht überschrieben."
Write-Host "IntVector.cs bleibt unverändert."