using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card;
using Core.Shared.Code.Utilities;
using GreClient.CardData;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Cards;
using Wotc.Mtga.Cards.ArtCrops;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

public class CardMaterialBuilder
{
	private class PoolMaterial
	{
		public readonly AltReferencedMaterial ReferencedMaterial;

		public readonly long MaterialHash;

		public int RefCount { get; private set; }

		public PoolMaterial(AltReferencedMaterial materialTextureReferenceLoader, long materialHash)
		{
			ReferencedMaterial = materialTextureReferenceLoader;
			RefCount = 0;
			MaterialHash = materialHash;
		}

		public void DecrementReference()
		{
			RefCount--;
		}

		public void AddReference()
		{
			RefCount++;
		}
	}

	private class PoolPropertyBlockReference
	{
		public AltReferencedMaterialAndBlock ReferencedMaterialAndBlock;

		public PoolMaterial PoolMaterial;

		public float? DecayTimer;

		public int RefCount { get; private set; }

		public PoolPropertyBlockReference(AltReferencedMaterialAndBlock materialAndBlockReference, PoolMaterial poolMaterial)
		{
			DecayTimer = null;
			PoolMaterial = poolMaterial;
			PoolMaterial.AddReference();
			ReferencedMaterialAndBlock = materialAndBlockReference;
			RefCount = 1;
		}

		public void DecrementReference()
		{
			RefCount--;
			if (RefCount == 0)
			{
				DecayTimer = 90f;
			}
		}

		public void AddReference()
		{
			DecayTimer = null;
			RefCount++;
		}
	}

	private const float DECAY_MAXIMUM = 90f;

	private const string MAIN_TEX = "_MainTex";

	private readonly Dictionary<long, PoolMaterial> _cachedSharedMaterials = new Dictionary<long, PoolMaterial>(30);

	private readonly Dictionary<long, PoolPropertyBlockReference> _cachedMatBlockReferences = new Dictionary<long, PoolPropertyBlockReference>(30);

	private readonly List<long> _cachesPendingDelete = new List<long>(10);

	private readonly HashSet<TextureOverride> _tmpTextureOverrides = new HashSet<TextureOverride>();

	private readonly List<TextureOverride.TextureOverrideEntry> _tmpTextureOverrideEntries = new List<TextureOverride.TextureOverrideEntry>(5);

	private readonly List<ArtIdOverride.ArtReplacement> _tmpArtReplacements = new List<ArtIdOverride.ArtReplacement>(5);

	private readonly IArtCropProvider _cropDatabase;

	private readonly CardArtTextureLoader _artTextureLoader;

	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly CardColorCaches _cardColorCaches;

	public IArtCropProvider CropDatabase => _cropDatabase;

	public CardArtTextureLoader TextureLoader => _artTextureLoader;

	public CardMaterialBuilder(AssetLookupSystem assetLookupSystem, CardArtTextureLoader artTextureLoader, IArtCropProvider cardArtCropDatabase, CardColorCaches cardColorCaches)
	{
		_assetLookupSystem = assetLookupSystem;
		_artTextureLoader = artTextureLoader;
		_cropDatabase = cardArtCropDatabase;
		_cardColorCaches = cardColorCaches;
	}

	public void ClearCacheImmediate()
	{
		foreach (KeyValuePair<long, PoolPropertyBlockReference> cachedMatBlockReference in _cachedMatBlockReferences)
		{
			cachedMatBlockReference.Value.ReferencedMaterialAndBlock.Cleanup();
		}
		_cachedMatBlockReferences.Clear();
		foreach (KeyValuePair<long, PoolMaterial> cachedSharedMaterial in _cachedSharedMaterials)
		{
			cachedSharedMaterial.Value.ReferencedMaterial.Cleanup();
		}
		_cachedSharedMaterials.Clear();
	}

	public void DecayUnreferencedMaterialBlocks()
	{
		UpdateDecayTimers(91f);
	}

