using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public interface ISettingsMessageProvider
{
	ObservableReference<SettingsMessage> Settings { get; }
}
