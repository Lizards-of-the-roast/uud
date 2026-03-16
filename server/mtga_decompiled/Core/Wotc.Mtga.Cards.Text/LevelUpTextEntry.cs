namespace Wotc.Mtga.Cards.Text;

public class LevelUpTextEntry : ICardTextEntry
{
	private readonly string _abilityText;

	public readonly string LevelRequirement;

	public readonly StringBackedInt Power;

	public readonly StringBackedInt Toughness;

	public readonly bool IsFirstStationAbility;

	public readonly CardFrameKey PresentationColor;

	public readonly bool IsActive;

	public LevelUpTextEntry(string abilityText, string levelRequirement, StringBackedInt power, StringBackedInt toughness, bool isFirstStationAbility, CardFrameKey presentationColor, bool isActive)
	{
		_abilityText = abilityText ?? string.Empty;
		LevelRequirement = levelRequirement;
		Power = power;
		Toughness = toughness;
		IsFirstStationAbility = isFirstStationAbility;
		PresentationColor = presentationColor;
		IsActive = isActive;
	}

	public string GetText()
	{
		return _abilityText;
	}
}
