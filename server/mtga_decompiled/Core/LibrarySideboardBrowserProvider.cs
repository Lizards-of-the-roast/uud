using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

public class LibrarySideboardBrowserProvider : IViewDismissBrowserProvider, ICardBrowserProvider, IDuelSceneBrowserProvider, ICardLock
{
	private GameManager gameManager;

	private Dictionary<string, ButtonStateData> buttonStateData = new Dictionary<string, ButtonStateData>();

	private LibrarySideboardBrowser openedBrowser;

	private readonly List<DuelScene_CDC> _cardsToDisplay = new List<DuelScene_CDC>();

	public string Header { get; private set; }

	public string SubHeader { get; private set; }

	public GREPlayerNum PlayerNum => GREPlayerNum.LocalPlayer;

	public Action<DuelScene_CDC> OnCardSelected { get; private set; }

	public bool LockCardDetails { get; set; }

	public bool ApplyTargetOffset => true;

	public bool ApplySourceOffset => false;

	public bool ApplyControllerOffset => false;

	public LibrarySideboardBrowserProvider(GameManager gameManager, Action<DuelScene_CDC> onCardSelected)
	{
		this.gameManager = gameManager;
		OnCardSelected = onCardSelected;
		MtgZone zone = this.gameManager.LatestGameState.GetZone(ZoneType.Library, GREPlayerNum.LocalPlayer);
		ButtonStateData buttonStateData = new ButtonStateData
		{
			StyleType = ButtonStyle.StyleType.MultiZone,
			BrowserElementKey = "ZoneButton0",
			Enabled = false,
			LocalizedString = Utils.GetLocalizedZoneKey(zone.Type, zone.Owner)
		};
		this.buttonStateData.Add(buttonStateData.BrowserElementKey, buttonStateData);
		zone = this.gameManager.LatestGameState.GetZone(ZoneType.Sideboard, GREPlayerNum.LocalPlayer);
		buttonStateData = new ButtonStateData
		{
			StyleType = ButtonStyle.StyleType.MultiZone,
			BrowserElementKey = "ZoneButton1",
			Enabled = true,
			LocalizedString = Utils.GetLocalizedZoneKey(zone.Type, zone.Owner)
		};
		this.buttonStateData.Add(buttonStateData.BrowserElementKey, buttonStateData);
		buttonStateData = new ButtonStateData
		{
			StyleType = ButtonStyle.StyleType.Main,
			BrowserElementKey = "SingleButton",
			Enabled = true,
			LocalizedString = "DuelScene/Browsers/ViewDismiss_Done"
		};
		this.buttonStateData.Add(buttonStateData.BrowserElementKey, buttonStateData);
		SelectZoneButton0();
	}

	public List<DuelScene_CDC> GetCardsToDisplay()
	{
		return _cardsToDisplay;
	}

	public DuelSceneBrowserType GetBrowserType()
	{
		return DuelSceneBrowserType.LibrarySideboard;
	}

	public Dictionary<string, ButtonStateData> GetButtonStateData()
	{
		return buttonStateData;
	}

	public BrowserCardHeader.BrowserCardHeaderData GetCardHeaderData(DuelScene_CDC cardView)
	{
		return null;
	}

	public string GetCardHolderLayoutKey()
	{
		return "SelectCards_MultiZone";
	}

	public void SetFxBlackboardData(IBlackboard bb)
	{
	}

	public void SetFxBlackboardDataForCard(DuelScene_CDC cardView, IBlackboard bb)
	{
	}

	public void SetOpenedBrowser(IBrowser browser)
	{
		openedBrowser = browser as LibrarySideboardBrowser;
		openedBrowser.ButtonPressedHandlers += OnButtonPressed;
		openedBrowser.ClosedHandlers += OnBrowserClosed;
		OnButtonPressed("ZoneButton0");
	}

