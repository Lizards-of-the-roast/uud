using GreClient.CardData;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class InjectStationChargeRequirement : IParameterizedInjector
{
	private const string CHARGE_REQUIREMENT_PARAM = "{chargeRequirement}";

	public string Inject(string value, ICardDataAdapter model, AbilityPrintingData ability)
	{
		foreach (AbilityPrintingData intrinsicAbility in model.IntrinsicAbilities)
		{
			if (intrinsicAbility.SubCategory == AbilitySubCategory.StationIntrinsicLevel && intrinsicAbility.Toughness.DefinedValue > 0 && !string.IsNullOrWhiteSpace(intrinsicAbility.LevelRequirement))
			{
				value = value.Replace("{chargeRequirement}", intrinsicAbility.LevelRequirement.Replace("+", string.Empty));
			}
		}
		return value;
	}
}
