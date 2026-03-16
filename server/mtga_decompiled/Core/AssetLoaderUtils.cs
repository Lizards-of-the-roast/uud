using UnityEngine;
using UnityEngine.UI;

public static class AssetLoaderUtils
{
	public static bool TrySetTextureFromSprite(Image image, AssetLoader.AssetTracker<Texture2D> assetTracker, string path)
	{
		if (image == null || string.IsNullOrEmpty(path))
		{
			return false;
		}
		image.sprite = null;
		Texture texture = assetTracker?.Acquire(path);
		if (texture == null)
		{
			return false;
		}
		image.sprite = Sprite.Create(rect: new Rect(0f, 0f, texture.width, texture.height), texture: (Texture2D)texture, pivot: new Vector2(0.5f, 0.5f));
		return true;
	}

	public static bool TrySetSprite(Image image, AssetLoader.AssetTracker<Sprite> assetTracker, string path)
	{
		if (image == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(path))
		{
			image.enabled = false;
			return false;
		}
		image.sprite = assetTracker?.Acquire(path);
		image.enabled = image.sprite != null;
		if (image.sprite == null)
		{
			return false;
		}
		return true;
	}

	public static bool TrySetSpriteAndLogOnError(Image image, AssetLoader.AssetTracker<Sprite> assetTracker, string path, string message)
	{
		bool num = TrySetSprite(image, assetTracker, path);
		if (!num && image != null && !string.IsNullOrEmpty(path))
		{
			SimpleLog.LogError($"[SetSprite] {message} path:{path} on image {image}");
		}
		return num;
	}

	public static bool TrySetRendererTexture(SpriteRenderer renderer, AssetLoader.AssetTracker<Texture2D> assetTracker, string path)
	{
		if (renderer == null || string.IsNullOrEmpty(path))
		{
			return false;
		}
		Texture2D texture2D = assetTracker?.Acquire(path);
		if (texture2D == null)
		{
			return false;
		}
		Rect rect = new Rect(0f, 0f, texture2D.width, texture2D.height);
		renderer.sprite = Sprite.Create(texture2D, rect, new Vector2(0.5f, 0.5f));
		return true;
	}

	public static bool TrySetRawImageTexture(RawImage image, AssetLoader.AssetTracker<Texture> assetTracker, string path)
	{
		if (image == null || string.IsNullOrEmpty(path))
		{
			return false;
		}
		image.texture = assetTracker?.Acquire(path);
		return true;
	}

	public static bool TrySetRendererSprite(SpriteRenderer renderer, AssetLoader.AssetTracker<Sprite> assetTracker, string path)
	{
		if (renderer == null || string.IsNullOrEmpty(path))
		{
			return false;
		}
		renderer.sprite = assetTracker?.Acquire(path);
		return true;
	}

	public static void CleanupImage(Image image, AssetLoader.AssetTracker<Sprite> spriteTracker, AssetLoader.AssetTracker<Texture2D> textureTracker = null)
	{
		if (image != null)
		{
			image.sprite = null;
		}
		spriteTracker?.Cleanup();
		textureTracker?.Cleanup();
	}

	public static void CleanupRawImage(RawImage rawImage, AssetLoader.AssetTracker<Texture> textureTracker)
	{
		if (rawImage != null)
		{
			rawImage.texture = null;
		}
		textureTracker?.Cleanup();
	}

	public static void CleanupSpriteRenderer(SpriteRenderer renderer, AssetLoader.AssetTracker<Sprite> spriteTracker, AssetLoader.AssetTracker<Texture2D> textureTracker)
	{
		if (renderer != null)
		{
			renderer.sprite = null;
		}
		spriteTracker?.Cleanup();
		textureTracker?.Cleanup();
	}
}
