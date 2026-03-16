using System;

namespace Wotc.Mtga.Wrapper.PacketSelect;

public class NullServiceInterface : IServiceInterface
{
	public ServiceState State => new ServiceState(new PacketDetails[0], new PacketDetails[0]);

	public Action<ServiceState> StateUpdated { get; set; }

	public void SubmitPack(string packId)
	{
	}
}
