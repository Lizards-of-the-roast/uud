using AssetLookupTree;
using AssetLookupTree.Payloads.Booster;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Wrapper;

public class StaticLogo : MonoBehaviour
{
	[SerializeField]
	private CollationMapping _setCode;

	[FormerlySerializedAs("logo")]
	[FormerlySerializedAs("_renderer")]
	[SerializeField]
	private RawImage _logo;

	private readonly AssetLoader.AssetTracker<Texture> _textureTracker = new AssetLoader.AssetTracker<Texture>("StaticLogoTextureTracker");

	private void OnEnable()
	{
		if (_logo != null && _setCode != CollationMapping.None)
		{
			LocalizeLogos();
			Languages.LanguageChangedSignal.Listeners += LocalizeLogos;
		}
	}

	private void OnDisable()
	{
		Languages.LanguageChangedSignal.Listeners -= LocalizeLogos;
	}

	private void LocalizeLogos()
	{
		AssetLookupSystem assetLookupSystem = WrapperController.Instance.AssetLookupSystem;
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.BoosterCollationMapping = _setCode;
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<Logo> loadedTree))
		{
			Logo payload = loadedTree.GetPayload(assetLookupSystem.Blackboard);
			if (payload != null)
			{
				_logo.texture = _textureTracker.Acquire(payload.TextureRef.RelativePath);
			}
		}
	}

	private void OnDestroy()
	{
		_logo.texture = null;
		_textureTracker.Cleanup();
	}
}
