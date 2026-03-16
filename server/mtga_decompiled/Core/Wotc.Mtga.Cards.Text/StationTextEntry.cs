namespace Wotc.Mtga.Cards.Text;

public class StationTextEntry : ICardTextEntry
{
	private readonly string _abilityText;

	public readonly string LevelRequirement;

	public readonly StringBackedInt Power;

	public readonly StringBackedInt Toughness;

	public readonly bool IsFirstStationAbility;

	public readonly CardFrameKey PresentationColor;

	public readonly bool IsActive;

	public bool DisplayPowerToughness
	{
		get
		{
			if (!Power.Equals(StringBackedInt.UNDEFINED))
			{
				return !Toughness.Equals(StringBackedInt.UNDEFINED);
			}
			return false;
		}
	}

	public StationTextEntry(string abilityText, string levelRequirement, StringBackedInt power, StringBackedInt toughness, bool isFirstStationAbility, CardFrameKey presentationColor, bool isActive)
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
