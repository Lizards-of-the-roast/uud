using System;
using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using SharedClientCore.SharedClientCore.Code.Providers;
using Wizards.Mtga;
using Wotc.Mtga.Cards.Database;

public class TitleCountManager : ITitleCountManager
{
	private ICardDatabaseAdapter _cardDatabaseAdapter;

	private IInventoryManager _inventoryManager;

	public Dictionary<uint, int> OwnedTitleCounts { get; private set; }

	public TitleCountManager(ICardDatabaseAdapter cardDataAdapter, IInventoryManager inventoryManager)
	{
		_cardDatabaseAdapter = cardDataAdapter;
		_inventoryManager = inventoryManager;
		_inventoryManager.CardsUpdated += BuildTitleCountCache;
		BuildTitleCountCache();
	}

	public static TitleCountManager Create()
	{
		return new TitleCountManager(Pantry.Get<ICardDatabaseAdapter>(), Pantry.Get<InventoryManager>());
	}

	private void BuildTitleCountCache()
	{
		OwnedTitleCounts = _inventoryManager.Cards.GroupBy(delegate(KeyValuePair<uint, int> kvp)
		{
			CardPrintingData item = SpecializeUtilities.GetBasePrinting(_cardDatabaseAdapter.CardDataProvider, kvp.Key).BasePrinting;
			string text = $"GrpId not found in CardData: {kvp.Key}";
			if (item == null)
			{
				SimpleLog.LogError(text);
				throw new Exception(text);
			}
			return item.TitleId;
		}).ToDictionary((IGrouping<uint, KeyValuePair<uint, int>> group) => group.Key, (IGrouping<uint, KeyValuePair<uint, int>> group) => group.Sum((KeyValuePair<uint, int> kvp) => kvp.Value));
	}
}
