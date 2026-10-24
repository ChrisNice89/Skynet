using System;
using System.Runtime.InteropServices;

namespace Skynet.Primitives;
[ComVisible(true)]
[Guid("BAD3AEA1-039B-4494-A21B-99C672A5EC15")]
[ProgId("Skynet.Date")]
[ClassInterface(ClassInterfaceType.AutoDual)]
public sealed class Date : IValue<DateTime>
{
    internal Date(DateTime value)
    {
        Value = value;
    }

    public DateTime Value { get; }

    object IValue.Value => Value;

    public DataType DataType => Skynet.DataType.Date;

    public bool IsTypeSafe(DataType dataType)
    {
        return dataType == Skynet.DataType.Date;
    }

    public bool Equals(IValue other)
    {
        return other is Date value && Value == value.Value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

}
