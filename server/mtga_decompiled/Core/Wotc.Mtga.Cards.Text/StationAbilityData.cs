namespace Wotc.Mtga.Cards.Text;

public readonly struct StationAbilityData
{
	public readonly uint ParentAbilityId;

	public readonly string LevelRequirement;

	public readonly StringBackedInt Power;

	public readonly StringBackedInt Toughness;

	public readonly bool IsFirstStationAbility;

	public readonly CardFrameKey PresentationColor;

	public readonly bool IsActive;

	public StationAbilityData(uint parentId, string levelRequirement, StringBackedInt power, StringBackedInt toughness, bool isFirstStationAbility, CardFrameKey presentationColor, bool isActive)
	{
		ParentAbilityId = parentId;
		LevelRequirement = levelRequirement;
		Power = power;
		Toughness = toughness;
		IsFirstStationAbility = isFirstStationAbility;
		PresentationColor = presentationColor;
		IsActive = isActive;
	}
}
