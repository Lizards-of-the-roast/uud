using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.CardParts;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtgo.Gre.External.Messaging;

public class CDCSpriteFiller : CDCFillerBase
{
	public enum FieldType
	{
		None,
		Face,
		WildcardInMeta,
		BasicLandIcon,
		Expansion,
		ColorOverride
	}

	[SerializeField]
	private FieldType _fieldType;

	private UnityEngine.Color _defaultColor = UnityEngine.Color.white;

	private Image _image;

	private RawImage _rawImage;

	private SpriteRenderer _spriteRenderer;

	private readonly AssetLoader.AssetTracker<Texture> _rawImageTextureTracker = new AssetLoader.AssetTracker<Texture>("CDCSpriteFillerRawImageTextureTracker");

	private AssetLoader.AssetTracker<Sprite> _spriteTracker = new AssetLoader.AssetTracker<Sprite>("CDCSpriteFillerSprite");

	private AssetLoader.AssetTracker<Texture2D> _textureTracker = new AssetLoader.AssetTracker<Texture2D>("CDCSpriteFillerTexture");

	private AssetLoader.AssetTracker<Sprite> _rendererSpriteTracker = new AssetLoader.AssetTracker<Sprite>("CDCSpriteFillerRendererSprite");

	private AssetLoader.AssetTracker<Texture2D> _rendererTextureTracker = new AssetLoader.AssetTracker<Texture2D>("CDCSpriteFillerRendererTexture");

	public override int RawFieldType => (int)_fieldType;

	public override void Init(ICardDatabaseAdapter cardDatabase, AssetLookupSystem assetLookupSystem, CardMaterialBuilder cardMaterialBuilder, IUnityObjectPool unityObjectPool, CardColorCaches cardColorCaches)
	{
		base.Init(cardDatabase, assetLookupSystem, cardMaterialBuilder, unityObjectPool, cardColorCaches);
		if (!_hasBeenInit)
		{
			_cardColorCaches = cardColorCaches;
			_image = GetComponent<Image>();
			if (_image != null)
			{
				_defaultColor = _image.color;
			}
			_rawImage = GetComponent<RawImage>();
			if (_rawImage != null)
			{
				_defaultColor = _rawImage.color;
			}
			_spriteRenderer = GetComponent<SpriteRenderer>();
			if (_spriteRenderer != null)
			{
				_defaultColor = _spriteRenderer.color;
			}
			_hasBeenInit = true;
		}
	}

