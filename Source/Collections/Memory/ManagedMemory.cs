using System;
using Skynet.Memory;

namespace Skynet.Collections;

#region Managed

internal sealed class ManagedMemory<T> : IMemoryAccess<T>, IDisposable
{
    private readonly T[] _values;

    internal ManagedMemory(int length)
    {
        _values = new T[length];
    }

    internal int Length => _values.Length;

    public ref T At(int index)
    {
        return ref _values[index];
    }

    internal void CopyTo(
        int sourceIndex,
        ManagedMemory<T> destination,
        int destinationIndex,
        int count)
    {
        Array.Copy(
            _values,
            sourceIndex,
            destination._values,
            destinationIndex,
            count);
    }

    public void Dispose()
    {
        Array.Clear(
            _values,
            0,
            _values.Length);
    }
}

#endregion
