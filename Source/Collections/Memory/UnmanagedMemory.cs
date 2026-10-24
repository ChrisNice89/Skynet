using System;
using System.Runtime.InteropServices;

namespace Skynet.Memory;

internal unsafe sealed class UnmanagedMemory<T> :IMemoryAccess<T>,IDisposable
    where T : unmanaged
{
    private T* _data;
    private readonly int _length;

    internal UnmanagedMemory(int length)
    {
        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        _length = length;

        if (length != 0)
        {
            _data = (T*)Marshal.AllocHGlobal(
                checked(length * sizeof(T)));
        }

    }

    internal int Length => _length;

    /*
     * Vollständig ungeprüfter Zugriff.
     * Der Aufrufer garantiert:
     *
     * 0 <= index < Length
     */
    internal ref T At(int index)
    {
        return ref _data[index];
    }

    ref T IMemoryAccess<T>.At(int index)
    {
        return ref At(index);
    }

    /*
     * Kopiert ab Quellposition 0.
     * Keine eigene Bereichsprüfung.
     */
    internal void CopyTo(
        UnmanagedMemory<T> destination,
        int destinationIndex,
        int count)
    {
        CopyTo(
            sourceIndex: 0,
            destination,
            destinationIndex,
            count);
    }

    /*
     * Allgemeine Blockkopie.
     * Keine eigene Bereichsprüfung.
     */
    internal void CopyTo(
        int sourceIndex,
        UnmanagedMemory<T> destination,
        int destinationIndex,
        int count)
    {
        if (count == 0)
        {
            return;
        }

        long bytes =
            checked((long)count * sizeof(T));

        long destinationBytes =
            checked(
                (long)(destination._length - destinationIndex) *
                sizeof(T));

        Buffer.MemoryCopy(
            _data + sourceIndex,
            destination._data + destinationIndex,
            destinationBytes,
            bytes);
    }

    public void Dispose()
    {
        Release();
        GC.SuppressFinalize(this);
    }

    ~UnmanagedMemory()
    {
        Release();
    }

    private void Release()
    {
        if (_data == null)
        {
            return;
        }

        Marshal.FreeHGlobal((IntPtr)_data);
        _data = null;
    }

}

