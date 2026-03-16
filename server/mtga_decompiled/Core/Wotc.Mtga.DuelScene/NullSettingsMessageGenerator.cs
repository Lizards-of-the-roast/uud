using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class NullSettingsMessageGenerator : ISettingsMessageGenerator
{
	public static readonly ISettingsMessageGenerator Default = new NullSettingsMessageGenerator();

	public SettingsMessage SetTurnAutoPass(SettingsMessage currentSettings, AutoPassOption autoPassOption, AutoPassOption defaultAutopassOption)
	{
		return null;
	}
}
