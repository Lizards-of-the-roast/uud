using AssetLookupTree.Payloads;
using Core.Code.AssetLookupTree.AssetLookup;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Wrapper;

[RequireComponent(typeof(RawImage))]
public class NamedTextureLocalize : MonoBehaviour
{
	[SerializeField]
	private string _textureName;

	private RawImage _rawImage;

	private void Awake()
	{
		_rawImage = GetComponent<RawImage>();
	}

	private void Start()
	{
		SetLocalizedSprite();
		Languages.LanguageChangedSignal.Listeners += SetLocalizedSprite;
	}

	private void OnDestroy()
	{
		Languages.LanguageChangedSignal.Listeners -= SetLocalizedSprite;
	}

	private void SetLocalizedSprite()
	{
		if (_rawImage == null)
		{
			Debug.LogError("NamedTextureLocalize: no texture target found");
			return;
		}
		AssetLookupManager assetLookupManager = Pantry.Get<AssetLookupManager>();
		assetLookupManager.AssetLookupSystem.Blackboard.Clear();
		assetLookupManager.AssetLookupSystem.Blackboard.TextureName = _textureName;
		NamedTexturePayload payload = assetLookupManager.AssetLookupSystem.TreeLoader.LoadTree<NamedTexturePayload>().GetPayload(assetLookupManager.AssetLookupSystem.Blackboard);
		if (payload == null)
		{
			Debug.LogError("NamedTextureLocalize: failed to find payload for texture name '" + _textureName + "'");
			return;
		}
		Texture objectData = AssetLoader.GetObjectData<Texture>(payload.Reference.RelativePath);
		if (objectData == null)
		{
			Debug.LogError("NamedTextureLocalize: failed to find payload texture at path '" + payload.Reference.RelativePath + "'");
		}
		else
		{
			_rawImage.texture = objectData;
		}
	}
}
