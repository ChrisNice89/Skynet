using System.Runtime.InteropServices;

namespace Skynet.Primitives;
[ComVisible(true)]
[Guid("EE18FA8A-7AD3-4716-B8BC-3FAF32805230")]
[ProgId("Skynet.Numeric")]
[ClassInterface(ClassInterfaceType.AutoDual)]
public sealed class Numeric : IValue<decimal>
{
    internal Numeric(decimal value)
    {
        Value = value;
    }

    public decimal Value { get; }

    object IValue.Value => Value;

    public DataType DataType => Skynet.DataType.Numeric;

    public bool IsTypeSafe(DataType dataType)
    {
        return dataType == Skynet.DataType.Numeric;
    }

    public bool Equals(IValue other)
    {
        return other is Numeric value && Value == value.Value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

}