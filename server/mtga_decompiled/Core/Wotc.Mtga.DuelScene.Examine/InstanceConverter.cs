using System;
using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Examine;

public class InstanceConverter : IModelConverter
{
	public ICardDataAdapter ConvertModel(ICardDataAdapter sourceModel, ExamineState examineState = ExamineState.None)
	{
		if (sourceModel == null)
		{
			return null;
		}
		CardPrintingData cardPrintingData = ((!sourceModel.IsRoomChild()) ? sourceModel.Printing : sourceModel.LinkedFacePrintings[0]);
		if (cardPrintingData == null)
		{
			return null;
		}
		MtgCardInstance mtgCardInstance = sourceModel.Instance;
		if (mtgCardInstance == null)
		{
			mtgCardInstance = cardPrintingData.CreateInstance();
		}
		MtgCardInstance examineCopy = mtgCardInstance.GetExamineCopy();
		examineCopy.ManaCostOverride = (IReadOnlyCollection<ManaQuantity>)(object)Array.Empty<ManaQuantity>();
		CardPrintingData printing = cardPrintingData;
		return new CardData(examineCopy, printing)
		{
			RulesTextOverride = sourceModel.RulesTextOverride
		};
	}
}
