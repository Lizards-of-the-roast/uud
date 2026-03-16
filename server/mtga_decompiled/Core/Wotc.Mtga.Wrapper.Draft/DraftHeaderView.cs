using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Avatar;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Wrapper.Draft;

public class DraftHeaderView : MonoBehaviour, IDraftHeaderView
{
	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private DraftBoosterView _boosterViewPrefab;

	[SerializeField]
	private Sprite _defaultSprite;

	[Header("Static Parameters")]
	[SerializeField]
	private TextMeshProUGUI _localSeatDisplayNameText;

	[SerializeField]
	private Image _localSeatAvatarImage;

	[SerializeField]
	private Image _leftSeatAvatarImage;

	[SerializeField]
	private Image _rightSeatAvatarImage;

	[SerializeField]
	private CustomButton _localSeatAvatar;

	[SerializeField]
	private CustomButton _leftSeatAvatar;

	[SerializeField]
	private CustomButton _rightSeatAvatar;

	[Header("Dynamic Parameters")]
	[SerializeField]
	private Localize _localSeatStatusText;

	[SerializeField]
	private Transform _localSeatPackQueueParent;

	[SerializeField]
	private Transform _leftSeatPackQueueParent;

	[SerializeField]
	private Transform _rightSeatPackQueueParent;

	[SerializeField]
	private TextMeshProUGUI _leftSeatPackQueueNumberText;

	[SerializeField]
	private TextMeshProUGUI _rightSeatPackQueueNumberText;

	[SerializeField]
	private GameObject _leftSeatPackQueueNumberGroup;

	[SerializeField]
	private GameObject _rightSeatPackQueueNumberGroup;

	private Queue<IDraftBoosterView> _boosterViewPool = new Queue<IDraftBoosterView>();

	private List<IDraftBoosterView> _localSeatBoosterViewList = new List<IDraftBoosterView>();

	private List<IDraftBoosterView> _leftSeatBoosterViewList = new List<IDraftBoosterView>();

	private List<IDraftBoosterView> _rightSeatBoosterViewList = new List<IDraftBoosterView>();

	private int ToLeft_BoolFlag = Animator.StringToHash("ToLeft");

	private Dictionary<string, string> _statusLocParameters;

	private AssetLoader.AssetTracker<Sprite> _localSeatAvatarSpriteTracker = new AssetLoader.AssetTracker<Sprite>("DraftHeaderLocalSeatSprite");

	private AssetLoader.AssetTracker<Sprite> _leftSeatAvatarSpriteTracker = new AssetLoader.AssetTracker<Sprite>("DraftHeaderLeftSeatSprite");

	private AssetLoader.AssetTracker<Sprite> _rightSeatAvatarSpriteTracker = new AssetLoader.AssetTracker<Sprite>("DraftHeaderRightSeatSprite");

	private AssetLookupSystem _assetLookupSystem;

	private DynamicDraftStateVisualData _stateVisualData;

	private void OnEnable()
	{
		_animator.SetBool(ToLeft_BoolFlag, _stateVisualData.PassDirectionIsLeft);
	}

	public void InitDraftState(StaticDraftStateVisualData stateVisualData, AssetLookupSystem assetLookupSystem)
	{
		_statusLocParameters = new Dictionary<string, string>();
		_statusLocParameters.Add("pack", "1");
		_statusLocParameters.Add("pick", "1");
		_statusLocParameters.Add("numberOfCards", "1");
		_localSeatStatusText.SetText("Social/Presence/Detail_PackPick", _statusLocParameters);
		_localSeatAvatar.Interactable = stateVisualData.IsInteractable;
		_leftSeatAvatar.Interactable = stateVisualData.IsInteractable;
		_rightSeatAvatar.Interactable = stateVisualData.IsInteractable;
		_localSeatDisplayNameText.text = stateVisualData.LocalSeatDisplayName;
		_assetLookupSystem = assetLookupSystem;
		SetSeatAvatarImage(stateVisualData.LocalSeatAvatarId, SeatLocationId.LocalSeat);
		SetSeatAvatarImage(stateVisualData.LeftSeatAvatarId, SeatLocationId.LeftSeat);
		SetSeatAvatarImage(stateVisualData.RightSeatAvatarId, SeatLocationId.RightSeat);
	}

