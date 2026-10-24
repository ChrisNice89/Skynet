using System;
using System.Runtime.InteropServices;

namespace Skynet.Collections;

[ComVisible(false)]
internal static class VectorFactory
{
    internal static Vector Create(DataType dataType, int length)
    {
        return dataType switch
        {
            Skynet.DataType.Object => new ObjectVector<object>(length),
            Skynet.DataType.Bool => new BoolVector<Skynet.Primitives.Bool>(length),
            Skynet.DataType.Int => new IntVector<Skynet.Primitives.Int>(length),
            Skynet.DataType.Float => new FloatVector<Skynet.Primitives.Float>(length),
            Skynet.DataType.Numeric => new NumericVector<Skynet.Primitives.Numeric>(length),
            Skynet.DataType.Date => new DateVector<Skynet.Primitives.Date>(length),
            Skynet.DataType.Chars => new CharsVector<Skynet.Primitives.Chars>(length),
            _ => throw new ArgumentOutOfRangeException(nameof(dataType))
        };
    }

    internal static Vector<T> Create<T>(DataType dataType, int length)
    {
        if (dataType == DataType.Object)
        {
            return new ObjectVector<T>(length);
        }

        if (Create(dataType, length) is Vector<T> vector)
        {
            return vector;
        }

        throw new ArgumentException(
            $"DataType {dataType} does not store {typeof(T)} values.",
            nameof(dataType));
    }
}
