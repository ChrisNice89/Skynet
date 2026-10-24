using System.Runtime.InteropServices;
using Skynet.Sequence;

namespace Skynet.Collections;

[ComVisible(true)]
[Guid("35530903-A9DE-4530-BDB1-17B9D33BEF68")]
[ProgId("Skynet.OrderedMap")]
[ClassInterface(ClassInterfaceType.None)]
[ComDefaultInterface(typeof(IContainer))]
public sealed class OrderedMap : IContainer
{
    public OrderedMap()
    {
    }

    public int Count => throw new System.NotImplementedException();

    public DataType DataType => Skynet.DataType.Unknown;

    public bool IsTypeSafe(DataType dataType) => false;

    public IIterator Iterator()
    {
        throw new System.NotImplementedException();
    }
}
