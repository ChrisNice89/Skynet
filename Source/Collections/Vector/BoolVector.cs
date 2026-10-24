using System;
using Skynet.Primitives;
using Skynet.Predicates;

namespace Skynet.Collections;

internal sealed class BoolVector<TWrapper> :
    UnmanagedVector<bool, BoolAscending>
    where TWrapper : IValue<bool>
{
    internal BoolVector(int length)
        : base(DataType.Bool,length)
    {
    }

#region COM Conversion
    internal override bool ComUnwrap(object value)
    => ((Bool)value).Value;
    internal override object ComWrap(bool value)
        => new Bool(value);
#endregion
}