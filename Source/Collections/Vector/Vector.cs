using System;
using System.Runtime.InteropServices;
using Skynet.Sequence;
using Skynet.Source.Collections.Memory;

namespace Skynet.Collections;

#region COM

[ComVisible(true)]
[Guid("D88D6E8D-84C4-4BFC-949E-1BC2C558CB8D")]
[ClassInterface(ClassInterfaceType.AutoDual)]
public abstract class Vector : ISequence, IDisposable
{
    #region Fields

    private readonly int _length;
    private readonly DataType _dataType;

    #endregion

    #region Construction

    protected Vector(DataType dataType, int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        _dataType = dataType;
        _length = length;
    }

    #endregion

    #region Properties

    public DataType DataType => _dataType;
    public int Length => _length;

    public bool IsTypeSafe(DataType dataType)
        => dataType != DataType.Unknown && _dataType == dataType;

    #endregion

    #region Element Access

    public object GetAt(int index)
    {
        CheckIndex(index);
        return GetObjectAt(index);
    }

    public void PutAt(
        int index,
        [In, MarshalAs(UnmanagedType.IDispatch)] object value)
    {
        CheckIndex(index);
        SetObjectAt(index, value);
    }

    internal abstract object GetObjectAt(int index);
    internal abstract void SetObjectAt(int index, object value);

    #endregion

    #region Reordering

    public void Swap(int left, int right)
    {
        CheckIndex(left);
        CheckIndex(right);
        SwapItems(left, right);
    }

    public void Shuffle()
        => ShuffleRange(0, _length);

    public void Shuffle(int start, int count)
    {
        CheckBlock(start, count, _length);
        ShuffleRange(start, count);
    }

    internal abstract void SwapItems(int left, int right);
    internal abstract void ShuffleRange(int start, int count);

    #endregion

    #region Sorting

    public void Sort()
        => SortRange(0, _length);

    public void Sort(int start, int count)
    {
        CheckBlock(start, count, _length);
        SortRange(start, count);
    }

    public RunOrder IsOrdered()
        => IsRangeOrdered(0, _length);

    public RunOrder IsOrdered(int start, int count)
    {
        CheckBlock(start, count, _length);
        return IsRangeOrdered(start, count);
    }

    internal abstract void SortRange(int start, int count);
    internal abstract RunOrder IsRangeOrdered(int start, int count);

    #endregion

    #region Searching

    public int BinarySearchLeft(
        [In, MarshalAs(UnmanagedType.IDispatch)] object element)
        => BinarySearchLeft(0, _length, element);

    public abstract int BinarySearchLeft(
        int start,
        int count,
        [In, MarshalAs(UnmanagedType.IDispatch)] object element);

    public int BinarySearchRight(
        [In, MarshalAs(UnmanagedType.IDispatch)] object element)
        => BinarySearchRight(0, _length, element);

    public abstract int BinarySearchRight(
        int start,
        int count,
        [In, MarshalAs(UnmanagedType.IDispatch)] object element);

    #endregion

    #region Copy

    public void CopyTo(
        int sourceIndex,
        [In, MarshalAs(UnmanagedType.IDispatch)] Vector destination,
        int destinationIndex,
        int count)
    {
        if (destination is null)
            throw new ArgumentNullException(nameof(destination));

        CheckBlock(sourceIndex, count, _length);
        CheckBlock(destinationIndex, count, destination._length);

        CopyElements(sourceIndex, destination, destinationIndex, count);
    }

    internal abstract void CopyElements(
        int sourceIndex,
        Vector destination,
        int destinationIndex,
        int count);

    #endregion

    #region Iteration

    public IIterator Iterator()
        => Iterator(0, _length);

    public IIterator Iterator(int start, int end)
    {
        CheckIteratorRange(start, end);
        return new VectorIterator(this, start, end);
    }

    #endregion

    #region Validation

    private void CheckIndex(int index)
    {
        if ((uint)index >= (uint)_length)
            throw new ArgumentOutOfRangeException(nameof(index));
    }

    private void CheckIteratorRange(int start, int end)
    {
        if ((uint)start > (uint)_length)
            throw new ArgumentOutOfRangeException(nameof(start));

        if ((uint)end > (uint)_length)
            throw new ArgumentOutOfRangeException(nameof(end));

        if (start > end)
            throw new ArgumentException(
                "Start darf nicht größer als End sein.");
    }

    protected static void CheckBlock(int index, int count, int length)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (count < 0 || count > length - index)
            throw new ArgumentOutOfRangeException(nameof(count));
    }

    #endregion

    #region Disposal

    public abstract void Dispose();

    #endregion
}

#endregion