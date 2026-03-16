using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public interface IActionHangerConfigProvider
{
	HangerConfig GetHangerConfig(Action action);

	bool TryGetHangerConfig(Action action, out HangerConfig hangerConfig)
	{
		hangerConfig = GetHangerConfig(action);
		return !hangerConfig.Equals(default(HangerConfig));
	}
}