	private void SelectZoneButton0()
	{
		MtgZone zone = gameManager.LatestGameState.GetZone(ZoneType.Library, GREPlayerNum.LocalPlayer);
		buttonStateData["ZoneButton0"].Enabled = false;
		buttonStateData["ZoneButton1"].Enabled = true;
		_cardsToDisplay.Clear();
		foreach (uint cardId in zone.CardIds)
		{
			if (gameManager.ViewManager.TryGetCardView(cardId, out var cardView))
			{
				_cardsToDisplay.Add(cardView);
			}
		}
		Header = gameManager.LocManager.GetLocalizedText("Enum/ZoneType/ZoneType_Library");
		SubHeader = ((_cardsToDisplay.Count != 1) ? gameManager.LocManager.GetLocalizedText("DuelScene/Browsers/Cards_Text", ("numCards", _cardsToDisplay.Count.ToString())) : gameManager.LocManager.GetLocalizedText("DuelScene/Browsers/Card_Text", ("numCards", _cardsToDisplay.Count.ToString())));
	}

	private void SelectZoneButton1()
	{
		MtgZone zone = gameManager.LatestGameState.GetZone(ZoneType.Sideboard, GREPlayerNum.LocalPlayer);
		buttonStateData["ZoneButton1"].Enabled = false;
		buttonStateData["ZoneButton0"].Enabled = true;
		_cardsToDisplay.Clear();
		MatchManager.PlayerInfo playerInfoForNum = gameManager.GetPlayerInfoForNum(GREPlayerNum.LocalPlayer);
		IEnumerable<uint> sideboardCards = playerInfoForNum.SideboardCards;
		List<uint> list = new List<uint>();
		if (gameManager.LatestGameState.GameInfo.SideboardLoadingEnabled)
		{
			foreach (uint distinctGrpId in sideboardCards.Distinct())
			{
				int a = sideboardCards.Count((uint x) => x == distinctGrpId);
				int b = zone.VisibleCards.Count((MtgCardInstance x) => x.GrpId == distinctGrpId);
				int num = Mathf.Min(a, b);
				for (int num2 = 0; num2 < num; num2++)
				{
					list.Add(distinctGrpId);
				}
			}
		}
		else
		{
			list.AddRange(sideboardCards);
		}
		foreach (uint grpId in list)
		{
			string skinCode = playerInfoForNum.CardStyles.Where((CardSkinTuple x) => x.CatalogId == grpId).FirstOrDefault()?.SkinCode;
			CardPrintingData cardPrintingById = gameManager.CardDatabase.CardDataProvider.GetCardPrintingById(grpId, skinCode);
			MtgCardInstance mtgCardInstance = new MtgCardInstance();
			mtgCardInstance.Zone = zone;
			mtgCardInstance.Controller = zone.Owner;
			mtgCardInstance.Owner = zone.Owner;
			mtgCardInstance.GrpId = grpId;
			mtgCardInstance.SkinCode = skinCode;
			mtgCardInstance.ObjectType = GameObjectType.Card;
			mtgCardInstance.CopyFromPrinting(cardPrintingById);
			CardData cardData = new CardData(mtgCardInstance, cardPrintingById);
			DuelScene_CDC item = gameManager.ViewManager.CreateCardView(cardData);
			_cardsToDisplay.Add(item);
		}
		Header = gameManager.LocManager.GetLocalizedText("Enum/ZoneType/ZoneType_Sideboard");
		SubHeader = ((_cardsToDisplay.Count != 1) ? gameManager.LocManager.GetLocalizedText("DuelScene/Browsers/Cards_Text", ("numCards", _cardsToDisplay.Count.ToString())) : gameManager.LocManager.GetLocalizedText("DuelScene/Browsers/Card_Text", ("numCards", _cardsToDisplay.Count.ToString())));
	}

	private void OnButtonPressed(string buttonKey)
	{
		switch (buttonKey)
		{
		case "SingleButton":
			openedBrowser.Close();
			break;
		case "ZoneButton0":
			SelectZoneButton0();
			openedBrowser.Refresh();
			break;
		case "ZoneButton1":
			SelectZoneButton1();
			openedBrowser.Refresh();
			break;
		}
	}

	private void OnBrowserClosed()
	{
		openedBrowser.ClosedHandlers -= OnBrowserClosed;
		openedBrowser.ButtonPressedHandlers -= OnButtonPressed;
	}
}
