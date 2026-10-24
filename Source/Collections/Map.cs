using System.Runtime.InteropServices;
using Skynet.Sequence;

namespace Skynet.Collections;

[ComVisible(true)]
[Guid("C5EC7FFC-7C06-455C-96AA-41974AB25BAD")]
[ProgId("Skynet.Map")]
[ClassInterface(ClassInterfaceType.None)]
[ComDefaultInterface(typeof(IContainer))]
public sealed class Map : IContainer
{
    public Map()
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
