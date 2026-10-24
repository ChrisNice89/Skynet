using System;
using System.Runtime.InteropServices;

namespace Skynet.Collections;

[ComVisible(false)]
internal abstract class Vector<T> : Vector
{
    protected Vector(DataType dataType, int length)
        : base(dataType, length)
    {
    }

    internal T this[int index]
    {
        get
        {
            CheckIndex(index);
            return UnsafeAt(index);
        }
        set
        {
            CheckIndex(index);
            UnsafeAt(index, value);
        }
    }

    #region COM Conversion

    internal abstract T ComUnwrap(object value);
    internal abstract object ComWrap(T value);
    internal sealed override void SetObjectAt(int index, object value)
        => UnsafeAt(index, ComUnwrap(value));
    internal sealed override object GetObjectAt(int index)
        => ComWrap(UnsafeAt(index));

    #endregion

    #region Unsafe Access

    internal abstract T UnsafeAt(int index);

    internal abstract void UnsafeAt(int index, T value);

    #endregion

    #region Copy

    internal void CopyToVector(
        int sourceIndex,
        Vector<T> destination,
        int destinationIndex,
        int count)
    {
        CheckRange(sourceIndex, count, Length);
        CheckRange(destinationIndex, count, destination.Length);

        CopyElements(
            sourceIndex,
            destination,
            destinationIndex,
            count);
    }

    internal sealed override void CopyElements(
        int sourceIndex,
        Vector destination,
        int destinationIndex,
        int count)
    {
        if (destination is not Vector<T> target)
        {
            throw new ArgumentException(
                "The destination is not a Skynet vector.",
                nameof(destination));
        }

        CopyElements(
            sourceIndex,
            target,
            destinationIndex,
            count);
    }

    internal abstract void CopyElements(
        int sourceIndex,
        Vector<T> destination,
        int destinationIndex,
        int count);

    #endregion

    #region Validation

    private void CheckIndex(int index)
    {
        if ((uint)index >= (uint)Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

    private static void CheckRange(
        int index,
        int count,
        int length)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (count < 0 || count > length - index)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }
    }

    #endregion
}