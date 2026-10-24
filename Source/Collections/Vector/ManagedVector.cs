using System;
using System.Runtime.InteropServices;
using Skynet.Predicates;
using Skynet.Source.Collections.Memory;

namespace Skynet.Collections;

[ComVisible(false)]
internal abstract class ManagedVector<T> : Vector<T>
{
    private readonly ManagedMemory<T> _memory;
    private static readonly Random Random = new Random();
    private static readonly object RandomLock = new object();

    protected ManagedVector(DataType kind, int length)
        : base(kind, length)
        => _memory = new ManagedMemory<T>(length);

    #region Element Access

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
            return;

        lock (RandomLock)
        {
            for (int i = start + count - 1; i > start; i--)
            {
                int j = Random.Next(start, i + 1);

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
        if (destination is not ManagedVector<T> target)
            throw new ArgumentException(
                "Quell- und Zielvector haben unterschiedliche Datentypen.",
                nameof(destination));

        _memory.CopyTo(
            sourceIndex,
            target._memory,
            destinationIndex,
            count);
    }

    #endregion

    #region Explicit Order

    internal void Sort<TOrder>(
        int start,
        int count,
        TOrder order)
        where TOrder : struct, IStrictOrder<T>
        => SortEngine.Sort<T, ManagedMemory<T>, TOrder>(
            _memory, start, count, order);

    internal RunOrder IsOrdered<TOrder>(
        int start,
        int count,
        TOrder order)
        where TOrder : struct, IStrictOrder<T>
        => SortEngine.IsOrdered<T, ManagedMemory<T>, TOrder>(
            _memory, start, count, order);

    internal int BinarySearchLeft<TOrder>(
        int start,
        int count,
        T element,
        TOrder order)
        where TOrder : struct, IStrictOrder<T>
        => SortEngine.BinarySearchLeft<T, ManagedMemory<T>, TOrder>(
            _memory, start, count, element, order);

    internal int BinarySearchRight<TOrder>(
        int start,
        int count,
        T element,
        TOrder order)
        where TOrder : struct, IStrictOrder<T>
        => SortEngine.BinarySearchRight<T, ManagedMemory<T>, TOrder>(
            _memory, start, count, element, order);

    #endregion

    #region Default Order

    internal override void SortRange(int start, int count)
        => throw new InvalidOperationException(
            "Vector benötigt eine explizite Sortierreihenfolge.");

    internal override RunOrder IsRangeOrdered(int start, int count)
        => throw new InvalidOperationException(
            "Vector benötigt eine explizite Sortierreihenfolge.");

    public override int BinarySearchLeft(
        int start,
        int count,
        [In, MarshalAs(UnmanagedType.IDispatch)] object element)
    {
        CheckBlock(start, count, Length);
        throw new InvalidOperationException(
            "Vector benötigt eine explizite Sortierreihenfolge.");
    }

    public override int BinarySearchRight(
        int start,
        int count,
        [In, MarshalAs(UnmanagedType.IDispatch)] object element)
    {
        CheckBlock(start, count, Length);
        throw new InvalidOperationException(
            "Vector benötigt eine explizite Sortierreihenfolge.");
    }

    #endregion

    public override void Dispose()
        => _memory.Dispose();
}

[ComVisible(false)]
internal abstract class ManagedVector<T, TDefaultOrder> : ManagedVector<T>
    where TDefaultOrder : struct, IStrictOrder<T>
{
    protected ManagedVector(DataType kind, int length)
        : base(kind, length)
    {
    }

    internal override void SortRange(int start, int count)
        => Sort(start, count, default(TDefaultOrder));

    internal override RunOrder IsRangeOrdered(int start, int count)
        => IsOrdered(start, count, default(TDefaultOrder));

    public override int BinarySearchLeft(
        int start,
        int count,
        [In, MarshalAs(UnmanagedType.IDispatch)] object element)
    {
        CheckBlock(start, count, Length);
        return BinarySearchLeft(
            start, count, ComUnwrap(element), default(TDefaultOrder));
    }

    public override int BinarySearchRight(
        int start,
        int count,
        [In, MarshalAs(UnmanagedType.IDispatch)] object element)
    {
        CheckBlock(start, count, Length);
        return BinarySearchRight(
            start, count, ComUnwrap(element), default(TDefaultOrder));
    }
}