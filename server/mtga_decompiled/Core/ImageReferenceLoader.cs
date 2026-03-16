using UnityEngine;
using UnityEngine.UI;

public class ImageReferenceLoader
{
	private Image _image;

	private AssetTracker assetTracker = new AssetTracker();

	public string CurrentSpritePath;

	private string _currentTexturePath;

	public ImageReferenceLoader(Image image)
	{
		_image = image;
	}

	public void SetSprite(string spritePath)
	{
		if (CurrentSpritePath != spritePath || (_image.sprite != null && string.IsNullOrEmpty(spritePath)))
		{
			ClearSprite();
			if (!string.IsNullOrEmpty(spritePath))
			{
				_image.sprite = AssetLoader.AcquireAndTrackAsset<Sprite>(assetTracker, "sprite", spritePath);
				CurrentSpritePath = spritePath;
			}
		}
	}

	public void SetSprite(Sprite sprite)
	{
		ClearSprite();
		_image.sprite = sprite;
	}

	public void Cleanup()
	{
		ClearSprite();
	}

	public void CreateSprite(string spritePath, string texturePath)
	{
		if (texturePath == null)
		{
			SetSprite(spritePath);
		}
		if (_currentTexturePath != texturePath && spritePath == null)
		{
			ClearSprite();
			CurrentSpritePath = null;
			_currentTexturePath = texturePath;
			Texture texture = AssetLoader.AcquireAndTrackAsset<Texture>(assetTracker, "texture", texturePath);
			Rect rect = new Rect(0f, 0f, texture.width, texture.height);
			_image.sprite = Sprite.Create((Texture2D)texture, rect, new Vector2(0.5f, 0.5f));
		}
	}

	private void ClearSprite()
	{
		if (_image != null)
		{
			_image.sprite = null;
		}
		assetTracker.Cleanup();
		CurrentSpritePath = string.Empty;
	}
}
