using System;
using System.Collections.Generic;
using Core.Code.AssetLookupTree.AssetLookup;
using Core.Meta.MainNavigation.BoosterChamber;
using GreClient.CardData;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using com.wizards.harness;

namespace Core.Code.Harnesses;

public class BoosterCardSpawnerHarness : Harness<BoosterOpenToScrollListController, HarnessDropdownUI>
{
	[Serializable]
	public class BoosterCardSpawnerViewModel : HarnessViewModel
	{
		public int cardCount;
	}

	private AssetLookupManager _assetLookupManager;

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	private ICardRolloverZoom _cardRolloverZoomBase;

	[SerializeField]
	private BoosterCardSpawnerViewModel[] _viewModels;

	public override void Initialize(Transform root, Transform uiRoot)
	{
		base.Initialize(root, uiRoot);
		_UI.Init(HarnessUICore.CreateOptions(_viewModels), OnValueChanged);
		_assetLookupManager = Pantry.Get<AssetLookupManager>();
		_cardDatabase = Pantry.Get<CardDatabase>();
		_cardViewBuilder = Pantry.Get<CardViewBuilder>();
		_cardRolloverZoomBase = Pantry.Get<ICardRolloverZoom>();
		AudioManager.InitializeAudio(_assetLookupManager.AssetLookupSystem);
	}

	public override void DisplayViewForIndex(int index)
	{
		base.DisplayViewForIndex(index);
		HarnessUICore.SetValueForDropdown(_UI.Dropdown, _UI.DropdownIndex);
		BoosterMetaCardViewPool cardViewPool = new BoosterMetaCardViewPool(new BoosterMetaCardView(), _cardDatabase, _cardViewBuilder);
		_Instance.Init(_cardDatabase, _cardViewBuilder, _cardRolloverZoomBase, delegate
		{
		}, cardViewPool);
	}

	private void OnValueChanged(int newIndex)
	{
		int cardCount = _viewModels[newIndex].cardCount;
		List<CardData> list = new List<CardData>();
		List<uint> list2 = new List<uint>
		{
			81841u, 72278u, 70272u, 81804u, 81890u, 81893u, 83474u, 83475u, 71283u, 77119u,
			19577u, 22911u, 64261u, 64663u, 77106u, 77107u, 77108u, 77109u
		};
		for (int i = 0; i < cardCount; i++)
		{
			int index = i % list2.Count;
			CardPrintingData cardPrintingById = _cardDatabase.CardDataProvider.GetCardPrintingById(list2[index]);
			CardData item = new CardData(cardPrintingById.CreateInstance(), cardPrintingById);
			list.Add(item);
		}
		List<CardDataAndRevealStatus> cardsToDisplay = BoosterOpenCardDataHelper.AddRevealStatusAndRebalancedCardsToCardData(BoosterOpenCardDataHelper.SortCardsByRarity(list));
		_Instance.SetCardsToDisplay(cardsToDisplay);
	}
}
