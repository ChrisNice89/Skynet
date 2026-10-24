
using System.Runtime.InteropServices;

namespace Skynet.Sequence;

[ComVisible(true)]
[Guid("75D8F033-84D7-4C00-A580-EE3FB7ED5BB5")]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
public interface ISequence : Skynet.Primitives.IGeneric
{
    DataType DataType { get; }

    IIterator Iterator();
}