using System;
using Skynet.Primitives;
using Skynet.Predicates;

namespace Skynet.Collections;

internal sealed class DateVector<TWrapper> :
    UnmanagedVector<DateTime, DateAscending>
    where TWrapper : IValue<DateTime>
{
    internal DateVector(int length)
        : base(DataType.Date,length)
    {
    }

#region COM Conversion
    internal override DateTime ComUnwrap(object value)
    => ((Date)value).Value;
    internal override object ComWrap(DateTime value)
        => new Date(value);
#endregion
}
