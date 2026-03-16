using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;

namespace Core.Shared.Code.ServiceFactories;

public class CosmeticsProviderFactory
{
	public static CosmeticsProvider Create()
	{
		return new CosmeticsProvider(Pantry.Get<ICosmeticsServiceWrapper>(), Pantry.Get<IBILogger>());
	}
}
