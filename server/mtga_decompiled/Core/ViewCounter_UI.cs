using UnityEngine;
using UnityEngine.UI;

public class ViewCounter_UI : ViewCounter
{
	[SerializeField]
	protected Image backgroundImage;

	private readonly AssetLoader.AssetTracker<Sprite> _assetTracker = new AssetLoader.AssetTracker<Sprite>("ViewCounterBackground");

	public void SetBackground(string spritePath)
	{
		backgroundImage.sprite = _assetTracker.Acquire(spritePath);
	}

	public void OnDestroy()
	{
		_assetTracker.Cleanup();
	}
}
