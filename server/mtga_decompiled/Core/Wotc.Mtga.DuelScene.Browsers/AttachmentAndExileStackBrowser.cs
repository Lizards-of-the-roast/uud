using System.Collections.Generic;
using AssetLookupTree.Payloads.Prefab;
using UnityEngine;
using UnityEngine.UI;

namespace Wotc.Mtga.DuelScene.Browsers;

public class AttachmentAndExileStackBrowser : CardBrowserBase
{
	private AttachmentAndExileStackBrowserProvider _browserProvider;

	private BrowserHeader _browserHeader;

	private Scrollbar _scrollbar;

	private int _totalGroupCount;

	private string _groupHeaderPrefab;

	private readonly Dictionary<AttachmentAndExileStackGroupData, BrowserCardHeader> _groupDataToHeader = new Dictionary<AttachmentAndExileStackGroupData, BrowserCardHeader>();

	private readonly List<DuelScene_CDC> tempVisibleCardsList = new List<DuelScene_CDC>();

	public AttachmentAndExileStackBrowser(BrowserManager browserManager, IDuelSceneBrowserProvider provider, GameManager gameManager)
		: base(browserManager, provider, gameManager)
	{
		base.AllowsHoverInteractions = true;
		_browserProvider = (AttachmentAndExileStackBrowserProvider)provider;
		scrollableLayout = new CardLayout_ScrollableBrowser(GetCardHolderLayoutData(_browserProvider.GetCardHolderLayoutKey()));
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.CardBrowserElementID = "AttachmentAndExileStackGroupHeader";
		BrowserElementPrefab payload = _assetLookupSystem.TreeLoader.LoadTree<BrowserElementPrefab>(returnNewTree: false).GetPayload(_assetLookupSystem.Blackboard);
		_groupHeaderPrefab = payload.PrefabPath;
	}

	public override Transform GetBrowserRoot()
	{
		return BrowserManager.ViewDismissRoot;
	}

	protected override void InitCardHolder()
	{
		base.InitCardHolder();
		cardHolder.PostLayoutEvent += OnPostCardHolderLayout;
	}

	protected override void SetupCards()
	{
		cardViews = new List<DuelScene_CDC>();
		_groupDataToHeader.Clear();
		_totalGroupCount = 0;
		AddCards(_browserProvider.GetGroupData(), ref _totalGroupCount);
		scrollableLayout.ArtificialSpacers.RemoveAt(scrollableLayout.ArtificialSpacers.Count - 1);
		MoveCardViewsToBrowser(cardViews);
	}

	private void AddCards(List<AttachmentAndExileStackGroupData> groupDataList, ref int currentGroupCount)
	{
		if (groupDataList != null)
		{
			for (int i = 0; i < groupDataList.Count; i++)
			{
				AttachmentAndExileStackGroupData attachmentAndExileStackGroupData = groupDataList[i];
				BrowserCardHeader component = _unityObjectPool.PopObject(_groupHeaderPrefab).GetComponent<BrowserCardHeader>();
				component.SetText(attachmentAndExileStackGroupData.HeaderData);
				component.gameObject.transform.SetParent(cardHolder.transform);
				component.gameObject.SetActive(value: false);
				_groupDataToHeader.Add(attachmentAndExileStackGroupData, component);
				cardViews.AddRange(attachmentAndExileStackGroupData.Cards);
				scrollableLayout.ArtificialSpacers.Add(cardViews.Count + currentGroupCount);
				currentGroupCount++;
				AddCards(attachmentAndExileStackGroupData.Children, ref currentGroupCount);
			}
		}
	}

	protected override void InitializeUIElements()
	{
		_browserHeader = GetBrowserElement("Header").GetComponent<BrowserHeader>();
		_browserHeader.SetHeaderText(_browserProvider.GetHeaderText());
		_browserHeader.SetSubheaderText(_browserProvider.GetSubHeaderText());
		_scrollbar = GetBrowserElement("Scrollbar").GetComponent<Scrollbar>();
		_scrollbar.onValueChanged.RemoveAllListeners();
		_scrollbar.onValueChanged.AddListener(OnScroll);
		_scrollbar.value = scrollableLayout.ScrollPosition;
		_scrollbar.gameObject.SetActive(cardViews.Count + (_totalGroupCount - 1) > scrollableLayout.FrontCount);
		ScrollWheelInputForScrollbar component = GetBrowserElement("Scrollbar").GetComponent<ScrollWheelInputForScrollbar>();
		component.ElementsFeatured = scrollableLayout.FrontCount;
		component.ElementCount = cardViews.Count;
		base.InitializeUIElements();
	}

	protected override void ReleaseCards()
	{
		foreach (BrowserCardHeader value in _groupDataToHeader.Values)
		{
			_unityObjectPool.PushObject(value.gameObject);
		}
		_groupDataToHeader.Clear();
		base.ReleaseCards();
	}

	private void OnPostCardHolderLayout(List<CardLayoutData> layoutDataList)
	{
		Dictionary<DuelScene_CDC, CardLayoutData> dictionary = new Dictionary<DuelScene_CDC, CardLayoutData>();
		foreach (CardLayoutData layoutData in layoutDataList)
		{
			dictionary.Add(layoutData.Card, layoutData);
		}
		foreach (KeyValuePair<AttachmentAndExileStackGroupData, BrowserCardHeader> item in _groupDataToHeader)
		{
			tempVisibleCardsList.Clear();
			foreach (DuelScene_CDC card in item.Key.Cards)
			{
				int num = cardViews.IndexOf(card);
				if (num >= scrollableLayout.PiledRight && num < cardViews.Count - scrollableLayout.PiledLeft)
				{
					tempVisibleCardsList.Add(card);
				}
			}
			if (tempVisibleCardsList.Count == 0)
			{
				item.Value.gameObject.SetActive(value: false);
				continue;
			}
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			foreach (DuelScene_CDC tempVisibleCards in tempVisibleCardsList)
			{
				if (dictionary.TryGetValue(tempVisibleCards, out var value))
				{
					zero += value.Position;
					zero2 += value.Scale;
				}
			}
			zero /= (float)tempVisibleCardsList.Count;
			zero2 /= (float)tempVisibleCardsList.Count;
			Transform transform = item.Value.transform;
			transform.localPosition = zero;
			transform.localScale = zero2;
			if (tempVisibleCardsList.Count > 0 && dictionary.TryGetValue(tempVisibleCardsList[0], out var value2) && dictionary.TryGetValue(tempVisibleCardsList[tempVisibleCardsList.Count - 1], out var value3))
			{
				transform.localRotation = Quaternion.Lerp(value2.Rotation, value3.Rotation, 0.5f);
			}
			item.Value.gameObject.SetActive(value: true);
		}
	}

	public override void Close()
	{
		if (cardHolder != null)
		{
			cardHolder.PostLayoutEvent -= OnPostCardHolderLayout;
		}
		base.Close();
	}
}
