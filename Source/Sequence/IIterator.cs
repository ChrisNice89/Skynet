using System.Runtime.InteropServices;

namespace Skynet.Sequence;

[ComVisible(true)]
[Guid("E74AC014-BF73-4EE8-BAA9-E286615F0FE1")]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
public interface IIterator : Skynet.Primitives.IGeneric
{
     DataType DataType { get; }

     bool MoveNext([Out, MarshalAs(UnmanagedType.IDispatch)] out object value);
}
