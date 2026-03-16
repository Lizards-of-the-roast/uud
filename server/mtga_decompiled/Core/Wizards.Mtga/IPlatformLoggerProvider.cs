using Wizards.Models.ClientBusinessEvents;

namespace Wizards.Mtga;

public interface IPlatformLoggerProvider
{
	T Generate<T>(ClientBusinessEventType businessType) where T : IClientBusinessEventReq;
}
