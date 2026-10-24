using System.Runtime.InteropServices;

namespace Skynet;

[ComVisible(true)]
public enum DataType
{
    Object = 0,
    Bool = 1,
    Int = 2,
    Float = 3,
    Numeric = 4,
    Date = 5,
    Chars = 6,
    Unknown = 7
}