	public void UpdateDecayTimers(float timeStep)
	{
		foreach (KeyValuePair<long, PoolPropertyBlockReference> cachedMatBlockReference in _cachedMatBlockReferences)
		{
			if (cachedMatBlockReference.Value.DecayTimer.HasValue)
			{
				cachedMatBlockReference.Value.DecayTimer -= timeStep;
				if (cachedMatBlockReference.Value.DecayTimer < 0f)
				{
					_ = cachedMatBlockReference.Value.RefCount;
					_ = 0;
					_cachesPendingDelete.Add(cachedMatBlockReference.Key);
				}
			}
		}
		if (_cachesPendingDelete.Count <= 0)
		{
			return;
		}
		foreach (long item in _cachesPendingDelete)
		{
			if (_cachedMatBlockReferences.TryGetValue(item, out var value))
			{
				PoolMaterial poolMaterial = value.PoolMaterial;
				poolMaterial.DecrementReference();
				if (poolMaterial.RefCount <= 0)
				{
					poolMaterial.ReferencedMaterial.Cleanup();
					_cachedSharedMaterials.Remove(poolMaterial.MaterialHash);
				}
				value.ReferencedMaterialAndBlock.Cleanup();
				_cachedMatBlockReferences.Remove(item);
			}
		}
		_cachesPendingDelete.Clear();
	}

	public void DecrementReferenceCount(int blockHashCode)
	{
		if (blockHashCode != 0 && _cachedMatBlockReferences.ContainsKey(blockHashCode))
		{
			_cachedMatBlockReferences[blockHashCode].DecrementReference();
		}
	}

