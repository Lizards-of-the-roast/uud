using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class SettingsMessageGenerator : ISettingsMessageGenerator
{
	public SettingsMessage SetTurnAutoPass(SettingsMessage currentSettings, AutoPassOption autoPassOption, AutoPassOption defaultAutopassOption)
	{
		return new SettingsMessage(currentSettings)
		{
			AutoPassOption = autoPassOption,
			DefaultAutoPassOption = defaultAutopassOption
		};
	}
}
