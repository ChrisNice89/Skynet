using System.Runtime.InteropServices;

namespace Skynet.Primitives;
[ComVisible(true)]
[Guid("2A081304-AF1B-4C53-A107-A754D4A77FB8")]
[ProgId("Skynet.Float")]
[ClassInterface(ClassInterfaceType.AutoDual)]
public sealed class Float : IValue<double>
{
    internal Float(double value)
    {
        Value = value;
    }

    public double Value { get; }

    object IValue.Value => Value;

    public DataType DataType => Skynet.DataType.Float;

    public bool IsTypeSafe(DataType dataType)
    {
        return dataType == Skynet.DataType.Float;
    }

    public bool Equals(IValue other)
    {
        return other is Float value && Value.Equals(value.Value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }

}
