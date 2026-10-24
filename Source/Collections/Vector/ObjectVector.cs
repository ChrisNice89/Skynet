using System;

namespace Skynet.Collections;

internal sealed class ObjectVector<T> : ManagedVector<T>
{
    internal ObjectVector(int length)
        : base(DataType.Object, length)
    {
    }

#region COM Conversion
  internal override T ComUnwrap(object value)
    => (T)value;
    internal override object ComWrap(T value)
        => value!;
#endregion
}

