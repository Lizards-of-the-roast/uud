using Wotc.Mtga.DuelScene;

namespace GreClient.Test;

public class TestGameplaySettingsProvider : IGameplaySettingsProvider
{
	public GameplaySettings GameplaySettings { get; private set; }

	public void SetSettings(bool fullControlEnabled = false, bool fullControlLocked = false, bool autoPassEnabled = false, bool resolveAllEnabled = false, bool autoPayManaEnabled = false, bool autoSelectReplacementEffects = false)
	{
		GameplaySettings = new GameplaySettings(fullControlEnabled, fullControlLocked, autoPassEnabled, resolveAllEnabled, autoPayManaEnabled, autoSelectReplacementEffects);
	}
}
