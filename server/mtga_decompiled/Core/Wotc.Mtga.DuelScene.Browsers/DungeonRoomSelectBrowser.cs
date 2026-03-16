using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.Interactions.SelectN;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene.Browsers;

public class DungeonRoomSelectBrowser : CardBrowserBase
{
	private const string FAKE_CARD_KEY = "DungeonRoomSelectionCdc";

	private readonly DungeonRoomSelectWorkflow _workflow;

	private CardData _dungeonCardData;

	private DuelScene_CDC _dungeonCdc;

	private CDCPart_TextBox_Dungeon _dungeonTextbox;

	private ICardDatabaseAdapter _cardDatabase;

	private IEntityViewManager _entityViewManager;

	public DungeonRoomSelectBrowser(BrowserManager browserManager, IDuelSceneBrowserProvider provider, GameManager gameManager)
		: base(browserManager, provider, gameManager)
	{
		_workflow = provider as DungeonRoomSelectWorkflow;
		_cardDatabase = gameManager.Context.Get<ICardDatabaseAdapter>();
		_entityViewManager = gameManager.Context.Get<IEntityViewManager>();
		base.AllowsHoverInteractions = false;
	}

	protected override void InitializeUIElements()
	{
		BrowserHeader component = GetBrowserElement("Header").GetComponent<BrowserHeader>();
		component.SetHeaderText(_workflow.GetHeaderText());
		component.SetSubheaderText(_workflow.GetSubHeaderText());
		base.InitializeUIElements();
	}

	protected override void ReleaseCards()
	{
		if ((bool)_dungeonCdc)
		{
			_entityViewManager.DeleteFakeCard("DungeonRoomSelectionCdc");
			_dungeonCdc = null;
			_dungeonTextbox = null;
		}
		base.ReleaseCards();
	}

	protected override void SetupCards()
	{
		CreateFakeCardData();
		_dungeonCdc = _entityViewManager.CreateFakeCard("DungeonRoomSelectionCdc", _dungeonCardData, isVisible: true);
		_dungeonCdc.Collider.enabled = false;
		_dungeonTextbox = _dungeonCdc.GetComponentInChildren<CDCPart_TextBox_Dungeon>(includeInactive: true);
		UpdateDungeonCdc();
		cardViews = _workflow.GetCardsToDisplay();
		cardViews.Add(_dungeonCdc);
		MoveCardViewsToBrowser(cardViews);
	}

	private void CreateFakeCardData()
	{
		CardPrintingData obj = _cardDatabase.CardDataProvider.GetCardPrintingById(_workflow.CurrentDungeonData.DungeonGrpId) ?? CardPrintingData.Blank;
		CardPrintingRecord record = obj.Record;
		IReadOnlyDictionary<uint, IReadOnlyList<uint>> abilityIdToLinkedTokenGrpId = DictionaryExtensions.Empty<uint, IReadOnlyList<uint>>();
		CardPrintingData cardPrintingData = new CardPrintingData(obj, new CardPrintingRecord(record, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, abilityIdToLinkedTokenGrpId));
		MtgCardInstance mtgCardInstance = cardPrintingData.CreateInstance();
		mtgCardInstance.Zone = null;
		mtgCardInstance.Abilities.Clear();
		_dungeonCardData = new CardData(mtgCardInstance, cardPrintingData);
	}

	private void OnDungeonRoomSelected(uint roomId)
	{
		_workflow.OnDungeonRoomSelected(roomId);
		UpdateDungeonCdc();
		UpdateButtons();
	}

	public void UpdateDungeonCdc()
	{
		_dungeonTextbox.SetDungeonRoomInteractions(_workflow.SelectableIds, _workflow.SelectedIds, _workflow.ActiveIds, OnDungeonRoomSelected);
	}
}
