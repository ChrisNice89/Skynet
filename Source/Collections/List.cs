using System;
using System.Runtime.InteropServices;
using Skynet.Sequence;
using Skynet.Source.Collections.Memory;

namespace Skynet.Collections;

#region COM List

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.AutoDual)]
public abstract class List : IContainer, IDisposable
{
    #region Properties

    public abstract int Count { get; }
    public abstract int Capacity { get; }
    public abstract DataType DataType { get; }

    public bool IsTypeSafe(DataType dataType)
        => dataType != DataType.Unknown && DataType == dataType;

    #endregion

    #region Element Access

    public void Add(
        [In, MarshalAs(UnmanagedType.IDispatch)] object value)
        => AddObject(value);

    public object GetAt(int index)
        => GetObjectAt(index);

    public void PutAt(
        int index,
        [In, MarshalAs(UnmanagedType.IDispatch)] object value)
        => PutObjectAt(index, value);

    protected abstract void AddObject(object value);
    protected abstract object GetObjectAt(int index);
    protected abstract void PutObjectAt(int index, object value);

    #endregion

    #region Reordering

    public abstract void Shuffle();

    #endregion

    #region Sorting

    public abstract void Sort();
    public abstract RunOrder IsOrdered();

    #endregion

    #region Iteration

    public abstract IIterator Iterator();

    #endregion

    public abstract void Dispose();
}

#endregion

#region Generic List

[ComVisible(false)]
internal sealed class List<T> : List
{
    #region Fields

    private Vector<T> _values;
    private int _count;

    #endregion

    #region Construction

    internal List(DataType dataType, int capacity)
        => _values = VectorFactory.Create<T>(dataType, capacity);

    #endregion

    #region Properties

    public override int Count => _count;
    public override int Capacity => _values.Length;
    public override DataType DataType => _values.DataType;

    #endregion

    #region Element Access

    public void Add(T value)
    {
        EnsureCapacity(_count + 1);
        _values.UnsafeAt(_count++, value);
    }

    public new T GetAt(int index)
    {
        CheckIndex(index);
        return _values.UnsafeAt(index);
    }

    public void PutAt(int index, T value)
    {
        CheckIndex(index);
        _values.UnsafeAt(index, value);
    }

    #endregion

    #region COM Conversion

    protected override void AddObject(object value)
        => Add(_values.ComUnwrap(value));

    protected override object GetObjectAt(int index)
    {
        CheckIndex(index);
        return _values.ComWrap(_values.UnsafeAt(index));
    }

    protected override void PutObjectAt(int index, object value)
    {
        CheckIndex(index);
        _values.UnsafeAt(index, _values.ComUnwrap(value));
    }

    #endregion

    #region Reordering

    public override void Shuffle()
        => _values.Shuffle(0, _count);

    #endregion

    #region Sorting

    public override void Sort()
        => _values.Sort(0, _count);

    public override RunOrder IsOrdered()
        => _values.IsOrdered(0, _count);

    #endregion

    #region Iteration

    public override IIterator Iterator()
        => _values.Iterator(0, _count);

    #endregion

    #region Capacity

    private void EnsureCapacity(int required)
    {
        if (required <= _values.Length)
            return;

        int capacity = _values.Length == 0
            ? 4
            : checked(_values.Length * 2);

        if (capacity < required)
            capacity = required;

        Resize(capacity);
    }

    private void Resize(int capacity)
    {
        Vector<T> replacement = VectorFactory.Create<T>(
            _values.DataType,
            capacity);

        _values.CopyToVector(0, replacement, 0, _count);
        _values = replacement;
    }

    #endregion

    #region Validation

    private void CheckIndex(int index)
    {
        if ((uint)index >= (uint)_count)
            throw new ArgumentOutOfRangeException(nameof(index));
    }

    #endregion

    public override void Dispose()
        => _values.Dispose();
}

#endregion