using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using GreClient.CardData;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Providers;

public class DisplayItemSleeve : DisplayItemCosmeticBase, IDisplayItemCosmetic<string>
{
	[SerializeField]
	private CardBackSelector sleeveVisual;

	private CardBackSelectorPopup _selector;

	private Action<string> _onCosmeticSelected;

	private Action _onDefaultCallback;

	private string _currentCardBack;

	private List<CardBackSelectorDisplayData> _cardBackSelectorDisplayData;

	[SerializeField]
	private MetaCardHolder _cardHolder;

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	private const string DefaultSleeveName = "CardBack_Default";

	private string _currentDefaultSleeve;

	private bool _showDefaultInterface;

	public void Init(Transform selectorTransform, CosmeticsProvider cosmeticsProvider, AssetLookupSystem assetLookupSystem, ICardRolloverZoom zoomHandler, IBILogger logger, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, StoreManager storeManager, bool isSideboarding)
	{
		string prefabPathFromALT = GetPrefabPathFromALT(assetLookupSystem);
		_selector = AssetLoader.Instantiate<CardBackSelectorPopup>(prefabPathFromALT, selectorTransform);
		_cardDatabase = cardDatabase;
		_cardViewBuilder = cardViewBuilder;
		_selector.Init(zoomHandler, logger, cardDatabase, cardViewBuilder, cosmeticsProvider, storeManager);
		_selector.SetCallbacks(OnCosmeticSelected, OnClose);
		_selector.OnStoreClicked(OnStoreSelected);
		_selector.SetOnDefaultCallback(OnDefaultCallback);
		if (_cardHolder != null)
		{
			_cardHolder.EnsureInit(cardDatabase, cardViewBuilder);
		}
		base.IsReadOnly = isSideboarding;
	}

	public void SetData(string currentCardBack, string defaultSleeve, bool showDefaultInterface, bool isReadOnly)
	{
		base.IsReadOnly = isReadOnly;
		_currentCardBack = currentCardBack;
		_currentDefaultSleeve = defaultSleeve ?? "CardBack_Default";
		_showDefaultInterface = showDefaultInterface;
		_selector.SetData(currentCardBack, defaultSleeve);
		string text = (string.IsNullOrEmpty(_currentCardBack) ? _currentDefaultSleeve : _currentCardBack);
		SetSleeveVisual(text);
	}

	public void SetData(string currentCardBack)
	{
		_currentCardBack = currentCardBack;
		_selector.SetData(currentCardBack, null);
		SetSleeveVisual(_currentCardBack);
	}

	public override void CloseSelector()
	{
		_selector.Close();
	}

	public override void OpenSelector()
	{
		if (!base.IsReadOnly)
		{
			base.OpenSelector();
			_selector.Open(_showDefaultInterface);
		}
	}

	public void SetOnCosmeticSelected(Action<string> onCosmeticSelected)
	{
		_onCosmeticSelected = onCosmeticSelected;
	}

	public void SetOnDefaultSelected(Action onDefaultCallback)
	{
		_onDefaultCallback = onDefaultCallback;
	}

	private string GetPrefabPathFromALT(AssetLookupSystem assetLookupSystem)
	{
		assetLookupSystem.Blackboard.Clear();
		string text = (assetLookupSystem.TreeLoader.LoadTree<CardBackSelectorPopupPrefab>()?.GetPayload(assetLookupSystem.Blackboard))?.PrefabPath;
		if (text == null)
		{
			SimpleLog.LogError("Could not find prefab for lookup: AvatarSelectPanelPrefab");
			return "";
		}
		return text;
	}

	private void OnDefaultCallback()
	{
		_onDefaultCallback?.Invoke();
	}

	private void OnCosmeticSelected(string sleeve)
	{
		_currentCardBack = sleeve;
		SetSleeveVisual(sleeve);
		_onCosmeticSelected?.Invoke(sleeve);
	}

	private void SetSleeveVisual(string sleeveName)
	{
		if (!(sleeveVisual == null))
		{
			CardData data = CardDataExtensions.CreateSkinCard(0u, _cardDatabase, null, sleeveName, faceDown: true);
			sleeveVisual.CardView.Init(_cardDatabase, _cardViewBuilder);
			sleeveVisual.CardView.SetData(data);
			sleeveVisual.CardView.Holder = _cardHolder;
			sleeveVisual.CDC = sleeveVisual.CardView.CardView;
		}
	}
}
