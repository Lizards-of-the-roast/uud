using System.Collections.Generic;
using AssetLookupTree.Payloads.Prefab;
using Pooling;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.DuelScene.Interactions.SelectN;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene.Browsers;

public class ButtonSelectionBrowser : ScrollListBrowser, IRefreshable
{
	private class LayoutWithABakedOffset : ICardLayout
	{
		private readonly ICardLayout _layout;

		private readonly Vector3 _centerOffset;

		public LayoutWithABakedOffset(ICardLayout actualLayout, Vector3 centerOffset)
		{
			_layout = actualLayout;
			_centerOffset = centerOffset;
		}

		public void GenerateData(List<DuelScene_CDC> allCardViews, ref List<CardLayoutData> allData, Vector3 center, Quaternion rotation)
		{
			_layout.GenerateData(allCardViews, ref allData, center + _centerOffset, rotation);
		}
	}

	private BrowserScrollList selectedScrollList;

	private ICardHolder _cardHolder;

	private List<DuelScene_CDC> _contextCards;

	private readonly LayoutWithABakedOffset _cardLayout = new LayoutWithABakedOffset(new CardLayout_Fan(), new Vector3(-6f, 0f, 0f));

	private IButtonSelectionBrowserProvider _provider;

	private readonly Dictionary<string, StyledButton> _listButtonsByKey = new Dictionary<string, StyledButton>();

	private readonly Dictionary<string, StyledButton> _selectedListButtonsByKey = new Dictionary<string, StyledButton>();

	private readonly ICardBuilder<DuelScene_CDC> _cardBuilder;

	private readonly IUnityObjectPool _objectPool;

	public ButtonSelectionBrowser(BrowserManager browserManager, IDuelSceneBrowserProvider provider, GameManager gameManager, ICardBuilder<DuelScene_CDC> cardBuilder)
		: base(browserManager, provider, gameManager)
	{
		_provider = provider as IButtonSelectionBrowserProvider;
		_cardHolder = gameManager.CardHolderManager.DefaultBrowser;
		_cardBuilder = cardBuilder ?? NullCardBuilder<DuelScene_CDC>.Default;
		_objectPool = Pantry.Get<IUnityObjectPool>();
	}

	protected override void InitializeUIElements()
	{
		base.InitializeUIElements();
		selectedScrollList = GetBrowserElement("SelectedScrollList").GetComponent<BrowserScrollList>();
		UpdateHeader();
		UpdateScrollLists();
		UpdateContextCards();
	}

	private void UpdateContextCards()
	{
		if (!(_provider is SelectNWorkflow_Counters selectNWorkflow_Counters))
		{
			return;
		}
		_contextCards = selectNWorkflow_Counters.GetCardsToDisplay();
		_cardHolder.Layout = _cardLayout;
		foreach (DuelScene_CDC contextCard in _contextCards)
		{
			_cardMovementController.MoveCard(contextCard, _cardHolder);
		}
	}

	private void UpdateHeader()
	{
		BrowserHeader component = GetBrowserElement("Header").GetComponent<BrowserHeader>();
		component.SetHeaderText(_provider.GetHeaderText());
		component.SetSubheaderText(_provider.GetSubHeaderText());
	}

	private void UpdateScrollLists()
	{
		Dictionary<string, ButtonStateData> scrollListButtonStateData = _provider.GetScrollListButtonStateData();
		UpdateScrollListButtons(scrollList, scrollListButtonStateData, _listButtonsByKey);
		Dictionary<string, ButtonStateData> selectedScrollListButtonStateData = _provider.GetSelectedScrollListButtonStateData();
		UpdateScrollListButtons(selectedScrollList, selectedScrollListButtonStateData, _selectedListButtonsByKey);
		selectedScrollList.gameObject.SetActive(selectedScrollListButtonStateData.Count > 0);
	}

