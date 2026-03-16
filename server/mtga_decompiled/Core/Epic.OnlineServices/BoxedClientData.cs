using System.Runtime.InteropServices;

namespace Epic.OnlineServices;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal sealed class BoxedClientData
{
	public object ClientData { get; private set; }

	public BoxedClientData(object clientData)
	{
		ClientData = clientData;
	}
}
