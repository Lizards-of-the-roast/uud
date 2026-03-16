using System;

namespace Wotc.Mtga.Wrapper.PacketSelect;

public interface IServiceInterface
{
	ServiceState State { get; }

	Action<ServiceState> StateUpdated { get; set; }

	void SubmitPack(string packId);
}
