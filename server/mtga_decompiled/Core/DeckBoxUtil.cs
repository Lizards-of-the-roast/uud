using System;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Pooling;
using UnityEngine;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wotc.Mtga.Cards.ArtCrops;
using Wotc.Mtga.Cards.Database;

public static class DeckBoxUtil
{
	public static MetaDeckView CreateDeckBox(AssetLookupSystem assetLookupSystem, IUnityObjectPool objectPool, BILogger biLogger)
	{
		string prefabPath = assetLookupSystem.GetPrefabPath<DeckBoxPrefab, MetaDeckView>();
		GameObject gameObject = objectPool.PopObject(prefabPath);
		if (!gameObject)
		{
			CreateDeckBoxError payload = new CreateDeckBoxError
			{
				EventTime = DateTime.UtcNow,
				Error = "DeckBox prefab could not be Instantiated.",
				Prefab = prefabPath
			};
			biLogger.Send(ClientBusinessEventType.CreateDeckBoxError, payload);
			return null;
		}
		MetaDeckView component = gameObject.GetComponent<MetaDeckView>();
		if (!component)
		{
			CreateDeckBoxError payload2 = new CreateDeckBoxError
			{
				EventTime = DateTime.UtcNow,
				Error = "Instantiated DeckBox did not have the MetaDeckView component.",
				Prefab = prefabPath
			};
			biLogger.Send(ClientBusinessEventType.CreateDeckBoxError, payload2);
			return null;
		}
		return component;
	}

	public static void DestroyDeckBox(MetaDeckView deckView, IUnityObjectPool objectPool)
	{
		if ((bool)deckView)
		{
			deckView.SetIsSelected(isSelected: false);
			deckView.Cleanup();
			objectPool.PushObject(deckView.gameObject);
		}
	}

	public static void SetDeckBoxTexture(string artPath, ArtCrop crop, MeshRendererReferenceLoader[] meshRenderers, Texture fallbackTexture = null)
	{
		if (string.IsNullOrEmpty(artPath) && fallbackTexture == null)
		{
			return;
		}
		if (!string.IsNullOrEmpty(artPath))
		{
			artPath = new CardArtTextureLoader().GetCardArtPath(artPath);
		}
		foreach (MeshRendererReferenceLoader meshRendererReferenceLoader in meshRenderers)
		{
			if (meshRendererReferenceLoader == null)
			{
				continue;
			}
			Material[] sharedMaterials = meshRendererReferenceLoader.GetSharedMaterials();
			for (int j = 0; j < sharedMaterials.Length; j++)
			{
				if (!(sharedMaterials[j] == null) && sharedMaterials[j].name.Contains("ArtInFrame"))
				{
					meshRendererReferenceLoader.SetPropertyBlockTexture(j, "_MainTex", artPath, fallbackTexture);
					meshRendererReferenceLoader.CropWithPropertyBlock(j, crop);
				}
			}
			meshRendererReferenceLoader.ApplyPropertyBlocks();
		}
	}

	public static void SetDeckBoxTexture(string artPath, CardArtTextureLoader textureLoader, IArtCropProvider cropDatabase, MeshRendererReferenceLoader[] meshRenderers, Texture defaultTexture = null)
	{
		SetDeckBoxTexture(textureLoader.GetCardArtPath(artPath), cropDatabase.GetCrop(artPath, "Normal"), meshRenderers, defaultTexture);
	}

	public static void SetDeckBoxTexture(uint grpId, ICardDataProvider cardDatabase, CardArtTextureLoader textureLoader, IArtCropProvider cropDatabase, MeshRendererReferenceLoader[] meshRenderers)
	{
		SetDeckBoxTexture(cardDatabase.GetCardPrintingById(grpId)?.ImageAssetPath ?? string.Empty, textureLoader, cropDatabase, meshRenderers);
	}
}
