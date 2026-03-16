using UnityEngine;

public class SpriteRendererReferenceLoader
{
	private readonly SpriteRenderer _spriteRenderer;

	private readonly AssetLoader.AssetTracker<Texture2D> _textureTracker = new AssetLoader.AssetTracker<Texture2D>("SpriteRendererReferenceLoaderTexture");

	private readonly AssetLoader.AssetTracker<Sprite> _spriteTracker = new AssetLoader.AssetTracker<Sprite>("SpriteRendererReferenceLoaderSprite");

	private bool UsingSprite => !string.IsNullOrEmpty(_spriteTracker.LastPath);

	public SpriteRendererReferenceLoader(SpriteRenderer spriteRenderer)
	{
		_spriteRenderer = spriteRenderer;
	}

	public void SetSprite(string spritePath)
	{
		if (!UsingSprite)
		{
			Cleanup();
		}
		_spriteRenderer.sprite = _spriteTracker.Acquire(spritePath);
	}

	public void SetTexture(string texturePath)
	{
		if (UsingSprite)
		{
			Cleanup();
		}
		Texture2D texture2D = _textureTracker.Acquire(texturePath);
		Rect rect = new Rect(0f, 0f, texture2D.width, texture2D.height);
		_spriteRenderer.sprite = Sprite.Create(texture2D, rect, new Vector2(0.5f, 0.5f));
	}

	public void SetSprite(Sprite sprite)
	{
		Cleanup();
		_spriteRenderer.sprite = sprite;
	}

	public void Cleanup()
	{
		_spriteRenderer.sprite = null;
		_spriteTracker.Cleanup();
		_textureTracker.Cleanup();
	}
}