	public override void UpdateField(ICardDataAdapter model, CardHolderType cardHolderType, HashSet<CDCFillerBase> otherFillers, CDCViewMetadata viewMetadata, MtgGameState gameState, WorkflowBase currentInteraction)
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.SetCardDataExtensive(model);
		_assetLookupSystem.Blackboard.SetCdcViewMetadata(viewMetadata);
		_assetLookupSystem.Blackboard.CardHolderType = cardHolderType;
		_assetLookupSystem.Blackboard.SpriteFillerType = _fieldType;
		_assetLookupSystem.Blackboard.GameState = gameState;
		_assetLookupSystem.Blackboard.TextureName = GetTextureName();
		string spritePath = null;
		string texturePath = null;
		UnityEngine.Color spriteColor = _defaultColor;
		switch (_fieldType)
		{
		case FieldType.Face:
		{
			if (!_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<FaceSymbol> loadedTree2))
			{
				break;
			}
			FaceSymbol faceSymbol = loadedTree2?.GetPayload(_assetLookupSystem.Blackboard);
			if (faceSymbol != null)
			{
				switch (model.LinkedFaceType)
				{
				case LinkedFace.DfcFront:
				case LinkedFace.MdfcFront:
					spritePath = faceSymbol.BackSymbolRef.RelativePath;
					break;
				case LinkedFace.DfcBack:
				case LinkedFace.MdfcBack:
					spritePath = faceSymbol.FrontSymbolRef.RelativePath;
					break;
				default:
					spritePath = faceSymbol.FrontSymbolRef.RelativePath;
					break;
				}
			}
			break;
		}
		case FieldType.WildcardInMeta:
			texturePath = model.ImageAssetPath;
			break;
		case FieldType.BasicLandIcon:
		{
			if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<LandSymbol> loadedTree3))
			{
				LandSymbol landSymbol = loadedTree3?.GetPayload(_assetLookupSystem.Blackboard);
				if (landSymbol != null)
				{
					spritePath = landSymbol.SpriteRef.RelativePath;
					spriteColor = landSymbol.SpriteColor;
				}
			}
			break;
		}
		case FieldType.Expansion:
		{
			if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<ExpansionSymbol> loadedTree4))
			{
				ExpansionSymbol expansionSymbol = loadedTree4?.GetPayload(_assetLookupSystem.Blackboard);
				if (expansionSymbol != null)
				{
					spritePath = expansionSymbol.GetIconRef(model.Rarity).RelativePath;
				}
			}
			break;
		}
		case FieldType.ColorOverride:
		{
			if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<ColorOverride> loadedTree))
			{
				ColorOverride colorOverride = loadedTree?.GetPayload(_assetLookupSystem.Blackboard);
				if (colorOverride != null)
				{
					spriteColor = _cardColorCaches.GetCardColorTable(colorOverride.ColorTableRef).GetColor(model.Colors);
				}
			}
			spritePath = null;
			texturePath = null;
			break;
		}
		}
		if (_fieldType != FieldType.ColorOverride)
		{
			AssetLoaderUtils.CleanupImage(_image, _spriteTracker, _textureTracker);
			AssetLoaderUtils.CleanupRawImage(_rawImage, _rawImageTextureTracker);
			AssetLoaderUtils.CleanupSpriteRenderer(_spriteRenderer, _rendererSpriteTracker, _rendererTextureTracker);
			if (!TrySetTextures(texturePath))
			{
				TrySetSprites(spritePath);
			}
		}
		SetSpriteColor(spriteColor);
		_assetLookupSystem.Blackboard.Clear();
	}

	public override void SetDestroyed(bool isDestroyed)
	{
		if (_image != null)
		{
			_image.enabled = !isDestroyed;
		}
		if (_rawImage != null)
		{
			_rawImage.enabled = !isDestroyed;
		}
		if (_spriteRenderer != null)
		{
			_spriteRenderer.enabled = !isDestroyed;
		}
	}

	private bool TrySetSprites(string spritePath)
	{
		if (!AssetLoaderUtils.TrySetSprite(_image, _spriteTracker, spritePath))
		{
			return AssetLoaderUtils.TrySetRendererSprite(_spriteRenderer, _rendererSpriteTracker, spritePath);
		}
		return true;
	}

	private bool TrySetTextures(string texturePath)
	{
		if (!AssetLoaderUtils.TrySetTextureFromSprite(_image, _textureTracker, texturePath) && !AssetLoaderUtils.TrySetRawImageTexture(_rawImage, _rawImageTextureTracker, texturePath))
		{
			return AssetLoaderUtils.TrySetRendererTexture(_spriteRenderer, _rendererTextureTracker, texturePath);
		}
		return true;
	}

	private void SetSpriteColor(UnityEngine.Color colorOverride)
	{
		if ((bool)_image)
		{
			_image.color = colorOverride;
		}
		if ((bool)_rawImage)
		{
			_rawImage.color = colorOverride;
		}
		if ((bool)_spriteRenderer)
		{
			_spriteRenderer.color = colorOverride;
		}
	}

	private string GetTextureName()
	{
		if (_image != null && _image.sprite != null && _image.sprite.texture != null)
		{
			return _image.sprite.texture.name;
		}
		if (_rawImage != null && _rawImage.texture != null)
		{
			return _rawImage.texture.name;
		}
		if (_spriteRenderer != null && _spriteRenderer.sprite != null && _spriteRenderer.sprite.texture != null)
		{
			return _spriteRenderer.sprite.texture.name;
		}
		return base.gameObject.name;
	}

	public override void Cleanup()
	{
		if (_fieldType != FieldType.ColorOverride)
		{
			AssetLoaderUtils.CleanupImage(_image, _spriteTracker, _textureTracker);
			AssetLoaderUtils.CleanupRawImage(_rawImage, _rawImageTextureTracker);
			AssetLoaderUtils.CleanupSpriteRenderer(_spriteRenderer, _rendererSpriteTracker, _rendererTextureTracker);
		}
		base.Cleanup();
	}
}
