using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class NullActionHangerConfigProvider : IActionHangerConfigProvider
{
	public static readonly IActionHangerConfigProvider Default = new NullActionHangerConfigProvider();

	public HangerConfig GetHangerConfig(Action action)
	{
		return default(HangerConfig);
	}
}
