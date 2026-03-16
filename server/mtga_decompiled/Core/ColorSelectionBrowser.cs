using System;
using System.Collections.Generic;
using AssetLookupTree.Payloads.Prefab;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtgo.Gre.External.Messaging;

public class ColorSelectionBrowser : CardBrowserBase
{
	private BrowserHeader _browserHeader;

	private ColorSelectionBrowserProvider _colorSelectionProvider;

	private readonly List<ManaSpinner> _manaSpinners = new List<ManaSpinner>();

	private readonly Dictionary<string, ButtonStateData> _buttonStateData = new Dictionary<string, ButtonStateData>();

	private uint _currentSpinnerCount;

	public event Action<List<ManaColor>> ManaSelectionsMadeEvent;

	public ColorSelectionBrowser(BrowserManager browserManager, IDuelSceneBrowserProvider provider, GameManager gameManager)
		: base(browserManager, provider, gameManager)
	{
		_colorSelectionProvider = provider as ColorSelectionBrowserProvider;
		_currentSpinnerCount = _colorSelectionProvider.ColorCount;
	}

	protected override void InitializeUIElements()
	{
		base.InitializeUIElements();
		HorizontalLayoutGroup componentInChildren = GetBrowserElement("SpinnerContainer").GetComponentInChildren<HorizontalLayoutGroup>();
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.CardBrowserType = _colorSelectionProvider.GetBrowserType();
		_assetLookupSystem.Blackboard.DeviceType = PlatformUtils.GetCurrentDeviceType();
		_assetLookupSystem.Blackboard.AspectRatio = (float)Screen.width / (float)Screen.height;
		_assetLookupSystem.Blackboard.CardBrowserElementID = "ManaSpinner";
		BrowserElementPrefab payload = _assetLookupSystem.TreeLoader.LoadTree<BrowserElementPrefab>(returnNewTree: false).GetPayload(_assetLookupSystem.Blackboard);
		if (payload == null || payload.PrefabPath == null)
		{
			Debug.LogError("No spinner prefab found");
			return;
		}
		uint num = _colorSelectionProvider.ColorCount / (uint)_colorSelectionProvider.ManaColors.Count;
		uint num2 = _colorSelectionProvider.ColorCount % (uint)_colorSelectionProvider.ManaColors.Count;
		for (int i = 0; i < _colorSelectionProvider.ManaColors.Count; i++)
		{
			ManaSpinner manaSpinner = AssetLoader.Instantiate<ManaSpinner>(payload.PrefabPath, componentInChildren.transform);
			uint num3 = num;
			if (i == 0)
			{
				num3 += num2;
			}
			manaSpinner.Init(_colorSelectionProvider.ManaColors[i], num3);
			manaSpinner.UpEvent += OnSpinnerUpClicked;
			manaSpinner.DownEvent += OnSpinnerDownClicked;
			_manaSpinners.Add(manaSpinner);
		}
		_browserHeader = GetBrowserElement("Header").GetComponent<BrowserHeader>();
		_browserHeader.SetHeaderText(_colorSelectionProvider.BrowserHeaderText);
		_browserHeader.SetSubheaderText(string.Empty);
		UpdateButtons();
	}

	protected override void SetupCards()
	{
		if (_gameManager.LatestGameState.TryGetCard(_colorSelectionProvider.SourceId, out var card) && card.TitleId == 702851 && _gameManager.LatestGameState.LocalLibrary.CardIds.Count > 0)
		{
			uint cardId = _gameManager.LatestGameState.LocalLibrary.CardIds[0];
			if (entityViewManager.TryGetCardView(cardId, out var cardView))
			{
				cardViews.Add(cardView);
				MoveCardViewsToBrowser(cardViews);
			}
		}
	}

	protected override void ReleaseUIElements()
	{
		foreach (ManaSpinner manaSpinner in _manaSpinners)
		{
			manaSpinner.UpEvent -= OnSpinnerUpClicked;
			manaSpinner.DownEvent -= OnSpinnerDownClicked;
			manaSpinner.Cleanup();
			UnityEngine.Object.Destroy(manaSpinner.gameObject);
		}
		base.ReleaseUIElements();
	}

	private void OnSpinnerUpClicked(ManaSpinner spinner)
	{
		if (_currentSpinnerCount < _colorSelectionProvider.ColorCount)
		{
			spinner.SetCount(spinner.Count + 1);
			_currentSpinnerCount++;
			UpdateButtons();
			UpdateSubheader();
		}
	}

	private void OnSpinnerDownClicked(ManaSpinner spinner)
	{
		if (spinner.Count != 0)
		{
			spinner.SetCount(spinner.Count - 1);
			_currentSpinnerCount--;
			UpdateButtons();
			UpdateSubheader();
		}
	}

	public override void UpdateButtons()
	{
		foreach (ManaSpinner manaSpinner in _manaSpinners)
		{
			manaSpinner.SetUpArrowInteractable(_currentSpinnerCount != _colorSelectionProvider.ColorCount);
			manaSpinner.SetDownArrowInteractable(manaSpinner.Count != 0);
		}
		_buttonStateData.Clear();
		ButtonStateData buttonStateData = new ButtonStateData();
		buttonStateData.LocalizedString = "DuelScene/ClientPrompt/ClientPrompt_Button_Submit";
		buttonStateData.Enabled = _currentSpinnerCount == _colorSelectionProvider.ColorCount;
		buttonStateData.BrowserElementKey = (_colorSelectionProvider.CanCancel ? "2Button_Left" : "SingleButton");
		buttonStateData.StyleType = ButtonStyle.StyleType.Main;
		_buttonStateData.Add("Submit", buttonStateData);
		if (_colorSelectionProvider.CanCancel)
		{
			ButtonStateData buttonStateData2 = new ButtonStateData();
			buttonStateData2.LocalizedString = "DuelScene/ClientPrompt/ClientPrompt_Button_Cancel";
			buttonStateData2.Enabled = true;
			buttonStateData2.StyleType = ButtonStyle.StyleType.Secondary;
			buttonStateData2.BrowserElementKey = "2Button_Right";
			_buttonStateData.Add("Cancel", buttonStateData2);
		}
		UpdateButtons(_buttonStateData);
	}

	private void UpdateSubheader()
	{
		string subheaderText = string.Empty;
		uint num = _colorSelectionProvider.ColorCount - _currentSpinnerCount;
		switch (num)
		{
		case 1u:
			subheaderText = _gameManager.LocManager.GetLocalizedText("DuelScene/ColorSelector/ColorPoolStatus_Singular");
			break;
		default:
			subheaderText = _gameManager.LocManager.GetLocalizedText("DuelScene/ColorSelector/ColorPoolStatus", ("selectionCount", num.ToString()));
			break;
		case 0u:
			break;
		}
		_browserHeader.SetSubheaderText(subheaderText);
	}

	protected override void OnButtonCallback(string buttonKey)
	{
		base.OnButtonCallback(buttonKey);
		if (!(buttonKey == "Submit"))
		{
			if (buttonKey == "Cancel")
			{
				this.ManaSelectionsMadeEvent?.Invoke(null);
			}
		}
		else
		{
			Submit();
		}
	}

	private void Submit()
	{
		List<ManaColor> list = new List<ManaColor>();
		foreach (ManaSpinner manaSpinner in _manaSpinners)
		{
			for (int i = 0; i < manaSpinner.Count; i++)
			{
				list.Add(manaSpinner.Color);
			}
		}
		this.ManaSelectionsMadeEvent?.Invoke(list);
	}
}
