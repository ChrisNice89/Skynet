using System;
using Skynet.Predicates;
using Skynet.Primitives;
using Skynet.Memory;

namespace Skynet.Collections;

internal sealed class IntVector<TWrapper> :
    UnmanagedVector<int, IntAscending>
    where TWrapper : IValue<int>
{
    internal IntVector(int length)
        : base(DataType.Int, length)
    {
    }
#region COM Conversion
    internal override int ComUnwrap(object value)
    => ((Int)value).Value;
    internal override object ComWrap(int value)
        => new Int(value);
#endregion
}
