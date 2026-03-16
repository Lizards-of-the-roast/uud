using UnityEngine;
using UnityEngine.UI;

public class RawImageReferenceLoader
{
	private readonly RawImage _rawImage;

	private readonly AssetLoader.AssetTracker<Texture> _textureTracker = new AssetLoader.AssetTracker<Texture>("RawImageReferenceLoader");

	public RawImageReferenceLoader(RawImage rawImage)
	{
		_rawImage = rawImage;
	}

	public void SetTexture(string texturePath)
	{
		if (string.IsNullOrWhiteSpace(texturePath))
		{
			texturePath = null;
		}
		_rawImage.texture = _textureTracker.Acquire(texturePath);
	}

	public void Cleanup()
	{
		_rawImage.texture = null;
		_textureTracker.Cleanup();
	}
}
