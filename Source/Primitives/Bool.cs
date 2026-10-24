using System;
using System.Runtime.InteropServices;

namespace Skynet.Primitives;
[ComVisible(true)]
[Guid("40287224-A3F7-4D82-A877-29C9CCEF380E")]
[ProgId("Skynet.Bool")]
[ClassInterface(ClassInterfaceType.AutoDual)]
public sealed class Bool : IValue<bool>
{
    internal Bool(bool value)
    {
        Value = value;
    }

    public bool Value { get; }

    object IValue.Value => Value;

    public DataType DataType => Skynet.DataType.Bool;

    public bool IsTypeSafe(DataType dataType)
    {
        return dataType == Skynet.DataType.Bool;
    }

    public bool Equals(IValue other)
    {
        return other is Bool value && Value == value.Value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

}
