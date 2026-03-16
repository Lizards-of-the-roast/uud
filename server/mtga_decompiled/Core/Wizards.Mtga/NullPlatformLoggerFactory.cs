using Wizards.Models.ClientBusinessEvents;

namespace Wizards.Mtga;

public class NullPlatformLoggerFactory : IPlatformLoggerProvider
{
	public T Generate<T>(ClientBusinessEventType businessType) where T : IClientBusinessEventReq
	{
		return default(T);
	}
}
