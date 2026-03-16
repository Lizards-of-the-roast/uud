using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card;
using Core.Shared.Code.Utilities;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.CardParts;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.Interactions;

public class CDCSpriteFillerSDF : CDCFillerBase
{
	public enum FieldType
	{
		None,
		Expansion,
		Guildmark
	}

	[SerializeField]
	private FieldType _fieldType;

	[SerializeField]
	private Transform _rendererRoot;

	private Renderer _renderer;

	private RawImage _rawImage;

	private Material _sdfInstance;

	private MaterialPropertyBlock _matBlock;

	public override int RawFieldType => (int)_fieldType;

	public override void Init(ICardDatabaseAdapter cardDatabase, AssetLookupSystem assetLookupSystem, CardMaterialBuilder cardMaterialBuilder, IUnityObjectPool unityObjectPool, CardColorCaches cardColorCaches)
	{
		base.Init(cardDatabase, assetLookupSystem, cardMaterialBuilder, unityObjectPool, cardColorCaches);
		if (!_hasBeenInit)
		{
			_hasBeenInit = true;
			_renderer = _rendererRoot.GetComponent<Renderer>();
			_rawImage = _rendererRoot.GetComponent<RawImage>();
			_matBlock = new MaterialPropertyBlock();
			_sdfInstance = _renderer.sharedMaterial;
		}
	}

	public override void UpdateField(ICardDataAdapter model, CardHolderType cardHolderType, HashSet<CDCFillerBase> otherFillers, CDCViewMetadata viewMetadata, MtgGameState gameState, WorkflowBase currentInteraction)
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.SetCardDataExtensive(model);
		_assetLookupSystem.Blackboard.SetCdcViewMetadata(viewMetadata);
		_assetLookupSystem.Blackboard.CardHolderType = cardHolderType;
		_assetLookupSystem.Blackboard.SpriteSdfFillerType = _fieldType;
		_assetLookupSystem.Blackboard.GameState = gameState;
		_matBlock.Clear();
		switch (_fieldType)
		{
		case FieldType.Expansion:
		{
			AltAssetReference<Texture2D> altAssetRef2 = null;
			AltAssetReference<Texture2D> altAssetRef3 = null;
			AltAssetReference<Texture2D> altAssetRef4 = null;
			Color black = Color.black;
			SetTexture("_MainTex", altAssetRef2);
			SetTexture("_LineTex", altAssetRef3);
			SetTexture("_MainTexture", altAssetRef4);
			_matBlock.SetColor(ShaderPropertyIds.LineColorPropId, black);
			break;
		}
		case FieldType.Guildmark:
		{
			AltAssetReference<Texture2D> altAssetRef = null;
			KeyValuePair<Color, Color> keyValuePair = default(KeyValuePair<Color, Color>);
			if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<Guildmark> loadedTree))
			{
				Guildmark payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
				if (payload != null)
				{
					altAssetRef = payload.SymbolRef;
					keyValuePair = _cardColorCaches.GetCardColorTable(payload.ColorTableRef)?.GetColors(model.GetFrameColors) ?? default(KeyValuePair<Color, Color>);
				}
			}
			SetTexture("_MainTex", altAssetRef);
			_matBlock.SetColor(ShaderPropertyIds.MainColorPropId, keyValuePair.Key);
			_matBlock.SetColor(ShaderPropertyIds.MainColor2PropId, keyValuePair.Value);
			break;
		}
		}
		if (viewMetadata.IsDimmed)
		{
			_sdfInstance.EnableKeyword("_USEDIMMED_ON");
		}
		else
		{
			_sdfInstance.DisableKeyword("_USEDIMMED_ON");
		}
		SetMaterial();
		_assetLookupSystem.Blackboard.Clear();
	}

	private void SetTexture(string name, AltAssetReference<Texture2D> altAssetRef)
	{
		Texture2D texture2D = (string.IsNullOrWhiteSpace(altAssetRef?.RelativePath) ? null : AssetLoader.AcquireAndTrackAsset(_assetTracker, name, altAssetRef));
		if (texture2D != null)
		{
			_matBlock.SetTexture(name, texture2D);
		}
	}

	public override void SetDestroyed(bool isDestroyed)
	{
		if (_renderer != null)
		{
			_renderer.enabled = !isDestroyed;
		}
		if (_rawImage != null)
		{
			_rawImage.enabled = !isDestroyed;
		}
	}

	private void SetMaterial()
	{
		if (_renderer != null)
		{
			_renderer.SetPropertyBlock(_matBlock);
			_renderer.material = _sdfInstance;
		}
		if (_rawImage != null)
		{
			_rawImage.material = _sdfInstance;
			_rawImage.texture = _sdfInstance.mainTexture;
			_rawImage.SetMaterialDirty();
		}
	}

	public override void Cleanup()
	{
		if (_renderer != null)
		{
			_renderer.SetPropertyBlock(null);
			_renderer.material = null;
		}
		if (_rawImage != null)
		{
			_rawImage.material = null;
			_rawImage.texture = null;
		}
		_matBlock.Clear();
		base.Cleanup();
	}
}
