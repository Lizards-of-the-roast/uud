namespace Wotc.Mtga.Cards.Text;

public readonly struct LevelUpAbilityData
{
	public readonly string LevelRequirement;

	public readonly StringBackedInt Power;

	public readonly StringBackedInt Toughness;

	public readonly bool IsFirstGrantedAbility;

	public readonly CardFrameKey PresentationColor;

	public readonly bool IsActive;

	public LevelUpAbilityData(string levelRequirement, StringBackedInt power, StringBackedInt toughness, bool isFirstGrantedAbility, CardFrameKey presentationColor, bool isActive)
	{
		LevelRequirement = levelRequirement;
		Power = power;
		Toughness = toughness;
		IsFirstGrantedAbility = isFirstGrantedAbility;
		PresentationColor = presentationColor;
		IsActive = isActive;
	}
}
