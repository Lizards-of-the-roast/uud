using UnityEngine;
using UnityEngine.UI;

namespace Wizards.MDN.Objectives;

public class RewardDataSetImage : AbstractDataConsumer<RewardDisplayData, RewardDataProvider>
{
	[SerializeField]
	private Image _image;

	private AssetLoader.AssetTracker<Sprite> _spriteTracker = new AssetLoader.AssetTracker<Sprite>("RewardDataSetImage");

	protected override void OnDataChanged()
	{
		_image.sprite = _spriteTracker.Acquire(base.Data?.Thumbnail1Path);
	}

	public void OnDestroy()
	{
		_spriteTracker.Cleanup();
	}
}
