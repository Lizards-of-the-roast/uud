using System;
using UnityEngine;
using Wotc.Mtga.Cards.ArtCrops;

public class ApplyCardArtToVFX : MonoBehaviour
{
	[Serializable]
	public struct RendererProperty
	{
		public Renderer Renderer;

		public string Property;

		public string CropFormat;
	}

	[Tooltip("Objects in this list will have their MainTexture replaced with that of the card causing them.")]
	public RendererProperty[] TargetRenderers = new RendererProperty[1];

	private readonly AssetTracker _cardAssetTracker = new AssetTracker();

	public void Apply(string artPath, CardArtTextureLoader artTextureLoader, IArtCropProvider cropDatabase)
	{
		if (string.IsNullOrWhiteSpace(artPath) || artTextureLoader == null || cropDatabase == null)
		{
			return;
		}
		Texture2D texture2D = artTextureLoader.AcquireCardArt(_cardAssetTracker, "ApplyCardArtToVFX", artPath, returnMissingArt: false);
		if (!texture2D)
		{
			return;
		}
		RendererProperty[] targetRenderers = TargetRenderers;
		for (int i = 0; i < targetRenderers.Length; i++)
		{
			RendererProperty rendererProperty = targetRenderers[i];
			if ((bool)rendererProperty.Renderer && (bool)rendererProperty.Renderer.material && !string.IsNullOrWhiteSpace(rendererProperty.Property))
			{
				if (!rendererProperty.Renderer.material.HasProperty(rendererProperty.Property))
				{
					Debug.LogErrorFormat("Material \"{0}\" on Renderer \"{1}\" has no Property named \"{2}\".", rendererProperty.Renderer.material.name, rendererProperty.Renderer.name, rendererProperty.Property);
				}
				else
				{
					rendererProperty.Renderer.material.SetTexture(rendererProperty.Property, texture2D);
					cropDatabase.GetCrop(artPath, rendererProperty.CropFormat)?.ApplyToMaterial(rendererProperty.Renderer.material);
				}
			}
		}
	}

	private void OnDestroy()
	{
		_cardAssetTracker.Cleanup();
	}
}
