using AssetLookupTree;
using AssetLookupTree.Payloads.Wrapper;
using Core.Code.AssetLookupTree.AssetLookup;
using SharedClientCore.SharedClientCore.Code.Providers;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga;

public class PerSetBackgroundPayloadLoader : MonoBehaviour
{
	[SerializeField]
	private Image _background;

	[SerializeField]
	private NavContentType _sceneNavType;

	private void Awake()
	{
		AssetLookupSystem assetLookupSystem = Pantry.Get<AssetLookupManager>().AssetLookupSystem;
		ISetMetadataProvider setMetadataProvider = Pantry.Get<ISetMetadataProvider>();
		if (!TrySetBackground(assetLookupSystem, setMetadataProvider))
		{
			SimpleLog.LogErrorFormat("Failed to load {0} background for latest published set {1}", _sceneNavType, setMetadataProvider.LastPublishedMajorSet);
		}
	}

	private bool TrySetBackground(AssetLookupSystem assetLookupSystem, ISetMetadataProvider setMetadataProvider)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.NavContentType = _sceneNavType;
		assetLookupSystem.Blackboard.SetCode = setMetadataProvider.LastPublishedMajorSet;
		PerSetSceneBackgroundPayload payload = assetLookupSystem.TreeLoader.LoadTree<PerSetSceneBackgroundPayload>().GetPayload(assetLookupSystem.Blackboard);
		if (payload != null)
		{
			AssetLoader.AssetTracker<Sprite> assetTracker = new AssetLoader.AssetTracker<Sprite>("PerSetSceneBackground");
			bool result = AssetLoaderUtils.TrySetSprite(_background, assetTracker, payload.Reference?.RelativePath);
			assetTracker.Cleanup();
			return result;
		}
		return false;
	}
}
