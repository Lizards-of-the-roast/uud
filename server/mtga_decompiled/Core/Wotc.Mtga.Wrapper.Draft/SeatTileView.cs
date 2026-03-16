using AssetLookupTree;
using AssetLookupTree.Payloads.Avatar;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Wrapper.Draft;

public class SeatTileView : MonoBehaviour
{
	private int Ready_BoolFlag = Animator.StringToHash("Ready");

	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private Image _avatarImage;

	[SerializeField]
	private Sprite _anonymousSeatImage;

	[Header("Localization Parameters")]
	[SerializeField]
	private Localize _displayNameLocalize;

	[SerializeField]
	private Localize _statusLocalize;

	[SerializeField]
	private MTGALocalizedString _filledSeatKey;

	private AssetLoader.AssetTracker<Sprite> _avatarImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("SeatTileAvatarSprite");

	public void ResetSeat()
	{
		AnonymousSeatVisualData seatVisualData = new AnonymousSeatVisualData(isVisible: false);
		_avatarImageSpriteTracker.Cleanup();
		_avatarImage.sprite = _anonymousSeatImage;
		UpdateAnonymousSeat(seatVisualData);
	}

	public void UpdateAnonymousSeat(AnonymousSeatVisualData seatVisualData)
	{
		base.gameObject.UpdateActive(seatVisualData.IsVisible);
		_displayNameLocalize.SetText(_filledSeatKey);
		_statusLocalize.SetText(seatVisualData.StatusKey);
	}

	public void UpdateReadySeat(AnonymousSeatVisualData seatVisualData)
	{
		UpdateAnonymousSeat(seatVisualData);
		_animator.SetBool(Ready_BoolFlag, seatVisualData.IsReady);
	}

	public void UpdateKnownSeat(KnownSeatVisualData seatVisualData, AssetLookupSystem assetLookupSystem)
	{
		UnlocalizedMTGAString unlocalizedMTGAString = new UnlocalizedMTGAString();
		unlocalizedMTGAString.Key = seatVisualData.DisplayName;
		_displayNameLocalize.SetText((MTGALocalizedString)unlocalizedMTGAString);
		_statusLocalize.SetText(seatVisualData.StatusKey);
		_animator.SetBool(Ready_BoolFlag, seatVisualData.IsReady);
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.CosmeticAvatarId = seatVisualData.AvatarId;
		ThumbnailPayload payload = assetLookupSystem.TreeLoader.LoadTree<ThumbnailPayload>().GetPayload(assetLookupSystem.Blackboard);
		if (payload != null)
		{
			AssetLoaderUtils.TrySetSprite(_avatarImage, _avatarImageSpriteTracker, payload.Reference.RelativePath);
		}
		assetLookupSystem.Blackboard.Clear();
	}

	public void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(_avatarImage, _avatarImageSpriteTracker);
	}
}
