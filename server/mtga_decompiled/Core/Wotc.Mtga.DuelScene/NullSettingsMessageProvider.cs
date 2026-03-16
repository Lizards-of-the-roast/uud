using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class NullSettingsMessageProvider : ISettingsMessageProvider
{
	public static readonly ISettingsMessageProvider Default = new NullSettingsMessageProvider();

	public ObservableReference<SettingsMessage> Settings => null;
}
