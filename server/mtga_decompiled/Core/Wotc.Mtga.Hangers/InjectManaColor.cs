using GreClient.CardData;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class InjectManaColor : IParameterizedInjector
{
	private const string COLOR_STRING = "{color}";

	private readonly IClientLocProvider _locProvider;

	public InjectManaColor(IClientLocProvider clientLocProvider)
	{
		_locProvider = clientLocProvider;
	}

	public string Inject(string value, ICardDataAdapter model, AbilityPrintingData ability)
	{
		string manaCost = ((ability is DynamicAbilityPrintingData dynamicAbilityPrintingData) ? dynamicAbilityPrintingData.Cost : ability.OldSchoolManaText);
		return GetColor(manaCost) switch
		{
			ManaColor.White => value.Replace("{color}", _locProvider.GetLocalizedText("AbilityHanger/Color/White")), 
			ManaColor.Blue => value.Replace("{color}", _locProvider.GetLocalizedText("AbilityHanger/Color/Blue")), 
			ManaColor.Black => value.Replace("{color}", _locProvider.GetLocalizedText("AbilityHanger/Color/Black")), 
			ManaColor.Red => value.Replace("{color}", _locProvider.GetLocalizedText("AbilityHanger/Color/Red")), 
			ManaColor.Green => value.Replace("{color}", _locProvider.GetLocalizedText("AbilityHanger/Color/Green")), 
			_ => value, 
		};
	}

	private ManaColor GetColor(string manaCost)
	{
		for (int i = 0; i < manaCost.Length; i++)
		{
			ManaColor manaColor = ManaUtilities.CharToColor(manaCost[i]);
			if (manaColor.IsWUBRG())
			{
				return manaColor;
			}
		}
		return ManaColor.None;
	}
}
