using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace Wizards.Mtga.Rank;

public class RankView : MonoBehaviour
{
	[Header("Parts")]
	[SerializeField]
	private Animator _pipAnimator;

	[SerializeField]
	private Image _rankImage;

	[SerializeField]
	private Localize _rankFormatLoc;

	[SerializeField]
	private Localize _rankTierLoc;

	[Header("Scaffolds")]
	[SerializeField]
	private GameObject _artParent;

	[SerializeField]
	private GameObject _formatTierTextParent;

	[FormerlySerializedAs("_pipParentGameObject")]
	[SerializeField]
	private GameObject _pipParent;

	[FormerlySerializedAs("_backer")]
	[SerializeField]
	private GameObject _backerGameObject;

	private AssetLoader.AssetTracker<Sprite> _rankImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("RankViewImageSprite");

	private RankViewInfo _rankViewInfo;

	private const string Anim_PipCount_Int = "PipCount";

	private const string Anim_PipFill_Current_Int = "PipFill_Current";

	private const string Anim_PipFill_New_Int = "PipFill_New";

	private void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(_rankImage, _rankImageSpriteTracker);
	}

	public void Initialize()
	{
	}

	public void SetModel(RankViewInfo rankViewInfo)
	{
		_rankViewInfo = rankViewInfo;
		if (_rankViewInfo == null)
		{
			HideAll();
		}
		else
		{
			ApplyModel(_rankViewInfo);
		}
	}

	private void ApplyModel(RankViewInfo rankViewInfo)
	{
		_artParent.UpdateActive(active: true);
		_formatTierTextParent.UpdateActive(active: true);
		_formatTierTextParent.UpdateActive(rankViewInfo.ShowText);
		_rankFormatLoc.SetText(rankViewInfo.RankFormatLocText);
		_rankTierLoc.SetText(rankViewInfo.RankTierLocText, rankViewInfo.RankTierLocTextParams);
		if (!string.IsNullOrEmpty(rankViewInfo.RankImageAssetPath))
		{
			_backerGameObject.UpdateActive(rankViewInfo.ShowBacker);
			AssetLoaderUtils.TrySetSprite(_rankImage, _rankImageSpriteTracker, rankViewInfo.RankImageAssetPath);
			if (rankViewInfo.UseNativeImageSize)
			{
				_rankImage.SetNativeSize();
			}
			if (rankViewInfo.ShowPips && !rankViewInfo.IsMythic)
			{
				_pipParent.UpdateActive(active: true);
				_pipAnimator.SetInteger("PipCount", rankViewInfo.MaxPips);
				_pipAnimator.SetInteger("PipFill_Current", rankViewInfo.Steps);
				_pipAnimator.SetInteger("PipFill_New", rankViewInfo.Steps);
			}
			else
			{
				_pipParent.UpdateActive(active: false);
			}
		}
	}

	private void HideAll()
	{
		_artParent.UpdateActive(active: false);
		_formatTierTextParent.UpdateActive(active: false);
	}
}
