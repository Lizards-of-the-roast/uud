using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Code.AssetLookupTree.AssetLookup;
using Core.Meta.UI;
using MTGA.KeyboardManager;
using Pooling;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;

namespace Core.Shared.Code.ServiceFactories;

public class CardZoomFactory
{
	public static CardRolloverZoomBase Create()
	{
		AssetLookupSystem assetLookupSystem = Pantry.Get<AssetLookupManager>().AssetLookupSystem;
		SceneUITransforms sceneUITransforms = Pantry.Get<SceneUITransforms>();
		string prefabPath = assetLookupSystem.GetPrefabPath<CardRolloverZoomPrefab, GameObject>();
		Transform parent = null;
		if (sceneUITransforms != null)
		{
			parent = sceneUITransforms.PopupsParent;
		}
		CardRolloverZoomBase componentInChildren = AssetLoader.Instantiate(prefabPath, parent).GetComponentInChildren<CardRolloverZoomBase>();
		componentInChildren.Initialize(Pantry.Get<CardViewBuilder>(), Pantry.Get<CardDatabase>(), unityObjectPool: Pantry.Get<IUnityObjectPool>(), genericObjectPool: Pantry.Get<IObjectPool>(), keyboardManager: Pantry.Get<KeyboardManager>(), currentEventFormat: Pantry.Get<MatchManager>()?.Event?.PlayerEvent?.Format, locManager: Languages.ActiveLocProvider);
		return componentInChildren;
	}
}