	public void UpdateDraftState(DynamicDraftStateVisualData stateVisualData)
	{
		_stateVisualData = stateVisualData;
		_animator.SetBool(ToLeft_BoolFlag, stateVisualData.PassDirectionIsLeft);
		_statusLocParameters["pack"] = stateVisualData.PackNumber.ToString();
		_statusLocParameters["pick"] = stateVisualData.PickNumber.ToString();
		_statusLocParameters["numberOfCards"] = stateVisualData.NumberOfCardsToPick.ToString();
		_localSeatStatusText.SetText((stateVisualData.NumberOfCardsToPick > 1) ? "Social/Presence/Detail_PackPick_PickX" : "Social/Presence/Detail_PackPick", _statusLocParameters);
		_localSeatBoosterViewList = DraftHeaderFunctions.UpdateSeatBoosters(this, _localSeatBoosterViewList, stateVisualData.PacksOnLocalSeat, stateVisualData.PassDirectionIsLeft, SeatLocationId.LocalSeat);
		_leftSeatBoosterViewList = DraftHeaderFunctions.UpdateSeatBoosters(this, _leftSeatBoosterViewList, stateVisualData.PacksOnLeftSeat, stateVisualData.PassDirectionIsLeft, SeatLocationId.LeftSeat);
		if (_leftSeatPackQueueNumberText != null)
		{
			_leftSeatPackQueueNumberText.text = "x" + _leftSeatBoosterViewList.Count;
			_leftSeatPackQueueNumberGroup.UpdateActive(_leftSeatBoosterViewList.Count > 0);
		}
		_rightSeatBoosterViewList = DraftHeaderFunctions.UpdateSeatBoosters(this, _rightSeatBoosterViewList, stateVisualData.PacksOnRightSeat, stateVisualData.PassDirectionIsLeft, SeatLocationId.RightSeat);
		if (_rightSeatPackQueueNumberText != null)
		{
			_rightSeatPackQueueNumberText.text = "x" + _rightSeatBoosterViewList.Count;
			_rightSeatPackQueueNumberGroup.UpdateActive(_rightSeatBoosterViewList.Count > 0);
		}
	}

	public void SetHeaderOnClickCallback(Action onClickCallback)
	{
		_localSeatAvatar.OnClick.AddListener(delegate
		{
			onClickCallback?.Invoke();
		});
		_leftSeatAvatar.OnClick.AddListener(delegate
		{
			onClickCallback?.Invoke();
		});
		_rightSeatAvatar.OnClick.AddListener(delegate
		{
			onClickCallback?.Invoke();
		});
	}

	public void AddBoosterViewToPool(IDraftBoosterView boosterView)
	{
		DraftBoosterView draftBoosterView = boosterView as DraftBoosterView;
		if (!(draftBoosterView == null))
		{
			draftBoosterView.gameObject.UpdateActive(active: false);
			_boosterViewPool.Enqueue(draftBoosterView);
		}
	}

	public IDraftBoosterView CreateBoosterView(CollationMapping boosterId, SeatLocationId seatLocationId)
	{
		DraftBoosterView draftBoosterView;
		if (_boosterViewPool.Count == 0)
		{
			draftBoosterView = UnityEngine.Object.Instantiate(_boosterViewPrefab);
		}
		else
		{
			draftBoosterView = _boosterViewPool.Dequeue() as DraftBoosterView;
			draftBoosterView.gameObject.SetActive(value: true);
		}
		draftBoosterView.SetBoosterData(boosterId);
		switch (seatLocationId)
		{
		case SeatLocationId.LocalSeat:
			draftBoosterView.transform.SetParent(_localSeatPackQueueParent, worldPositionStays: false);
			break;
		case SeatLocationId.LeftSeat:
			draftBoosterView.transform.SetParent(_leftSeatPackQueueParent, worldPositionStays: false);
			break;
		case SeatLocationId.RightSeat:
			draftBoosterView.transform.SetParent(_rightSeatPackQueueParent, worldPositionStays: false);
			break;
		}
		draftBoosterView.transform.SetAsLastSibling();
		draftBoosterView.transform.ZeroOut();
		return draftBoosterView;
	}

	private void SetSeatAvatarImage(string avatarId, SeatLocationId boosterParentId)
	{
		CustomButton customButton;
		Image image;
		AssetLoader.AssetTracker<Sprite> assetTracker;
		switch (boosterParentId)
		{
		case SeatLocationId.LeftSeat:
			customButton = _leftSeatAvatar;
			image = _leftSeatAvatarImage;
			assetTracker = _leftSeatAvatarSpriteTracker;
			break;
		case SeatLocationId.RightSeat:
			customButton = _rightSeatAvatar;
			image = _rightSeatAvatarImage;
			assetTracker = _rightSeatAvatarSpriteTracker;
			break;
		default:
			customButton = _localSeatAvatar;
			image = _localSeatAvatarImage;
			assetTracker = _localSeatAvatarSpriteTracker;
			break;
		}
		bool flag = avatarId == "**";
		customButton.gameObject.UpdateActive(!flag);
		if (!flag)
		{
			_assetLookupSystem.Blackboard.Clear();
			_assetLookupSystem.Blackboard.CosmeticAvatarId = avatarId;
			ThumbnailPayload payload = _assetLookupSystem.TreeLoader.LoadTree<ThumbnailPayload>().GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				AssetLoaderUtils.TrySetSprite(image, assetTracker, payload.Reference.RelativePath);
			}
			else
			{
				image.sprite = _defaultSprite;
			}
			_assetLookupSystem.Blackboard.Clear();
		}
	}

	public void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(_localSeatAvatarImage, _localSeatAvatarSpriteTracker);
		AssetLoaderUtils.CleanupImage(_leftSeatAvatarImage, _leftSeatAvatarSpriteTracker);
		AssetLoaderUtils.CleanupImage(_rightSeatAvatarImage, _rightSeatAvatarSpriteTracker);
	}
}
