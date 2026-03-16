using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using Core.Code.Decks;
using Core.Shared.Code;
using GreClient.CardData;
using Pooling;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.Interactions.ActionsAvailable.ManaSelections;
using Wotc.Mtga.DuelScene.UI;
using Wotc.Mtgo.Gre.External.Messaging;

public class SpecializePopup : PopupBase
{
	[SerializeField]
	private CascadingManaWheel _manaWheel;

	[SerializeField]
	private CustomButton _dismissButton;

	private IAbilityDataProvider _abilityDataProvider;

	private System.Action _onDismissed;

	private ICardRolloverZoom _zoomHandler;

	private DeckBuilderModelProvider ModelProvider => Pantry.Get<DeckBuilderModelProvider>();

	private ICardRolloverZoom ZoomHandler
	{
		get
		{
			if (_zoomHandler == null)
			{
				return Pantry.Get<ICardRolloverZoom>();
			}
			return _zoomHandler;
		}
	}

	public void Init(Camera camera, IUnityObjectPool unityObjectPool, IObjectPool objectPool, AssetLookupSystem assetLookupSystem, IAbilityDataProvider cardDb, ICardRolloverZoom zoomHandler)
	{
		_abilityDataProvider = cardDb;
		_manaWheel.Init(camera, unityObjectPool, objectPool, assetLookupSystem);
		_dismissButton.OnClick.AddListener(delegate
		{
			_onDismissed?.Invoke();
		});
		_zoomHandler = zoomHandler;
	}

	public void OpenSelector(CardData baseCard, Action<CardData> onFacetSelected, System.Action onDismissed)
	{
		List<ManaColor> coloredManaInCost = GetColoredManaInCost(baseCard.Printing);
		List<Wotc.Mtgo.Gre.External.Messaging.Action> list = new List<Wotc.Mtgo.Gre.External.Messaging.Action>();
		Dictionary<ManaColor, CardPrintingData> colorToFacetTable = new Dictionary<ManaColor, CardPrintingData>();
		foreach (CardPrintingData specializeFacet in SpecializeUtilities.GetSpecializeFacets(baseCard.Printing))
		{
			List<ManaColor> coloredManaInCost2 = GetColoredManaInCost(specializeFacet);
			foreach (ManaColor item2 in coloredManaInCost)
			{
				coloredManaInCost2.Remove(item2);
			}
			ManaColor manaColor = coloredManaInCost2[0];
			Wotc.Mtgo.Gre.External.Messaging.Action item = CreateAction(manaColor);
			colorToFacetTable[manaColor] = specializeFacet;
			list.Add(item);
		}
		ManaSelectionFlow manaSelectionFlow = new ManaSelectionFlow(0u, _abilityDataProvider);
		manaSelectionFlow.CreateTrees(list);
		_manaWheel.OpenSelector(manaSelectionFlow, base.transform, default(ManaColorSelector.ManaColorSelectorConfig), delegate(IReadOnlyCollection<ManaColor> selectedColors)
		{
			ManaColor key = selectedColors.First();
			CardData obj = new CardData(null, colorToFacetTable[key]);
			onFacetSelected?.Invoke(obj);
		});
		_onDismissed = onDismissed;
	}

	public override void OnEnter()
	{
	}

	public override void OnEscape()
	{
		_onDismissed?.Invoke();
	}

	protected override void Hide()
	{
		_manaWheel.CloseSelector();
		base.Hide();
	}

	private static Wotc.Mtgo.Gre.External.Messaging.Action CreateAction(ManaColor color)
	{
		ManaPaymentOption manaPaymentOption = new ManaPaymentOption();
		manaPaymentOption.Mana.Add(new ManaInfo
		{
			Color = color,
			Count = 1u
		});
		return new Wotc.Mtgo.Gre.External.Messaging.Action
		{
			ManaPaymentOptions = { manaPaymentOption }
		};
	}

	private static List<ManaColor> GetColoredManaInCost(CardPrintingData printing)
	{
		List<ManaColor> list = new List<ManaColor>();
		foreach (ManaQuantity item in printing.CastingCost)
		{
			if (item.Color == ManaColor.White || item.Color == ManaColor.Blue || item.Color == ManaColor.Black || item.Color == ManaColor.Red || item.Color == ManaColor.Green)
			{
				list.Add(item.Color);
			}
		}
		return list;
	}

	public void OnCommanderAdded(DeckBuilderPile pile, CardData card)
	{
		if (pile == DeckBuilderPile.Commander && SpecializeUtilities.IsSpecializeBaseCard(card.Printing))
		{
			Pantry.Get<GlobalCoroutineExecutor>().StartGlobalCoroutine(Coroutine_ShowSpecializePopup(card, ZoomHandler));
		}
	}

	private IEnumerator Coroutine_ShowSpecializePopup(CardData card, ICardRolloverZoom zoomHandler)
	{
		Activate(activate: true);
		yield return null;
		OpenSelector(card, delegate(CardData selectedFacet)
		{
			Activate(activate: false);
			ModelProvider.RemoveCardFromPile(DeckBuilderPile.Commander, zoomHandler, card);
			ModelProvider.AddCardToDeckPile(DeckBuilderPile.Commander, selectedFacet, zoomHandler, fromSpecializePopup: true);
		}, delegate
		{
			Activate(activate: false);
			ModelProvider.RemoveCardFromPile(DeckBuilderPile.Commander, zoomHandler, card);
		});
	}

	public void OnDestroy()
	{
		ModelProvider.CardModifiedInPile -= OnCommanderAdded;
	}
}
