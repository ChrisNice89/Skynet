using System.Runtime.InteropServices;
using Skynet.Sequence;

namespace Skynet.Collections;

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[ComDefaultInterface(typeof(IIterator))]
internal sealed class VectorIterator : IIterator
{
    private readonly Vector _vector;
    private readonly int _end;
    private int _index;

    internal VectorIterator(Vector vector, int start, int end)
    {
        _vector = vector;
        _index = start;
        _end = end;
    }

    public DataType DataType => _vector.DataType;

    public bool IsTypeSafe(DataType dataType)
    {
        return dataType != Skynet.DataType.Unknown
            && _vector.DataType == dataType;
    }

    public bool MoveNext(
        [Out, MarshalAs(UnmanagedType.IDispatch)] out object value)
    {
        if (_index >= _end)
        {
            value = null!;
            return false;
        }

        value = _vector.GetObjectAt(_index);
        _index++;
        return true;
    }
}