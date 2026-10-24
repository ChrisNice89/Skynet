using System.Runtime.InteropServices;

namespace Skynet.Primitives;
[ComVisible(true)]
[Guid("C1177BE0-E5E8-45AF-BAC1-DC59D2712C20")]
[ProgId("Skynet.String")]
[ClassInterface(ClassInterfaceType.AutoDual)]
public sealed class Chars : IValue<string>
{
    internal Chars(string value)
    {
        Value = value;
    }

    public string Value { get; }

    object IValue.Value => Value;

    public DataType DataType => Skynet.DataType.Chars;

    public bool IsTypeSafe(DataType dataType)
    {
        return dataType == Skynet.DataType.Chars;
    }

    public bool Equals(IValue other)
    {
        return other is Chars value
            && string.Equals(Value, value.Value, System.StringComparison.Ordinal);
    }

    public override string ToString()
    {
        return Value ?? string.Empty;
    }

}
