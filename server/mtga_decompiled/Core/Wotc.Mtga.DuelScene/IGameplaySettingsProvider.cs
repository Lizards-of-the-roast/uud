namespace Wotc.Mtga.DuelScene;

public interface IGameplaySettingsProvider
{
	GameplaySettings GameplaySettings { get; }

	bool FullControlEnabled => GameplaySettings.FullControlEnabled;

	bool FullControlDisabled => !GameplaySettings.FullControlEnabled;
}
