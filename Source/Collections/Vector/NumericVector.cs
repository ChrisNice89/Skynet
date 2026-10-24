using System;
using Skynet.Primitives;
using Skynet.Predicates;

namespace Skynet.Collections;

internal sealed class NumericVector<TWrapper> :
    UnmanagedVector<decimal, NumericAscending>
    where TWrapper : IValue<decimal>
{
    internal NumericVector(int length)
        : base(DataType.Numeric,length)
    {
    }
#region COM Conversion
    internal override decimal ComUnwrap(object value)
    => ((Numeric)value).Value;
    internal override object ComWrap(decimal value)
        => new Numeric(value);
#endregion
}
