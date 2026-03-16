using Core.Shared.Code.Network;
using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Shared.Code.ServiceFactories;

public class ChallengeServiceWrapperFactory
{
	public static IChallengeServiceWrapper Create()
	{
		return new ChallengeServiceWrapper(Pantry.Get<IFrontDoorConnectionServiceWrapper>());
	}
}
