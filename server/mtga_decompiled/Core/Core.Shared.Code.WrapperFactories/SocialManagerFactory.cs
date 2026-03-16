using MTGA.Social;

namespace Core.Shared.Code.WrapperFactories;

public static class SocialManagerFactory
{
	public static ISocialManager Create()
	{
		return SocialManagerHasbroGo.Create();
	}
}
