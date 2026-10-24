using System.Runtime.InteropServices;

namespace Skynet.Primitives;

[ComVisible(true)]
[Guid("508E74C3-58A4-4DC0-B4DC-CC5B07F709B0")]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
public interface IGeneric
{
	bool IsTypeSafe([In] DataType dataType);
}

[ComVisible(true)]
[Guid("5CA6A063-AF1F-493F-83DF-A3FE123B86D5")]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
public interface IValue : IGeneric
{
	DataType DataType { get; }

	object Value { get; }

	bool Equals([In] IValue other);

	[return: MarshalAs(UnmanagedType.BStr)]
	string ToString();
}

[ComVisible(false)]
public interface IValue<T> : IValue
{
	new T Value { get; }
}
