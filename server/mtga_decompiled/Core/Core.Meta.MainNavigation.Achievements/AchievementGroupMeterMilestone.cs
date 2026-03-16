using System.Collections.Generic;
using System.Linq;
using Core.Code.AssetLookupTree.AssetLookup;
using Core.Meta.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wizards.GeneralUtilities;
using Wizards.Mtga;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.CustomInput;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace Core.Meta.MainNavigation.Achievements;

[RequireComponent(typeof(Animator))]
public class AchievementGroupMeterMilestone : UIBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerClickHandler, IPointerExitHandler
{
	private enum HandheldPopupState
	{
		Closed,
		Opening,
		Open
	}

	[SerializeField]
	private Image _rewardImage;

	[SerializeField]
	private Localize _rewardCountText;

	[SerializeField]
	private TextMeshProUGUI _achievementIndexText;

	[SerializeField]
	private GameObject _poptartParent;

	[SerializeField]
	[AdditionalInformation("These will be disabled when there is no reward.")]
	private GameObject[] _gameObjectsToDisableIfNoReward;

	private Animator _animator;

	private IAchievementGroupReward _rewardItem;

	private int _index;

	private AssetLoader.AssetTracker<Sprite> _rewardAssetTracker;

	private NotificationPopup _notificationPopup;

	private HandheldPopupState _handheldPopupState;

	public Animator Animator
	{
		get
		{
			if (_animator == null)
			{
				_animator = GetComponent<Animator>();
			}
			return _animator;
		}
	}

	private string AssetTrackerRewardSpriteKeyName => _rewardItem.ThumbnailPath + " Meta Meter Bubble Reward " + _index;

	public void SetRewardItem(IAchievementGroupReward rewardItem, int rewardIndex)
	{
		_rewardItem = rewardItem;
		_index = rewardIndex;
		_achievementIndexText.text = _index.ToString();
		GameObject[] gameObjectsToDisableIfNoReward = _gameObjectsToDisableIfNoReward;
		for (int i = 0; i < gameObjectsToDisableIfNoReward.Length; i++)
		{
			gameObjectsToDisableIfNoReward[i].SetActive(_rewardItem != null);
		}
		if (_rewardItem == null || _rewardItem.Amount == 0)
		{
			if (_rewardCountText != null)
			{
				_rewardCountText.gameObject.SetActive(value: false);
			}
			return;
		}
		if (_rewardAssetTracker == null)
		{
			_rewardAssetTracker = new AssetLoader.AssetTracker<Sprite>(AssetTrackerRewardSpriteKeyName);
		}
		if (!AssetLoaderUtils.TrySetSprite(_rewardImage, _rewardAssetTracker, _rewardItem.ThumbnailPath))
		{
			SimpleLog.LogErrorFormat("Meta reward bubble failed to load reward server art for Group ({0}) at index ({1}) reward Id: {2}", _rewardItem.GroupRewardId, _index, _rewardItem.RewardIconPrefab);
		}
		if (_rewardCountText != null)
		{
			if (_rewardItem.Amount > 1)
			{
				_rewardCountText.SetText(_rewardCountText.TextTarget.locKey, new Dictionary<string, string> { 
				{
					"number",
					_rewardItem.Amount.ToString()
				} });
			}
			_rewardCountText.gameObject.SetActive(rewardItem.Amount > 1);
		}
		SetRewardPoptart();
	}

	public void SetRewardPoptart()
	{
		string prefabPath = AchievementsScreenHelperFunctions.GetPrefabPath(Pantry.Get<AssetLookupManager>().AssetLookupSystem, "AchievementPoptart");
		_notificationPopup = AssetLoader.Instantiate(prefabPath, _poptartParent.transform).GetComponent<NotificationPopup>();
		if (!(_notificationPopup == null))
		{
			CardDatabase cardDatabase = Pantry.Get<CardDatabase>();
			CardViewBuilder cardViewBuilder = Pantry.Get<CardViewBuilder>();
			_notificationPopup.Init(cardDatabase, cardViewBuilder);
			RewardDisplayData popupDisplayData = new RewardDisplayData
			{
				Popup3dObjectPath = ServerRewardUtils.FormatAssetFromServerReference(_rewardItem.RewardIconPrefab, ServerRewardFileExtension.Prefab),
				ReferenceID = _rewardItem.CosmeticRewardReferenceID
			};
			NotificationPopup.PopupData popupData = new NotificationPopup.PopupData();
			popupData.HeaderString1 = new MTGALocalizedString
			{
				Key = "MainNav/EventRewards/Reward"
			};
			popupData.HeaderString2 = new MTGALocalizedString
			{
				Key = _rewardItem.TitleLocKey
			};
			popupData.DescriptionString = new MTGALocalizedString
			{
				Key = "MainNav/General/Empty_String"
			};
			popupData.ProgressString = new MTGALocalizedString
			{
				Key = "MainNav/General/Simple_Number",
				Parameters = new Dictionary<string, string> { 
				{
					"number",
					_rewardItem.Amount.ToString()
				} }
			};
			popupData.SetPopupDisplayData(popupDisplayData);
			popupData.ApplyData(_notificationPopup);
			_notificationPopup.gameObject.UpdateActive(active: false);
		}
	}

	public void OnPointerEnter(PointerEventData data)
	{
		if (!PlatformUtils.IsHandheld())
		{
			ShowPopup();
		}
	}

	public void OnPointerExit(PointerEventData data)
	{
		if (!PlatformUtils.IsHandheld())
		{
			HidePopup();
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (PlatformUtils.IsHandheld() && _handheldPopupState == HandheldPopupState.Closed)
		{
			ShowPopup();
			_handheldPopupState = HandheldPopupState.Opening;
		}
	}

	public void Update()
	{
		if (_notificationPopup != null && _notificationPopup.gameObject.activeSelf && PlatformUtils.IsHandheld())
		{
			if (_handheldPopupState == HandheldPopupState.Opening && CustomInputModule.PointerWasReleasedThisFrame())
			{
				_handheldPopupState = HandheldPopupState.Open;
			}
			else if (_handheldPopupState == HandheldPopupState.Open && CustomInputModule.PointerWasPressedThisFrame() && CustomInputModule.GetHovered().All((GameObject hovered) => hovered != base.gameObject && hovered != _notificationPopup.gameObject))
			{
				HidePopup();
				_handheldPopupState = HandheldPopupState.Closed;
			}
		}
	}

	public void ShowPopup()
	{
		_notificationPopup.gameObject.UpdateActive(active: true);
	}

	public void HidePopup()
	{
		_notificationPopup.gameObject.UpdateActive(active: false);
	}

	protected override void OnDestroy()
	{
		if (_rewardImage != null)
		{
			_rewardImage.sprite = null;
		}
		if (_rewardAssetTracker != null)
		{
			_rewardAssetTracker.Cleanup();
			_rewardAssetTracker = null;
		}
	}
}
