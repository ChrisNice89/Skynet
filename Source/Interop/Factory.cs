using System.Runtime.InteropServices;
using System;
using RGiesecke.DllExport;
using Skynet.Collections;
using Skynet.Primitives;

namespace Skynet;

[ComVisible(true)]
[Guid("39D84C48-F5B1-437C-889C-11D0EA4DF683")]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
public interface IFactory
{
    Bool Bool([In] bool value);
    Int Int([In] int value);
    Float Float([In] double value);
    Numeric Numeric([In] decimal value);
    Date Date([In] DateTime value);
    Chars Chars([In, MarshalAs(UnmanagedType.BStr)] string value);
    List List([In] DataType dataType, [In, Optional, DefaultParameterValue(0)] int capacity);
    Vector Vector([In] DataType dataType, [In] int length);
}

[ComVisible(true)]
[Guid("4C8E038F-9C67-483D-AB78-607D61333906")]
[ProgId("Skynet.Factory")]
[ClassInterface(ClassInterfaceType.None)]
[ComDefaultInterface(typeof(IFactory))]
public sealed class Factory : IFactory
{
    Bool IFactory.Bool(bool value) => new Bool(value);
    Int IFactory.Int(int value) => new Int(value);
    Float IFactory.Float(double value) => new Float(value);
    Numeric IFactory.Numeric(decimal value) => new Numeric(value);
    Date IFactory.Date(DateTime value) => new Date(value);
    Chars IFactory.Chars(string value) => new Chars(value);

    List IFactory.List(DataType dataType, int capacity)
        => dataType switch
        {
            DataType.Object => new Skynet.Collections.List<object>(dataType, capacity),
            DataType.Bool => new Skynet.Collections.List<bool>(dataType, capacity),
            DataType.Int => new Skynet.Collections.List<int>(dataType, capacity),
            DataType.Float => new Skynet.Collections.List<double>(dataType, capacity),
            DataType.Numeric => new Skynet.Collections.List<decimal>(dataType, capacity),
            DataType.Date => new Skynet.Collections.List<DateTime>(dataType, capacity),
            DataType.Chars => new Skynet.Collections.List<string>(dataType, capacity),
            _ => throw new ArgumentOutOfRangeException(nameof(dataType))
        };

    Vector IFactory.Vector(DataType dataType, int length)
        => Skynet.Collections.VectorFactory.Create(dataType, length);
}