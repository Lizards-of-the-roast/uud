using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public interface ISettingsMessageController
{
	void SetSettings(SettingsMessage settings);
}
