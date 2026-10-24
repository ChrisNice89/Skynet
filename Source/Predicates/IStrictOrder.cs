using System.Runtime.InteropServices;

namespace Skynet.Predicates;

[ComVisible(true)]
[Guid("F1369AF3-F85D-4A62-80E3-DAF36D2356C8")]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
internal interface IStrictOrder<T>
{
    bool IsLess(
        T left,
        T right);
}
