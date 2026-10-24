using Skynet.Primitives;
using Skynet.Predicates;

namespace Skynet.Collections;

internal sealed class CharsVector<T> :    ManagedVector<string, CharsAscending>
{
    internal CharsVector(int length)
        : base(DataType.Chars, length)
    {
    }

#region COM Conversion
    internal override string ComUnwrap(object value)
    => ((Chars)value).Value;
    internal override object ComWrap(string value)
        => new Chars(value);
#endregion
}
