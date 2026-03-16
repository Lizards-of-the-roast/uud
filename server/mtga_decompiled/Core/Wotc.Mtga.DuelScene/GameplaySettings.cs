using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public readonly struct GameplaySettings
{
	public readonly bool FullControlEnabled;

	public readonly bool FullControlLocked;

	public readonly bool AutoPassEnabled;

	public readonly bool ResolveAllEnabled;

	public readonly bool AutoPayManaEnabled;

	public readonly bool AutoSelectReplacementEffects;

	public GameplaySettings(SettingsMessage settingsMessage)
		: this(settingsMessage.FullControlEnabled(), settingsMessage.FullControlLocked(), settingsMessage.AutoPassEnabled(), settingsMessage.ResolveAllEnabled(), settingsMessage.AutoPayManaEnabled(), settingsMessage.AutoSelectReplacementEffects())
	{
	}

	public GameplaySettings(bool fullControlEnabled, bool fullControlLocked, bool autoPassEnabled, bool resolveAllEnabled, bool autoPayManaEnabled, bool autoSelectReplacementEffects)
	{
		FullControlEnabled = fullControlEnabled;
		FullControlLocked = fullControlLocked;
		AutoPassEnabled = autoPassEnabled;
		ResolveAllEnabled = resolveAllEnabled;
		AutoPayManaEnabled = autoPayManaEnabled;
		AutoSelectReplacementEffects = autoSelectReplacementEffects;
	}
}
