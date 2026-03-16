namespace Wotc.Mtga.DuelScene;

public class NullGameplaySettingsProvider : IGameplaySettingsProvider
{
	public static readonly IGameplaySettingsProvider Default = new NullGameplaySettingsProvider();

	public GameplaySettings GameplaySettings => default(GameplaySettings);
}
