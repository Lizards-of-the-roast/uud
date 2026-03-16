using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Pooling;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.Interactions;

public class ChooseXInterfaceBuilder : IChooseXInterfaceBuilder
{
	private readonly IUnityObjectPool _unityPool;

	private readonly ICanvasRootProvider _canvasRootProvider;

	private readonly ChooseXInterfacePrefab _payload;

	private readonly AssetTracker _interfaceTracker = new AssetTracker();

	public ChooseXInterfaceBuilder(IUnityObjectPool unityPool, AssetLookupSystem assetLookupSystem, ICanvasRootProvider canvasRootProvider)
	{
		_unityPool = unityPool;
		_canvasRootProvider = canvasRootProvider;
		AssetLookupTree<ChooseXInterfacePrefab> assetLookupTree = assetLookupSystem.TreeLoader.LoadTree<ChooseXInterfacePrefab>();
		_payload = assetLookupTree.GetPayload(assetLookupSystem.Blackboard);
	}

	public IChooseXInterface CreateInterface(string assetTrackingKey)
	{
		Transform canvasRoot = _canvasRootProvider.GetCanvasRoot(CanvasLayer.ScreenSpace_Default);
		View_ChooseXInterface view_ChooseXInterface = _interfaceTracker.AcquireAndTrack(assetTrackingKey, _payload.Prefab);
		RectTransform component = view_ChooseXInterface.GetComponent<RectTransform>();
		View_ChooseXInterface component2 = _unityPool.PopObject(view_ChooseXInterface.gameObject, canvasRoot).GetComponent<View_ChooseXInterface>();
		RectTransform component3 = component2.GetComponent<RectTransform>();
		component3.anchoredPosition = component.anchoredPosition;
		component3.localRotation = component.localRotation;
		component3.localScale = component.localScale;
		return component2;
	}

	public void DestroyInterface(IChooseXInterface chooseXInterface, string assetTrackingKey)
	{
		chooseXInterface.Close();
		_interfaceTracker.RemoveAssetReference(assetTrackingKey);
		if (chooseXInterface is View_ChooseXInterface view_ChooseXInterface)
		{
			_unityPool.PushObject(view_ChooseXInterface.gameObject);
		}
	}
}
