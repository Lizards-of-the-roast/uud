using System;
using AssetLookupTree;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;

public class CampaignGraphObjectiveBubble : MonoBehaviour
{
	[SerializeField]
	private Image _primaryImage;

	[SerializeField]
	private Image _secondaryImage;

	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private EPPRewardWebHanger _notificationPopupPrefab;

	[SerializeField]
	private Transform _popupParent;

	[SerializeField]
	private GameObject _locationIndicator;

	[SerializeField]
	private TextMeshProUGUI _circleText;

	[SerializeField]
	private GameObject _sparkyVFX;

	private AssetLoader.AssetTracker<Sprite> _primaryImageSpriteTracker;

	private AssetLoader.AssetTracker<Sprite> _secondaryImageSpriteTracker;

	private AssetLookupSystem _assetLookupSystem;

	public Action<CampaignGraphObjectiveBubble> OnMouseOverObjectiveBubble = delegate
	{
	};

	public Action<CampaignGraphObjectiveBubble> OnMouseClickObjectiveBubble = delegate
	{
	};

	private Action<string> _onClickCallback;

	private bool _locked;

	private EPPRewardWebHanger _notificationPopup;

	private float _timer;

	private bool _mouseOver;

	private bool _isClickToOpen;

	private static readonly int Locked = Animator.StringToHash("Locked");

	private static readonly int Completed = Animator.StringToHash("Completed");

	private static readonly int Selected = Animator.StringToHash("Selected");

	public string ID { get; private set; }

	public bool IsPopupActive
	{
		get
		{
			if (_notificationPopup != null)
			{
				return _notificationPopup.gameObject.activeInHierarchy;
			}
			return false;
		}
		private set
		{
			if (_notificationPopup != null)
			{
				_notificationPopup.gameObject.UpdateActive(value);
			}
		}
	}

	public void Init(AssetLookupSystem assetLookupSystem)
	{
		_assetLookupSystem = assetLookupSystem;
	}

	private void Awake()
	{
		IsPopupActive = false;
	}

	public void Init(Client_ColorChallengeMatchNode node, IColorChallengeTrack track, Action<string> _onClick, string romanNumeral)
	{
		_onClickCallback = _onClick;
		ID = node.Id;
		_circleText.text = romanNumeral;
		RewardDisplayData reward = node.Reward;
		if (_primaryImage != null)
		{
			if (_primaryImageSpriteTracker == null)
			{
				_primaryImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("CampaignBubblePrimaryImageSprite");
			}
			AssetLoaderUtils.TrySetSprite(_primaryImage, _primaryImageSpriteTracker, ProfileUtilities.GetAvatarBustImagePath(_assetLookupSystem, node.OpponentAvatar));
		}
		if (_secondaryImage != null)
		{
			if (_secondaryImageSpriteTracker == null)
			{
				_secondaryImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("CampaignBubbleSecondarySprite");
			}
			AssetLoaderUtils.TrySetSprite(_secondaryImage, _secondaryImageSpriteTracker, reward?.Thumbnail1Path);
			bool active = !string.IsNullOrEmpty(reward?.Thumbnail1Path) || track.IsNodeCompleted(ID);
			_secondaryImage.transform.parent.gameObject.SetActive(active);
		}
		if (reward != null)
		{
			if (_notificationPopup == null)
			{
				_notificationPopup = UnityEngine.Object.Instantiate(_notificationPopupPrefab, _popupParent);
				EPPRewardWebHanger notificationPopup = _notificationPopup;
				notificationPopup.OnEventTriggerPointerEnter = (Action)Delegate.Combine(notificationPopup.OnEventTriggerPointerEnter, (Action)delegate
				{
					_mouseOver = true;
				});
				EPPRewardWebHanger notificationPopup2 = _notificationPopup;
				notificationPopup2.OnEventTriggerPointerExit = (Action)Delegate.Combine(notificationPopup2.OnEventTriggerPointerExit, new Action(Button_OnMouseOff));
			}
			_notificationPopup.SetRewardData(reward).SetGraphNode(_assetLookupSystem, node);
			_notificationPopup.gameObject.SetActive(value: false);
		}
		else if (_notificationPopup != null)
		{
			_notificationPopup.gameObject.SetActive(value: false);
			UnityEngine.Object.Destroy(_notificationPopup);
			_notificationPopup = null;
		}
	}

	public void ResetAnimations()
	{
		_animator.SetBool(Locked, value: false);
		_animator.SetBool(Completed, value: false);
		_animator.SetBool(Selected, value: false);
	}

	public void SetToLock()
	{
		_locked = true;
		_animator.SetBool(Locked, value: true);
	}

	public void SetNextToUnlock()
	{
		_locked = false;
	}

	public void SetToCompleted()
	{
		_locked = false;
		_animator.SetBool(Completed, value: true);
	}

	public void SetSelected()
	{
		_animator.SetBool(Selected, value: true);
	}

	public void SetIsClickToOpen(bool isClickToOpen)
	{
		_isClickToOpen = isClickToOpen;
	}

	public Vector3 GetIndicatorPosition()
	{
		return _locationIndicator.transform.position;
	}

	public Vector3 GetIndicatorCanvasPosition()
	{
		Vector3[] array = new Vector3[4];
		_locationIndicator.GetComponent<RectTransform>().GetWorldCorners(array);
		return array[0];
	}

	public void SetActivePromotionVFX(bool val)
	{
		if (_sparkyVFX != null)
		{
			_sparkyVFX.UpdateActive(val);
		}
	}

	public void Button_OnMouseOver()
	{
		OnMouseOverObjectiveBubble?.Invoke(this);
		_mouseOver = true;
	}

	public void Button_OnMouseOff()
	{
		_mouseOver = false;
		_timer = 1f;
	}

	public void ActivatePopup()
	{
		IsPopupActive = true;
		if (_notificationPopup != null)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_quest_rollover_a, base.gameObject);
		}
	}

	public void DeactivatePopup()
	{
		IsPopupActive = false;
	}

	public void _button_OnClick()
	{
		OnMouseClickObjectiveBubble?.Invoke(this);
		if (!_locked)
		{
			_onClickCallback?.Invoke(ID);
			SetSelected();
		}
	}

	private void Update()
	{
		if (!_isClickToOpen && !_mouseOver && _timer > 0f)
		{
			_timer -= Time.deltaTime;
			if (_timer <= 0f)
			{
				DeactivatePopup();
			}
		}
	}

	private void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(_primaryImage, _primaryImageSpriteTracker);
		AssetLoaderUtils.CleanupImage(_secondaryImage, _secondaryImageSpriteTracker);
	}
}
