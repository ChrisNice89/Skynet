using System.Runtime.InteropServices;
using Skynet.Sequence;

namespace Skynet.Collections;

[ComVisible(true)]
[Guid("E42594D3-ADC1-4648-8224-F60280CD9F30")]
[ProgId("Skynet.Lookup")]
[ClassInterface(ClassInterfaceType.None)]
[ComDefaultInterface(typeof(IContainer))]
public sealed class Lookup : IContainer
{
    public Lookup()
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
