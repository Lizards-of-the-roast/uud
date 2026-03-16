using System.Collections.Generic;
using System.Text.RegularExpressions;
using Core.MainNavigation.RewardTrack;
using GreClient.CardData;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class RewardIcon : MonoBehaviour
{
	[HideInInspector]
	public bool PopupEnabled = true;

	public Vector2 PopupOffset;

	public NotificationPopup Popup;

	[HideInInspector]
	public CustomButton ClickShield;

	[HideInInspector]
	public RewardTrackItemView RewardTrackItemView;

	[HideInInspector]
	public SetMasteryDataProvider MasteryPassProvider;

	[SerializeField]
	private Image _primaryImage;

	[SerializeField]
	private Image _secondaryImage;

	[SerializeField]
	private Image _tertiaryImage;

	private AssetLoader.AssetTracker<Sprite> _primaryImageSpriteTracker;

	private AssetLoader.AssetTracker<Sprite> _secondaryImageSpriteTracker;

	private AssetLoader.AssetTracker<Sprite> _tertiaryImageSpriteTracker;

	[SerializeField]
	private Localize _primaryImageText;

	[SerializeField]
	private GameObject _textShadow;

	[SerializeField]
	private int _tier;

	private MTGALocalizedString _popupRewardPrefaceText;

	private MTGALocalizedString _popupRewardTitleText;

	private MTGALocalizedString _popupDescriptionText;

	private MTGALocalizedString _popupDetailsText;

	private MTGALocalizedString _popupProgressText;

	private RewardDisplayData _popupRewardData;

	private NotificationPopup.PopupData _popupData;

	private bool _locked;

	private bool _isPopupActive;

	public bool IsPopupActive
	{
		get
		{
			return _isPopupActive;
		}
		protected set
		{
			_isPopupActive = value && PopupEnabled;
			Popup?.gameObject.UpdateActive(_isPopupActive);
		}
	}

	public void ActivatePopup()
	{
		if (PopupEnabled)
		{
			SetupPopup();
		}
	}

	private void SetupPopup()
	{
		if (!(Popup == null))
		{
			Popup.gameObject.UpdateActive(active: true);
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_quest_rollover_a, base.gameObject);
			RectTransform component = Popup.GetComponent<RectTransform>();
			RectTransform component2 = base.transform.parent.GetComponent<RectTransform>();
			component.localPosition = component.parent.InverseTransformPoint(component2.position + (Vector3)PopupOffset);
			InitPopupData();
			_popupData.ApplyData(Popup);
		}
	}

	private void InitPopupData()
	{
		if (_popupData == null)
		{
			_popupData = new NotificationPopup.PopupData();
			if (_popupRewardData != null)
			{
				_popupData.SetPopupDisplayData(_popupRewardData);
			}
			_popupData.HeaderString1 = new MTGALocalizedString
			{
				Key = ((!string.IsNullOrEmpty(_popupRewardPrefaceText.Key)) ? _popupRewardPrefaceText.Key : "MainNav/General/Empty_String"),
				Parameters = _popupRewardPrefaceText.Parameters
			};
			_popupData.HeaderString2 = new MTGALocalizedString
			{
				Key = ((!string.IsNullOrEmpty(_popupRewardTitleText.Key)) ? _popupRewardTitleText.Key : "MainNav/General/Empty_String"),
				Parameters = _popupRewardTitleText.Parameters
			};
			_popupData.DescriptionString = new MTGALocalizedString
			{
				Key = ((!string.IsNullOrEmpty(_popupDescriptionText.Key)) ? _popupDescriptionText.Key : "MainNav/General/Empty_String"),
				Parameters = _popupDescriptionText.Parameters
			};
			_popupData.FooterString = new MTGALocalizedString
			{
				Key = ((!string.IsNullOrEmpty(_popupDetailsText.Key)) ? _popupDetailsText.Key : "MainNav/General/Empty_String"),
				Parameters = _popupDetailsText.Parameters
			};
			_popupData.ProgressString = new MTGALocalizedString
			{
				Key = ((!string.IsNullOrEmpty(_popupProgressText.Key)) ? _popupProgressText.Key : "MainNav/General/Empty_String"),
				Parameters = _popupProgressText.Parameters
			};
			_popupData.IsLocked = _locked;
		}
	}

	public void DeactivatePopup()
	{
		Popup?.gameObject.UpdateActive(active: false);
	}

	public void ToggleMobilePopup()
	{
		if (IsPopupActive)
		{
			CloseMobilePopup();
			return;
		}
		IsPopupActive = true;
		if (ClickShield != null)
		{
			ClickShield?.gameObject.SetActive(value: true);
			ClickShield?.OnClick.AddListener(ToggleMobilePopup);
		}
		SetupPopup();
		UpdatePopupButton();
	}

	public void CloseMobilePopup()
	{
		IsPopupActive = false;
		if (ClickShield != null)
		{
			ClickShield?.gameObject.SetActive(value: false);
			ClickShield.OnClick.RemoveAllListeners();
		}
		DeactivatePopup();
	}

	private void UpdatePopupButton()
	{
		InitPopupData();
		_popupData.RefreshButtonActive = !RewardTrackItemView.Completed;
		_popupData.RefreshButtonOnClickListener = ButtonClick;
		UpdatePopupButtonText();
		_popupData.ApplyData(Popup);
	}

	private void UpdatePopupButtonText()
	{
		string text = ((!MasteryPassProvider.PlayerHitPremiumRewardTier(MasteryPassProvider.CurrentBpName)) ? "MainNav/BattlePass/MasteryPassUnlock" : "MainNav/BattlePass/LevelBoost");
		_popupData.RefreshButtonString = text;
	}

	public void ButtonClick()
	{
		RewardTrackItemView.Button_OnPointerClick(_tier);
		CloseMobilePopup();
	}

	public void SetReward(RewardDisplayData reward)
	{
		if (reward == null)
		{
			return;
		}
		if (reward.ReferenceID != null)
		{
			uint grpId;
			string styleName;
			if (Regex.Match(reward.ReferenceID, "cardbacks\\.", RegexOptions.IgnoreCase).Success)
			{
				string sleeveName = reward.GetSleeveName(Languages.ActiveLocProvider);
				if (!string.IsNullOrEmpty(sleeveName))
				{
					if (reward.MainText.Parameters == null)
					{
						reward.MainText.Parameters = new Dictionary<string, string>();
					}
					reward.MainText.Parameters["sleeveName"] = sleeveName;
				}
			}
			else if (RewardDisplayData.TryParseCard(reward.ReferenceID, out grpId, out styleName))
			{
				CardPrintingData cardPrintingById = WrapperController.Instance.CardDatabase.CardDataProvider.GetCardPrintingById(grpId);
				if (cardPrintingById != null)
				{
					if (reward.MainText.Parameters == null)
					{
						reward.MainText.Parameters = new Dictionary<string, string>();
					}
					reward.MainText.Parameters["cardName"] = CardUtilities.FormatComplexTitle(WrapperController.Instance.CardDatabase.GreLocProvider.GetLocalizedText(cardPrintingById.TitleId));
				}
			}
			else
			{
				string fullLocKey = EmoteUtils.GetFullLocKey(reward.ReferenceID, WrapperController.Instance.AssetLookupSystem);
				if (fullLocKey != null && !string.IsNullOrEmpty(fullLocKey))
				{
					reward.SecondaryText = fullLocKey;
				}
			}
		}
		SetReward(reward.Thumbnail1Path, reward.Quantity, reward.Thumbnail2Path, reward.Thumbnail3Path);
		SetPopupText("MainNav/EventRewards/Reward", reward.MainText, reward.DescriptionText, reward.SecondaryText, reward.ProgressText);
		_popupData = null;
		_popupRewardData = ((!string.IsNullOrEmpty(reward.Popup3dObjectPath) || reward.OverridePopup3dObject != null) ? reward : null);
	}

	public void SetSingleReward(Sprite icon1)
	{
		_primaryImageSpriteTracker?.Cleanup();
		_primaryImage.sprite = icon1;
		_primaryImage.SetNativeSize();
		_secondaryImage.gameObject.UpdateActive(active: false);
		_tertiaryImage.gameObject.UpdateActive(active: false);
	}

	public void SetReward(string icon1Path, int quantity = 1, string icon2Path = null, string icon3Path = null)
	{
		if (_primaryImageSpriteTracker == null)
		{
			_primaryImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("RewardIconPrimaryImageSprite");
		}
		AssetLoaderUtils.TrySetSprite(_primaryImage, _primaryImageSpriteTracker, icon1Path);
		_primaryImage.SetNativeSize();
		if (_secondaryImage != null)
		{
			if (_secondaryImageSpriteTracker == null)
			{
				_secondaryImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("RewardIconSecondaryImageSprite");
			}
			AssetLoaderUtils.TrySetSprite(_secondaryImage, _secondaryImageSpriteTracker, icon2Path);
			_secondaryImage.SetNativeSize();
			_secondaryImage.gameObject.UpdateActive(!string.IsNullOrEmpty(icon2Path));
		}
		if (_tertiaryImage != null)
		{
			if (_tertiaryImageSpriteTracker == null)
			{
				_tertiaryImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("RewardIconTertiaryImageSprite");
			}
			AssetLoaderUtils.TrySetSprite(_tertiaryImage, _tertiaryImageSpriteTracker, icon3Path);
			_tertiaryImage.SetNativeSize();
			_tertiaryImage.gameObject.UpdateActive(!string.IsNullOrEmpty(icon3Path));
		}
		if (quantity > 1)
		{
			_primaryImageText.SetText("MainNav/General/Simple_Number", new Dictionary<string, string> { 
			{
				"number",
				quantity.ToString()
			} });
			_primaryImageText.gameObject.UpdateActive(active: true);
			_textShadow.UpdateActive(active: true);
		}
		else
		{
			_primaryImageText.SetText(string.Empty);
			_primaryImageText.gameObject.UpdateActive(active: false);
			_textShadow.UpdateActive(active: false);
		}
	}

	public void SetPopupText(MTGALocalizedString rewardPreface, MTGALocalizedString rewardTitle = null, MTGALocalizedString description = null, MTGALocalizedString details = null, MTGALocalizedString progress = null)
	{
		_popupRewardPrefaceText = rewardPreface;
		_popupRewardTitleText = rewardTitle;
		_popupDescriptionText = description;
		_popupDetailsText = details;
		_popupProgressText = progress;
	}

	public void SetLocked(bool value)
	{
		_locked = value;
	}

	public void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(_primaryImage, _primaryImageSpriteTracker);
		AssetLoaderUtils.CleanupImage(_secondaryImage, _secondaryImageSpriteTracker);
		AssetLoaderUtils.CleanupImage(_tertiaryImage, _tertiaryImageSpriteTracker);
	}
}
