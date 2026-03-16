using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Code.AssetLookupTree.AssetLookup;
using Core.Meta.UI;
using MTGA.KeyboardManager;
using Pooling;
using UnityEngine;
using Wizards.Mtga;
using Wizards.Mtga.Platforms;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;

namespace Core.Shared.Code.ServiceFactories;

public class CardZoomDelegatorFactory
{
	public static ICardRolloverZoom Create()
	{
		CardRolloverZoomBase cardRolloverZoomBase = null;
		StaticCardRolloverZoom staticCardRolloverZoom = null;
		AssetLookupSystem assetLookupSystem = Pantry.Get<AssetLookupManager>().AssetLookupSystem;
		SceneUITransforms sceneUITransforms = Pantry.Get<SceneUITransforms>();
		CardViewBuilder cardViewBuilder = Pantry.Get<CardViewBuilder>();
		CardDatabase cardDatabase = Pantry.Get<CardDatabase>();
		IUnityObjectPool unityObjectPool = Pantry.Get<IUnityObjectPool>();
		IObjectPool genericObjectPool = Pantry.Get<IObjectPool>();
		KeyboardManager keyboardManager = Pantry.Get<KeyboardManager>();
		MatchManager matchManager = Pantry.Get<MatchManager>();
		string prefabPath = assetLookupSystem.GetPrefabPath<CardRolloverZoomPrefab, GameObject>();
		Transform parent = null;
		if (sceneUITransforms != null)
		{
			parent = sceneUITransforms.PopupsParent;
		}
		cardRolloverZoomBase = AssetLoader.Instantiate(prefabPath, parent).GetComponentInChildren<CardRolloverZoomBase>();
		cardRolloverZoomBase.Initialize(cardViewBuilder, cardDatabase, Languages.ActiveLocProvider, unityObjectPool, genericObjectPool, keyboardManager, matchManager?.Event?.PlayerEvent?.Format);
		ICardRolloverZoom cardRolloverZoom = null;
		if (PlatformUtils.IsHandheld())
		{
			return new CardRolloverZoomDelegator(null, (StaticCardRolloverZoom)cardRolloverZoomBase);
		}
		string prefabPath2 = assetLookupSystem.GetPrefabPath<StaticCardRolloverZoomPrefab, GameObject>();
		Transform parent2 = null;
		if (sceneUITransforms != null)
		{
			parent2 = sceneUITransforms.PopupsParent;
		}
		staticCardRolloverZoom = AssetLoader.Instantiate(prefabPath2, parent2).GetComponentInChildren<StaticCardRolloverZoom>();
		staticCardRolloverZoom.Initialize(cardViewBuilder, cardDatabase, Languages.ActiveLocProvider, unityObjectPool, genericObjectPool, keyboardManager, matchManager?.Event?.PlayerEvent?.Format);
		return new CardRolloverZoomDelegator((cardRolloverZoomBase != null) ? ((View_CardRolloverZoom)cardRolloverZoomBase) : null, staticCardRolloverZoom);
	}
}