	private void UpdateScrollListButtons(BrowserScrollList browserScrollList, Dictionary<string, ButtonStateData> scrollListButtonsByKey, Dictionary<string, StyledButton> cachedButtonsByKey)
	{
		List<string> list = new List<string>();
		foreach (string key2 in cachedButtonsByKey.Keys)
		{
			if (!scrollListButtonsByKey.ContainsKey(key2))
			{
				_objectPool.PushObject(cachedButtonsByKey[key2].gameObject);
				list.Add(key2);
			}
		}
		foreach (string item in list)
		{
			cachedButtonsByKey.Remove(item);
		}
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.CardBrowserElementID = "ButtonDefault";
		BrowserElementPrefab payload = _assetLookupSystem.TreeLoader.LoadTree<BrowserElementPrefab>(returnNewTree: false).GetPayload(_assetLookupSystem.Blackboard);
		if (payload == null || payload.PrefabPath == null)
		{
			Debug.LogError("No button payload found");
			return;
		}
		foreach (string key in scrollListButtonsByKey.Keys)
		{
			ButtonStateData buttonStateData = scrollListButtonsByKey[key];
			StyledButton styledButton = null;
			if (cachedButtonsByKey.ContainsKey(key))
			{
				styledButton = cachedButtonsByKey[key];
			}
			else
			{
				styledButton = _objectPool.PopObject(payload.PrefabPath).GetComponent<StyledButton>();
				styledButton.name = key;
				cachedButtonsByKey.Add(key, styledButton);
				styledButton.transform.ZeroOut();
				browserScrollList.AddListItem(styledButton.transform);
			}
			styledButton.Init(_assetLookupSystem);
			styledButton.SetModel(new PromptButtonData
			{
				ButtonText = buttonStateData.LocalizedString,
				ButtonIcon = buttonStateData.Sprite,
				Style = buttonStateData.StyleType,
				Enabled = buttonStateData.Enabled,
				ChildView = buttonStateData.ChildView,
				ButtonCallback = delegate
				{
					OnButtonCallback(key);
				}
			});
		}
		if (!_provider.SortButtonsByKey())
		{
			return;
		}
		List<string> list2 = new List<string>(cachedButtonsByKey.Keys);
		list2.Sort();
		foreach (KeyValuePair<string, StyledButton> item2 in cachedButtonsByKey)
		{
			item2.Value.transform.SetSiblingIndex(list2.IndexOf(item2.Key));
		}
	}

	public void Refresh()
	{
		UpdateScrollLists();
		UpdateButtons();
	}

	private void CleanupScrollListButtons()
	{
		foreach (StyledButton value in _listButtonsByKey.Values)
		{
			_objectPool.PushObject(value.gameObject);
		}
		_listButtonsByKey.Clear();
		foreach (StyledButton value2 in _selectedListButtonsByKey.Values)
		{
			_objectPool.PushObject(value2.gameObject);
		}
		_selectedListButtonsByKey.Clear();
	}

	private void CleanupContextCards()
	{
		if (_contextCards == null)
		{
			return;
		}
		List<DuelScene_CDC> list = new List<DuelScene_CDC>(_cardHolder.CardViews);
		for (int i = 0; i < list.Count; i++)
		{
			DuelScene_CDC duelScene_CDC = list[i];
			if ((bool)duelScene_CDC && (bool)duelScene_CDC.Root)
			{
				duelScene_CDC.gameObject.UpdateActive(active: true);
				if (duelScene_CDC.Model == null || duelScene_CDC.Model.Zone == null || duelScene_CDC.Model.InstanceId == 0 || !_gameManager.ViewManager.TryGetCardView(duelScene_CDC.InstanceId, out var _))
				{
					_cardHolder.RemoveCard(duelScene_CDC);
					_cardBuilder.DestroyCDC(duelScene_CDC);
				}
				else
				{
					_cardMovementController.MoveCard(duelScene_CDC, duelScene_CDC.Model.Zone);
				}
			}
		}
		list.Clear();
		_contextCards.Clear();
	}

	protected override void ReleaseUIElements()
	{
		CleanupScrollListButtons();
		CleanupContextCards();
		base.ReleaseUIElements();
	}
}
