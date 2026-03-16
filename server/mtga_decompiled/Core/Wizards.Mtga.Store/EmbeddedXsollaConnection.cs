using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Code.AssetLookupTree.AssetLookup;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using ZenFulcrum.EmbeddedBrowser;

namespace Wizards.Mtga.Store;

public class EmbeddedXsollaConnection : XsollaConnection, INewWindowHandler
{
	private const string ZFBROWSER_PARENT_ID = "Parent";

	private const string ZFBROWSER_WINDOW_ID = "Window";

	private XsollaPurchasePrompt _purchasePrompt;

	private XsollaMainBrowser _mainBrowser;

	private GameObject _parentBrowserPrefab;

	private List<GameObject> _browserWindows = new List<GameObject>();

	public EmbeddedXsollaConnection(TransactionType transactionType)
		: base(transactionType)
	{
	}

	public override void Dispose()
	{
		if ((bool)_mainBrowser)
		{
			Object.Destroy(_mainBrowser);
		}
		if ((bool)_purchasePrompt)
		{
			Object.Destroy(_purchasePrompt.gameObject);
		}
		foreach (GameObject browserWindow in _browserWindows)
		{
			Object.Destroy(browserWindow);
		}
		_browserWindows.Clear();
	}

	public override void OpenBrowser(string xsollaToken, string redirect)
	{
		LoadBrowserParent(_purchasePrompt.CanvasGroup.GetComponentInChildren<VerticalLayoutGroup>().transform);
		_parentBrowserPrefab.transform.SetSiblingIndex(0);
		_mainBrowser.Init(this);
		_purchasePrompt.CanvasGroup.gameObject.SetActive(value: true);
		_purchasePrompt.CanvasGroup.DOFade(1f, 0.3f);
		_purchasePrompt.CanvasGroup.blocksRaycasts = true;
		string url = CreateURL(xsollaToken, redirect);
		_mainBrowser.Browser.LoadURL(url, force: false);
	}

	protected override void OnUnityCloseButtonClicked()
	{
		CloseBrowser();
		base.OnUnityCloseButtonClicked();
	}

	private void CloseBrowser()
	{
		if ((bool)_mainBrowser)
		{
			_mainBrowser.SetBackerActive(isActive: false);
		}
		_purchasePrompt.CanvasGroup.DOFade(0f, 0.5f).OnComplete(delegate
		{
			_purchasePrompt.CanvasGroup.blocksRaycasts = false;
			foreach (GameObject browserWindow in _browserWindows)
			{
				Object.Destroy(browserWindow);
			}
			_browserWindows.Clear();
			Object.Destroy(_mainBrowser.gameObject);
		});
	}

	public Browser CreateBrowser(Browser parent, string url)
	{
		GameObject browserObj = LoadBrowserWindow(_purchasePrompt.CanvasGroup.transform);
		Browser componentInChildren = browserObj.GetComponentInChildren<Browser>();
		Rect rect = componentInChildren.GetComponent<RectTransform>().rect;
		componentInChildren.Resize((int)rect.width, (int)rect.height);
		browserObj.GetComponentInChildren<Button>().onClick.AddListener(delegate
		{
			Object.Destroy(browserObj);
		});
		_browserWindows.Add(browserObj);
		return componentInChildren;
	}

	protected override XsollaPurchasePrompt LoadPurchasePromptPrefab()
	{
		AssetLookupSystem assetLookupSystem = Pantry.Get<AssetLookupManager>().AssetLookupSystem;
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.PurchaseFlow = PurchaseFlow.XsollaEmbedded;
		PurchasePromptPrefab payload = assetLookupSystem.TreeLoader.LoadTree<PurchasePromptPrefab>().GetPayload(assetLookupSystem.Blackboard);
		_purchasePrompt = AssetLoader.Instantiate<XsollaPurchasePrompt>(payload.PrefabPath);
		return _purchasePrompt;
	}

	private void LoadBrowserParent(Transform parent)
	{
		AssetLookupSystem assetLookupSystem = Pantry.Get<AssetLookupManager>().AssetLookupSystem;
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.LookupString = "Parent";
		ZFBrowserPrefab payload = assetLookupSystem.TreeLoader.LoadTree<ZFBrowserPrefab>().GetPayload(assetLookupSystem.Blackboard);
		_parentBrowserPrefab = AssetLoader.Instantiate(payload.PrefabPath, parent);
		_mainBrowser = _parentBrowserPrefab.GetComponent<XsollaMainBrowser>();
	}

	private GameObject LoadBrowserWindow(Transform parent)
	{
		AssetLookupSystem assetLookupSystem = Pantry.Get<AssetLookupManager>().AssetLookupSystem;
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.LookupString = "Window";
		return AssetLoader.Instantiate(assetLookupSystem.TreeLoader.LoadTree<ZFBrowserPrefab>().GetPayload(assetLookupSystem.Blackboard).PrefabPath, parent);
	}
}
