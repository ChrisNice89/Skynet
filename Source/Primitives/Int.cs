using System.Runtime.InteropServices;

namespace Skynet.Primitives;
[ComVisible(true)]
[Guid("0815E4B6-17CE-4508-9F05-BA33273FEFE4")]
[ProgId("Skynet.Int")]
[ClassInterface(ClassInterfaceType.AutoDual)]
public sealed class Int : IValue<int>
{
    internal Int(int value)
    {
        Value = value;
    }

    public int Value { get; }

    object IValue.Value => Value;

    public DataType DataType => Skynet.DataType.Int;

    public bool IsTypeSafe(DataType dataType)
    {
        return dataType == Skynet.DataType.Int;
    }

    public bool Equals(IValue other)
    {
        return other is Int value && Value == value.Value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

}