	public AltReferencedMaterialAndBlock GetMaterialReplacement(ICardDatabaseAdapter cardDatabase, AltReferencedMaterialAndBlock original, string originalTextureName, MaterialOverrideType materialOverrideType, ICardDataAdapter model, CardHolderType cardHolderType, Func<MtgGameState> getCurrentGameState, bool dimmed = false, bool dissolve = false, bool invertColors = false, string primaryArtSuffix = null, string secondaryArtSuffix = null, string artCropFormatName = null, Vector2 artOffset = default(Vector2), bool mousedOver = false)
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.Material = original.SharedMaterial;
		_assetLookupSystem.Blackboard.MaterialName = original.SharedMaterial.name;
		_assetLookupSystem.Blackboard.Texture = (original.SharedMaterial.HasProperty("_MainTex") ? ((Texture2D)original.SharedMaterial.mainTexture) : null);
		_assetLookupSystem.Blackboard.TextureName = originalTextureName;
		_assetLookupSystem.Blackboard.SetCardDataExtensive(model);
		_assetLookupSystem.Blackboard.CardHolderType = cardHolderType;
		_assetLookupSystem.Blackboard.Player = getCurrentGameState?.Invoke()?.LocalPlayer;
		_assetLookupSystem.Blackboard.MouseOverType = (mousedOver ? MouseOverType.MouseOver : MouseOverType.None);
		_assetLookupSystem.Blackboard.CardIsHovered = mousedOver;
		if (model.ObjectType == GameObjectType.Ability)
		{
			_assetLookupSystem.Blackboard.Ability = cardDatabase.AbilityDataProvider.GetAbilityPrintingById(model.GrpId);
		}
		_tmpTextureOverrides.Clear();
		_tmpTextureOverrideEntries.Clear();
		_tmpArtReplacements.Clear();
		string overridePath = GetOverridePath(_assetLookupSystem, materialOverrideType);
		if (!string.IsNullOrWhiteSpace(overridePath))
		{
			_assetLookupSystem.Blackboard.MaterialName = GetFileName(overridePath);
		}
		_assetLookupSystem.TreeLoader.LoadTree<TextureOverride>(returnNewTree: false)?.GetPayloadLayered(_assetLookupSystem.Blackboard, _tmpTextureOverrides);
		if (_tmpTextureOverrides.Count > 0)
		{
			foreach (TextureOverride tmpTextureOverride in _tmpTextureOverrides)
			{
				foreach (TextureOverride.TextureOverrideEntry textureOverrideEntry in tmpTextureOverride.TextureOverrideEntries)
				{
					if (textureOverrideEntry.Property.Equals("_MainTex"))
					{
						_assetLookupSystem.Blackboard.TextureName = GetFileName(textureOverrideEntry.TextureRef.RelativePath);
						break;
					}
				}
			}
		}
		ColorOverride colorOverride = _assetLookupSystem.TreeLoader.LoadTree<ColorOverride>(returnNewTree: false)?.GetPayload(_assetLookupSystem.Blackboard);
		IReadOnlyList<CardColor> readOnlyList = ((colorOverride != null && colorOverride.UsePrintedFrameColors) ? model.Printing.FrameColors : model.GetFrameColors);
		HashCode hashCode = default(HashCode);
		if (dimmed)
		{
			hashCode.Add("DIM");
		}
		if (dissolve)
		{
			hashCode.Add("DIS");
		}
		hashCode.Add(original.SharedMaterial.GetInstanceID());
		if (!string.IsNullOrWhiteSpace(overridePath))
		{
			hashCode.Add(overridePath.GetHashCode());
		}
		foreach (TextureOverride tmpTextureOverride2 in _tmpTextureOverrides)
		{
			foreach (TextureOverride.TextureOverrideEntry textureOverrideEntry2 in tmpTextureOverride2.TextureOverrideEntries)
			{
				if (!string.IsNullOrEmpty(textureOverrideEntry2.Keyword))
				{
					hashCode.Add(textureOverrideEntry2.Keyword);
				}
			}
		}
		HashCode hashCode2 = default(HashCode);
		_ = model.SkinCode;
		bool flag = original.SharedMaterial.name.Contains("ArtInFrame");
		string text = null;
		int num = 0;
		if (!flag)
		{
			num = hashCode.ToHashCode();
			hashCode2.Add(num);
		}
		else
		{
			text = model.ImageAssetPath;
			ArtIdOverride artIdOverride = _assetLookupSystem.TreeLoader.LoadTree<ArtIdOverride>(returnNewTree: false)?.GetPayload(_assetLookupSystem.Blackboard);
			if (artIdOverride == null && (text.Equals("Assets/Core/CardArt/000000/000000_AIF") || model.ObjectType == GameObjectType.Ability) && model.Parent != null && model.Parent.FaceDownState.IsFaceDown)
			{
				CardData cardDataExtensive = new CardData(model.Parent, model.Printing);
				_assetLookupSystem.Blackboard.SetCardDataExtensive(cardDataExtensive);
				artIdOverride = _assetLookupSystem.TreeLoader.LoadTree<ArtIdOverride>(returnNewTree: false)?.GetPayload(_assetLookupSystem.Blackboard);
			}
			if (artIdOverride != null)
			{
				CardPrintingData cardPrintingData = model.Printing;
				if (model.ObjectType == GameObjectType.Ability && model.Parent != null)
				{
					CardPrintingData cardPrintingById = cardDatabase.CardDataProvider.GetCardPrintingById(model.Parent.GrpId);
					if (cardPrintingById != null)
					{
						cardPrintingData = cardPrintingById;
						_ = model.Parent.SkinCode;
					}
				}
				if (cardPrintingData != null)
				{
					_tmpArtReplacements.AddRange(artIdOverride.GetArtReplacements(cardPrintingData));
					text = ((_tmpArtReplacements.Count != 0) ? _tmpArtReplacements[0].ArtPath : CardArtUtil.GetArtPath(model, artIdOverride.ArtPath));
				}
			}
			foreach (ArtIdOverride.ArtReplacement tmpArtReplacement in _tmpArtReplacements)
			{
				if (!string.IsNullOrEmpty(tmpArtReplacement.Keyword))
				{
					hashCode.Add(tmpArtReplacement.Keyword);
				}
			}
			num = hashCode.ToHashCode();
			hashCode2.Add(num);
			if (text == null)
			{
				text = string.Empty;
			}
			List<ArtIdOverride.ArtReplacement> tmpArtReplacements = _tmpArtReplacements;
			if (tmpArtReplacements != null && tmpArtReplacements.Count > 0)
			{
				text = null;
				foreach (ArtIdOverride.ArtReplacement tmpArtReplacement2 in _tmpArtReplacements)
				{
					if (text == null)
					{
						text = tmpArtReplacement2.ArtPath;
					}
					hashCode2.Add(tmpArtReplacement2.ArtPath);
				}
			}
			else
			{
				hashCode2.Add(text);
			}
			if (!string.IsNullOrWhiteSpace(primaryArtSuffix))
			{
				hashCode2.Add(primaryArtSuffix);
			}
			if (!string.IsNullOrWhiteSpace(secondaryArtSuffix))
			{
				hashCode2.Add(secondaryArtSuffix);
			}
			if (!string.IsNullOrWhiteSpace(artCropFormatName))
			{
				hashCode2.Add(artCropFormatName);
			}
			if (artOffset != default(Vector2))
			{
				hashCode2.Add(artOffset);
			}
		}
		foreach (TextureOverride tmpTextureOverride3 in _tmpTextureOverrides)
		{
			foreach (TextureOverride.TextureOverrideEntry textureOverrideEntry3 in tmpTextureOverride3.TextureOverrideEntries)
			{
				hashCode2.Add(textureOverrideEntry3.TextureRef.RelativePath.GetHashCode());
				_tmpTextureOverrideEntries.Add(textureOverrideEntry3);
			}
		}
		if (colorOverride != null)
		{
			hashCode2.Add(colorOverride.ColorTableRef.RelativePath.GetHashCode());
			hashCode2.Add(invertColors.GetHashCode());
			foreach (CardColor item in readOnlyList)
			{
				hashCode2.Add(item.GetHashCode());
			}
		}
		_assetLookupSystem.Blackboard.Clear();
		bool flag2 = !string.IsNullOrEmpty(text) || (_tmpArtReplacements != null && _tmpArtReplacements.Count > 0);
		bool flag3 = flag && flag2 && !_tmpTextureOverrideEntries.Exists((TextureOverride.TextureOverrideEntry x) => x.Property == "_MainTex");
		int num2 = hashCode2.ToHashCode();
		AltReferencedMaterial altReferencedMaterial = null;
		PoolMaterial value = null;
		if (Application.isPlaying)
		{
			if (_cachedMatBlockReferences.TryGetValue(num2, out var value2) && value2.ReferencedMaterialAndBlock?.SharedMaterial != null)
			{
				if (flag3 && (object)value2.ReferencedMaterialAndBlock.GetMainTexture() == null)
				{
					if (_tmpArtReplacements != null && _tmpArtReplacements.Count > 0)
					{
						UpdateArt(_tmpArtReplacements, value2, artCropFormatName, artOffset);
					}
					else
					{
						UpdateArt(text, value2, primaryArtSuffix, secondaryArtSuffix, artCropFormatName, artOffset);
					}
				}
				_tmpTextureOverrides.Clear();
				_tmpTextureOverrideEntries.Clear();
				_tmpArtReplacements.Clear();
				value2.AddReference();
				return value2.ReferencedMaterialAndBlock;
			}
			if (_cachedSharedMaterials.TryGetValue(num, out value) && value.ReferencedMaterial?.Material != null)
			{
				altReferencedMaterial = value.ReferencedMaterial;
			}
		}
		if (altReferencedMaterial == null)
		{
			altReferencedMaterial = (string.IsNullOrWhiteSpace(overridePath) ? original.CreateAltReferenceMaterialCopy() : new AltReferencedMaterial(overridePath));
			SetUpSharedMaterial(altReferencedMaterial, _tmpArtReplacements, _tmpTextureOverrideEntries, dimmed, dissolve);
			altReferencedMaterial.Material.name = string.Format("{0}:{1}{2}", altReferencedMaterial.Material.name, num, dissolve ? ":DIS" : (dimmed ? ":DIM" : ""));
			value = new PoolMaterial(altReferencedMaterial, num);
			if (Application.isPlaying)
			{
				_cachedSharedMaterials[num] = value;
			}
		}
		AltReferencedMaterialAndBlock altReferencedMaterialAndBlock = new AltReferencedMaterialAndBlock(altReferencedMaterial.Material, num2);
		SetUpMaterialAndBlock(altReferencedMaterialAndBlock, _tmpTextureOverrideEntries, colorOverride, readOnlyList, invertColors);
		PoolPropertyBlockReference poolPropertyBlockReference = new PoolPropertyBlockReference(altReferencedMaterialAndBlock, value);
		if (flag3)
		{
			if (_tmpArtReplacements != null && _tmpArtReplacements.Count > 0)
			{
				UpdateArt(_tmpArtReplacements, poolPropertyBlockReference, artCropFormatName, artOffset);
			}
			else
			{
				UpdateArt(text, poolPropertyBlockReference, primaryArtSuffix, secondaryArtSuffix, artCropFormatName, artOffset);
			}
		}
		if (Application.isPlaying)
		{
			_cachedMatBlockReferences[num2] = poolPropertyBlockReference;
		}
		_tmpTextureOverrides.Clear();
		_tmpTextureOverrideEntries.Clear();
		_tmpArtReplacements.Clear();
		return altReferencedMaterialAndBlock;
		static string GetFileName(string filePath)
		{
			string text2 = filePath;
			int num3 = text2.LastIndexOf('/');
			if (num3 > 0)
			{
				text2 = text2.Substring(num3 + 1);
			}
			int num4 = text2.IndexOf('.');
			if (num4 > 0)
			{
				text2 = text2.Remove(num4);
			}
			return text2;
		}
	}

	private void SetUpSharedMaterial(AltReferencedMaterial refSharedMaterial, List<ArtIdOverride.ArtReplacement> artReplacements, List<TextureOverride.TextureOverrideEntry> textureOverrideEntries, bool isDimmed, bool isDissolved)
	{
		foreach (ArtIdOverride.ArtReplacement artReplacement in artReplacements)
		{
			if (!string.IsNullOrEmpty(artReplacement.ArtPath) && !string.IsNullOrEmpty(artReplacement.Keyword))
			{
				refSharedMaterial.Material.EnableKeyword(artReplacement.Keyword);
			}
		}
		foreach (TextureOverride.TextureOverrideEntry textureOverrideEntry in textureOverrideEntries)
		{
			if (refSharedMaterial.Material.HasProperty(textureOverrideEntry.Property) && textureOverrideEntry.TextureRef != null)
			{
				refSharedMaterial.SetTexture(textureOverrideEntry.Property, textureOverrideEntry.TextureRef.RelativePath);
				if (!string.IsNullOrEmpty(textureOverrideEntry.Keyword))
				{
					refSharedMaterial.Material.EnableKeyword(textureOverrideEntry.Keyword);
				}
			}
		}
		if (isDimmed)
		{
			refSharedMaterial.Material.EnableKeyword("_USEDIMMED_ON");
		}
		else
		{
			refSharedMaterial.Material.DisableKeyword("_USEDIMMED_ON");
		}
		refSharedMaterial.Material.SetFloat(ShaderPropertyIds.UseDimmedPropId, isDimmed ? 1f : 0f);
		if (isDissolved)
		{
			refSharedMaterial.Material.EnableKeyword("_USEDISSOLVED_ON");
		}
	}

	private static string GetOverridePath(AssetLookupSystem als, MaterialOverrideType overrideType)
	{
		switch (overrideType)
		{
		case MaterialOverrideType.Generic:
		{
			AssetLookupTree<MaterialOverride> assetLookupTree2 = als.TreeLoader.LoadTree<MaterialOverride>(returnNewTree: false);
			if (assetLookupTree2 == null)
			{
				return string.Empty;
			}
			MaterialOverride payload2 = assetLookupTree2.GetPayload(als.Blackboard);
			if (payload2 == null)
			{
				return string.Empty;
			}
			AltAssetReference materialRef2 = payload2.MaterialRef;
			if (materialRef2 == null)
			{
				return string.Empty;
			}
			return materialRef2.RelativePath;
		}
		case MaterialOverrideType.CardSleeve:
		{
			AssetLookupTree<Sleeve> assetLookupTree = als.TreeLoader.LoadTree<Sleeve>(returnNewTree: false);
			if (assetLookupTree == null)
			{
				return string.Empty;
			}
			Sleeve payload = assetLookupTree.GetPayload(als.Blackboard);
			if (payload == null)
			{
				return string.Empty;
			}
			AltAssetReference materialRef = payload.MaterialRef;
			if (materialRef == null)
			{
				return string.Empty;
			}
			return materialRef.RelativePath;
		}
		case MaterialOverrideType.None:
			return string.Empty;
		default:
			return string.Empty;
		}
	}

	private void SetUpMaterialAndBlock(AltReferencedMaterialAndBlock matBlockReference, List<TextureOverride.TextureOverrideEntry> textureOverrideEntries, ColorOverride colorOverride, IReadOnlyList<CardColor> cardColors, bool invertColors)
	{
		foreach (TextureOverride.TextureOverrideEntry textureOverrideEntry in textureOverrideEntries)
		{
			if (matBlockReference.SharedMaterial.HasProperty(textureOverrideEntry.Property) && textureOverrideEntry.TextureRef != null)
			{
				matBlockReference.SetTexture(textureOverrideEntry.Property, textureOverrideEntry.TextureRef.RelativePath);
				if (!string.IsNullOrEmpty(textureOverrideEntry.Trigger))
				{
					matBlockReference.MatBlock.SetFloat(textureOverrideEntry.Trigger, 1f);
				}
			}
		}
		if (colorOverride == null || (!matBlockReference.SharedMaterial.HasProperty(colorOverride.PrimaryProperty) && !matBlockReference.SharedMaterial.HasProperty(colorOverride.SecondaryProperty)))
		{
			return;
		}
		CardColorTable cardColorTable = _cardColorCaches.GetCardColorTable(colorOverride.ColorTableRef);
		if ((bool)cardColorTable)
		{
			KeyValuePair<UnityEngine.Color, UnityEngine.Color> colors = cardColorTable.GetColors(cardColors);
			if (matBlockReference.SharedMaterial.HasProperty(colorOverride.PrimaryProperty))
			{
				matBlockReference.MatBlock.SetColor(colorOverride.PrimaryProperty, invertColors ? colors.Value : colors.Key);
			}
			if (matBlockReference.SharedMaterial.HasProperty(colorOverride.SecondaryProperty))
			{
				matBlockReference.MatBlock.SetColor(colorOverride.SecondaryProperty, invertColors ? colors.Key : colors.Value);
			}
		}
	}

	private void UpdateArt(List<ArtIdOverride.ArtReplacement> artReplacements, PoolPropertyBlockReference poolMatRefBlock, string cropFormat, Vector2 artOffset)
	{
		foreach (ArtIdOverride.ArtReplacement artReplacement in artReplacements)
		{
			if (!string.IsNullOrEmpty(artReplacement.ArtPath))
			{
				poolMatRefBlock.ReferencedMaterialAndBlock.SetTexture(artReplacement.Property, _artTextureLoader.GetCardArtPath(artReplacement.ArtPath));
				if (!string.IsNullOrEmpty(artReplacement.Trigger))
				{
					poolMatRefBlock.ReferencedMaterialAndBlock.MatBlock.SetFloat(artReplacement.Trigger, 1f);
				}
				_cropDatabase.GetCrop(artReplacement.ArtPath, cropFormat)?.SetToPropertyBlock(poolMatRefBlock.ReferencedMaterialAndBlock.MatBlock, artOffset);
			}
		}
	}

	private void UpdateArt(string artAssetPath, PoolPropertyBlockReference poolMatRefBlock, string primaryArtSuffix, string secondaryArtSuffix, string cropFormat, Vector2 artOffset)
	{
		string text = null;
		ArtCrop artCrop = null;
		if (!string.IsNullOrWhiteSpace(primaryArtSuffix))
		{
			string text2 = artAssetPath.Replace("_AIF", "_AIF_" + primaryArtSuffix);
			text = _artTextureLoader.GetCardArtPath(text2, returnMissingArt: false);
			artCrop = _cropDatabase.GetCrop(text2, cropFormat);
		}
		if (string.IsNullOrWhiteSpace(text))
		{
			text = _artTextureLoader.GetCardArtPath(artAssetPath);
			artCrop = _cropDatabase.GetCrop(artAssetPath, cropFormat);
		}
		poolMatRefBlock.ReferencedMaterialAndBlock.SetMainTexture(text);
		if (!string.IsNullOrWhiteSpace(secondaryArtSuffix))
		{
			string cardArtPath = _artTextureLoader.GetCardArtPath(artAssetPath.Replace("_AIF", "_AIF_" + secondaryArtSuffix), returnMissingArt: false);
			if (!string.IsNullOrEmpty(cardArtPath))
			{
				poolMatRefBlock.ReferencedMaterialAndBlock.SetTexture("_DepthFoilDistTex", cardArtPath);
			}
		}
		(artCrop ?? ArtCrop.DEFAULT).SetToPropertyBlock(poolMatRefBlock.ReferencedMaterialAndBlock.MatBlock, artOffset);
	}
}
