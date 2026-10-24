using System;
using System.Runtime.InteropServices;
using Skynet.Memory;
using Skynet.Predicates;
using Skynet.Source.Collections.Memory;

namespace Skynet.Collections;

[ComVisible(false)]
internal abstract class UnmanagedVector<T, TOrder> : Vector<T>
    where T : unmanaged
    where TOrder : struct, IStrictOrder<T>
{
    #region Fields

    private readonly UnmanagedMemory<T> _memory;
    private static readonly Random Random = new Random();
    private static readonly object RandomLock = new object();

    #endregion

    #region Construction

    protected UnmanagedVector(DataType kind, int length)
        : base(kind, length)
    {
        _memory = new UnmanagedMemory<T>(length);
    }

    #endregion

    #region Unsafe Access

    internal override T UnsafeAt(int index)
        => _memory.At(index);

    internal override void UnsafeAt(int index, T value)
        => _memory.At(index) = value;

    #endregion

    #region Reordering

    internal override void SwapItems(int left, int right)
    {
        ref T leftValue = ref _memory.At(left);
        ref T rightValue = ref _memory.At(right);

        T temporary = leftValue;
        leftValue = rightValue;
        rightValue = temporary;
    }

    internal override void ShuffleRange(int start, int count)
    {
        if (count <= 1)
        {
            return;
        }

        lock (RandomLock)
        {
            int begin = start;

            for (int i = start + count - 1; i > begin; i--)
            {
                int j = Random.Next(begin, i + 1);

                ref T leftValue = ref _memory.At(i);
                ref T rightValue = ref _memory.At(j);

                T temporary = leftValue;
                leftValue = rightValue;
                rightValue = temporary;
            }
        }
    }

    #endregion

    #region Copy

    internal override void CopyElements(
        int sourceIndex,
        Vector<T> destination,
        int destinationIndex,
        int count)
    {
        if (destination is not UnmanagedVector<T, TOrder> target)
        {
            throw new ArgumentException(
                "Quell- und Zielvector haben unterschiedliche Datentypen.",
                nameof(destination));
        }

        _memory.CopyTo(
            sourceIndex,
            target._memory,
            destinationIndex,
            count);
    }

    #endregion

    #region Sorting

    internal override void SortRange(int start, int count)
        => SortEngine.Sort<
            T,
            UnmanagedMemory<T>,
            TOrder>(
                _memory,
                start,
                count,
                default);

    internal override RunOrder IsRangeOrdered(int start, int count)
        => SortEngine.IsOrdered<
            T,
            UnmanagedMemory<T>,
            TOrder>(
                _memory,
                start,
                count,
                default);

    #endregion

    #region Searching

    public override int BinarySearchLeft(
        int start,
        int count,
        [In, MarshalAs(UnmanagedType.IDispatch)] object element)
    {
        CheckBlock(start, count, Length);

        return SortEngine.BinarySearchLeft<
            T,
            UnmanagedMemory<T>,
            TOrder>(
                _memory,
                start,
                count,
                ComUnwrap(element),
                default);
    }

    public override int BinarySearchRight(
        int start,
        int count,
        [In, MarshalAs(UnmanagedType.IDispatch)] object element)
    {
        CheckBlock(start, count, Length);

        return SortEngine.BinarySearchRight<
            T,
            UnmanagedMemory<T>,
            TOrder>(
                _memory,
                start,
                count,
                ComUnwrap(element),
                default);
    }

    #endregion

    #region Disposal

    public override void Dispose()
    {
        _memory.Dispose();
    }

    #endregion
}