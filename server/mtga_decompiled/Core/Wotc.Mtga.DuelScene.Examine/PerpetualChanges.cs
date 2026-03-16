using GreClient.CardData;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.DuelScene.Examine;

public class PerpetualChanges
{
	private ICardDatabaseAdapter _cardDatabase;

	public PerpetualChanges(ICardDatabaseAdapter cardDatabase)
	{
		_cardDatabase = cardDatabase;
	}

	public bool CanApplyPerpetualChanges(ICardDataAdapter model)
	{
		if (model == null)
		{
			return false;
		}
		if (model.Instance == null)
		{
			return false;
		}
		if (model.HasPerpetualChanges())
		{
			return !PerpetualChangeUtilities.DoesInstanceOnlyHavePerpetualDifferencesFromPrinting(model);
		}
		return false;
	}

	public ICardDataAdapter ApplyPerpetualChanges(ICardDataAdapter sourceModel, ICardDataAdapter convertedModel)
	{
		PerpetualChangeUtilities.CopyPerpetualEffects(sourceModel, convertedModel, _cardDatabase.AbilityDataProvider);
		return convertedModel;
	}

	public ICardDataAdapter ApplyPerpetualChangesToLinkedFaces(ICardDataAdapter sourceModel, ICardDataAdapter convertedModel)
	{
		for (int i = 0; i < convertedModel.LinkedFaceInstances.Count; i++)
		{
			CardPrintingData printing = convertedModel.LinkedFacePrintings[i];
			ICardDataAdapter convertedModel2 = new CardData(convertedModel.LinkedFaceInstances[i], printing);
			convertedModel2 = ApplyPerpetualChanges(sourceModel, convertedModel2);
			convertedModel.Instance.LinkedFaceInstances[i] = convertedModel2.Instance;
		}
		return convertedModel;
	}
}
