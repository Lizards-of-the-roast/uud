using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Code.AssetLookupTree.AssetLookup;
using DG.Tweening;
using UnityEngine;

namespace Wizards.Mtga.Store;

public class ExternalXsollaConnection : XsollaConnection
{
	private XsollaPurchasePrompt _purchasePrompt;

	public ExternalXsollaConnection(TransactionType transactionType)
		: base(transactionType)
	{
	}

	protected override XsollaPurchasePrompt LoadPurchasePromptPrefab()
	{
		AssetLookupSystem assetLookupSystem = Pantry.Get<AssetLookupManager>().AssetLookupSystem;
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.PurchaseFlow = PurchaseFlow.XsollaExternal;
		PurchasePromptPrefab payload = assetLookupSystem.TreeLoader.LoadTree<PurchasePromptPrefab>().GetPayload(assetLookupSystem.Blackboard);
		_purchasePrompt = AssetLoader.Instantiate<XsollaPurchasePrompt>(payload.PrefabPath);
		return _purchasePrompt;
	}

	public override void Dispose()
	{
		if ((bool)_purchasePrompt)
		{
			Object.Destroy(_purchasePrompt.gameObject);
		}
	}

	protected override void OnUnityCloseButtonClicked()
	{
		ClosePopupPrompt();
		base.OnUnityCloseButtonClicked();
	}

	public override void OpenBrowser(string xsollaToken, string redirect)
	{
		OpenPopupPrompt();
		Application.OpenURL(CreateURL(xsollaToken, redirect));
	}

	private void OpenPopupPrompt()
	{
		_purchasePrompt.CanvasGroup.gameObject.SetActive(value: true);
		_purchasePrompt.CanvasGroup.DOFade(1f, 0.3f);
		_purchasePrompt.CanvasGroup.blocksRaycasts = true;
	}

	private void ClosePopupPrompt()
	{
		_purchasePrompt.CanvasGroup.DOFade(0f, 0.5f).OnComplete(delegate
		{
			_purchasePrompt.CanvasGroup.blocksRaycasts = false;
		});
	}
}
