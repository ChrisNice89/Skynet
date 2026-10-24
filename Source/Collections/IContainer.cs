using System.Runtime.InteropServices;
using Skynet.Sequence;

namespace Skynet.Collections;

[ComVisible(true)]
[Guid("D9774441-F18A-47B9-A58E-212F2B651F57")]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
public interface IContainer : ISequence
{
     int Count { get; }
}

