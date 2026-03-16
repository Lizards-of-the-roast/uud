using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class NullSettingsMessageController : ISettingsMessageController
{
	public static readonly ISettingsMessageController Default = new NullSettingsMessageController();

	public void SetSettings(SettingsMessage settings)
	{
	}
}
