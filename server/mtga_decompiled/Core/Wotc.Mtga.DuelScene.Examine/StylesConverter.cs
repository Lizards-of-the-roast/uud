using GreClient.CardData;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.DuelScene.Examine;

public class StylesConverter : IModelConverter
{
	private readonly ICardDataProvider _cardDataProvider;

	private readonly IAbilityDataProvider _abilityDataProvider;

	public StylesConverter(ICardDataProvider cardDataProvider, IAbilityDataProvider abilityDataProvider)
	{
		_cardDataProvider = cardDataProvider ?? NullCardDataProvider.Default;
		_abilityDataProvider = abilityDataProvider ?? new NullAbilityDataProvider();
	}

	public ICardDataAdapter ConvertModel(ICardDataAdapter sourceModel, ExamineState examineState = ExamineState.None)
	{
		if (sourceModel == null)
		{
			return null;
		}
		CardPrintingData printing = sourceModel.Printing;
		if (printing == null)
		{
			return null;
		}
		if (sourceModel.Instance == null)
		{
			printing.CreateInstance();
		}
		return CardSimplifier.Simplify(CardSimplifier.Context.Examine, sourceModel, _cardDataProvider, _abilityDataProvider);
	}
}
