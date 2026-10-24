using System;
using Skynet.Primitives;
using Skynet.Predicates;

namespace Skynet.Collections;

internal sealed class FloatVector<TWrapper> :
    UnmanagedVector<double, FloatAscending>
    where TWrapper : IValue<double>
{
    internal FloatVector(int length)
        : base(DataType.Float,length)
    {
    }
#region COM Conversion
    internal override double ComUnwrap(object value)
    => ((Float)value).Value;
    internal override object ComWrap(double value)
        => new Float(value);
#endregion
}