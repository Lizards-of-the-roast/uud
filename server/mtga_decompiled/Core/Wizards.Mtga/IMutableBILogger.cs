using Wotc.Mtga.Network.ServiceWrappers;

namespace Wizards.Mtga;

public interface IMutableBILogger : IBILogger
{
	void SetFrontDoorServiceWrapper(IFrontDoorConnectionServiceWrapper fdcWrapper);

	void ClearFdServiceWrapper();
}
