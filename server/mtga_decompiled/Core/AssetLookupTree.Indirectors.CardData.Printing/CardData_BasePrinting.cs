using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Indirectors.CardData.Printing;

public class CardData_BasePrinting : IIndirector
{
	private ICardDataAdapter _cacheCardData;

	public void SetCache(IBlackboard bb)
	{
		_cacheCardData = bb.CardData;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.SetCardDataRaw(_cacheCardData);
		_cacheCardData = null;
	}

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.CardData != null && bb.CardData.Instance != null)
		{
			CardPrintingData cardPrintingById = bb.CardDataProvider.GetCardPrintingById(bb.CardData.Instance.BaseGrpId);
			if (cardPrintingById != null)
			{
				bb.SetCardDataExtensive(new GreClient.CardData.CardData(bb.CardData.Instance, cardPrintingById));
				yield return bb;
			}
		}
	}
}
