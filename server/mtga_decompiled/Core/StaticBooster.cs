using AssetLookupTree;
using AssetLookupTree.Payloads.Booster;
using UnityEngine;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Wrapper;

public class StaticBooster : MonoBehaviour
{
	[SerializeField]
	private CollationMapping _setCode;

	[SerializeField]
	private MeshRenderer _renderer;

	[SerializeField]
	private bool _setBackground = true;

	[SerializeField]
	private bool _setLogo = true;

	private RendererReferenceLoader _materialReferenceLoader;

	private void OnEnable()
	{
		if (_renderer != null && _setCode != CollationMapping.None)
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
		if (_materialReferenceLoader == null)
		{
			_materialReferenceLoader = new RendererReferenceLoader(_renderer);
		}
		AssetLookupSystem assetLookupSystem = WrapperController.Instance.AssetLookupSystem;
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.BoosterCollationMapping = _setCode;
		if (_setBackground && assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<Background> loadedTree))
		{
			Background payload = loadedTree.GetPayload(assetLookupSystem.Blackboard);
			if (payload != null)
			{
				_materialReferenceLoader.SetAndApplyPropertyBlockTexture(0, "_MainTex", payload.TextureRef.RelativePath);
			}
		}
		if (_setLogo && assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<Logo> loadedTree2))
		{
			Logo payload2 = loadedTree2.GetPayload(assetLookupSystem.Blackboard);
			if (payload2 != null)
			{
				_materialReferenceLoader.SetAndApplyPropertyBlockTexture(0, "_Decal1", payload2.TextureRef.RelativePath);
			}
		}
	}

	public void OnDestroy()
	{
		_materialReferenceLoader?.Cleanup();
	}
}
