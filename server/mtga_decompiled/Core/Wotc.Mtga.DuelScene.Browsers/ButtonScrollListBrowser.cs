using System.Collections.Generic;
using AssetLookupTree.Payloads.Prefab;
using Pooling;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene.Browsers;

public class ButtonScrollListBrowser : ScrollListBrowser
{
	private readonly IButtonScrollListBrowserProvider buttonScrollListProvider;

	private readonly Dictionary<string, StyledButton> listButtonsByKey = new Dictionary<string, StyledButton>();

	private readonly IUnityObjectPool _objectPool;

	public ButtonScrollListBrowser(BrowserManager browserManager, IDuelSceneBrowserProvider provider, GameManager gameManager)
		: base(browserManager, provider, gameManager)
	{
		buttonScrollListProvider = provider as IButtonScrollListBrowserProvider;
		_objectPool = Pantry.Get<IUnityObjectPool>();
	}

	protected override void InitializeUIElements()
	{
		base.InitializeUIElements();
		UpdateHeader();
		UpdateScrollListButtons();
	}

	private void UpdateHeader()
	{
		if (buttonScrollListProvider is IBrowserHeaderProvider browserHeaderProvider)
		{
			BrowserHeader component = GetBrowserElement("Header").GetComponent<BrowserHeader>();
			component.SetHeaderText(browserHeaderProvider.GetHeaderText());
			component.SetSubheaderText(browserHeaderProvider.GetSubHeaderText());
		}
	}

	private void UpdateScrollListButtons()
	{
		Dictionary<string, ButtonStateData> scrollListButtonDataByKey = buttonScrollListProvider.GetScrollListButtonDataByKey();
		List<string> list = new List<string>();
		foreach (string key2 in listButtonsByKey.Keys)
		{
			if (!scrollListButtonDataByKey.ContainsKey(key2))
			{
				_objectPool.PushObject(listButtonsByKey[key2].gameObject);
				list.Add(key2);
			}
		}
		foreach (string item in list)
		{
			listButtonsByKey.Remove(item);
		}
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.CardBrowserElementID = "ButtonDefault";
		BrowserElementPrefab payload = _assetLookupSystem.TreeLoader.LoadTree<BrowserElementPrefab>(returnNewTree: false).GetPayload(_assetLookupSystem.Blackboard);
		if (payload == null || payload.PrefabPath == null)
		{
			Debug.LogError("No button payload found");
			return;
		}
		foreach (string key in scrollListButtonDataByKey.Keys)
		{
			ButtonStateData buttonStateData = scrollListButtonDataByKey[key];
			StyledButton styledButton = null;
			if (listButtonsByKey.ContainsKey(key))
			{
				styledButton = listButtonsByKey[key];
			}
			else
			{
				styledButton = _objectPool.PopObject(payload.PrefabPath).GetComponent<StyledButton>();
				styledButton.name = key;
				listButtonsByKey.Add(key, styledButton);
				styledButton.transform.ZeroOut();
				scrollList.AddListItem(styledButton.transform);
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
		if (_duelSceneBrowserProvider.GetButtonStateData().Count == 0)
		{
			scrollList.HideScrollElementsWithScrollBar = true;
		}
	}

	private void CleanupScrollListButtons()
	{
		foreach (string key in listButtonsByKey.Keys)
		{
			_objectPool.PushObject(listButtonsByKey[key].gameObject);
		}
		listButtonsByKey.Clear();
	}

	protected override void ReleaseUIElements()
	{
		CleanupScrollListButtons();
		base.ReleaseUIElements();
	}
}
