using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public interface ISettingsMessageGenerator
{
	SettingsMessage SetTurnAutoPass(SettingsMessage currentSettings, AutoPassOption autoPassOption, AutoPassOption defaultAutopassOption);
}
