using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using Core.Meta.MainNavigation.Cosmetics;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.Enums.Cosmetic;
using Wizards.Unification.Models.Mercantile;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;

namespace ProfileUI;

public class AvatarSelectPanel : MonoBehaviour, ICosmeticSelector<AvatarSelection>
{
	public Action<string> OnDefaultSelected;

	[SerializeField]
	private AvatarSelection _avatarBustPrefab;

	[SerializeField]
	private Transform _avatarBustsParent;

	[SerializeField]
	private CustomButton _backButton;

	[SerializeField]
	private CustomButton _doneButton;

	[SerializeField]
	private GameObject _storeButton;

	[SerializeField]
	private CustomButton _confirmButton;

	[SerializeField]
	private Image avatarBodyImage;

	[SerializeField]
	private CustomButton _makeAccountDefaultButton;

	[SerializeField]
	private Localize _avatarNameText;

	[SerializeField]
	private Localize _avatarBioText;

	private bool _isDefaultSelector;

	private AssetLoader.AssetTracker<Sprite> _avatarBodyImageSpriteTracker;

	private AvatarSelection _currentAvatar;

	private AvatarSelection _accountDefault;

	private AvatarSelection _savedAvatar;

	private List<AvatarSelection> _avatarSelectionList = new List<AvatarSelection>();

	private string _currentSelectedAvatar;

	private string _defaultAvatar;

	private CosmeticsProvider _cosmetics;

	private AssetLookupSystem _assetLookupSystem;

	private AvatarCatalog _avatarCatalog;

	private Action<AvatarSelection> _avatarBust_OnClicked;

	private Action<AvatarSelection> _avatarBust_OnSelected;

	private Action _backButton_OnClicked;

	private FadeTextAlphaBasedOnScrollviewValue[] _fadeTexts;

	private bool _nullIsValid;

	private Action<Action> _storeAction;

	public void Initialize(AssetLookupSystem assetLookupSystem, CosmeticsProvider cosmetics, AvatarCatalog avatarCatalog, bool nullIsValid = false)
	{
		_assetLookupSystem = assetLookupSystem;
		_cosmetics = cosmetics;
		_avatarCatalog = avatarCatalog;
		_nullIsValid = nullIsValid;
		_confirmButton.OnClick.AddListener(DoneButton_OnClick);
		_makeAccountDefaultButton.OnClick.AddListener(SetDefaultAvatar);
		_backButton.OnClick.AddListener(BackButtonClicked);
	}

	private void Awake()
	{
		_fadeTexts = base.transform.GetComponentsInChildren<FadeTextAlphaBasedOnScrollviewValue>();
	}

	public void Display()
	{
		base.gameObject.SetActive(value: true);
		_backButton.gameObject.SetActive(_savedAvatar != null || _nullIsValid);
		_storeButton.SetActive(value: false);
		foreach (AvatarSelection avatarSelection in _avatarSelectionList)
		{
			if (avatarSelection != null)
			{
				avatarSelection.SetInitialState();
			}
		}
		if ((bool)_currentAvatar)
		{
			SetSelectedAvatar(_currentAvatar);
			return;
		}
		_doneButton.gameObject.SetActive(value: false);
		_storeButton.SetActive(value: false);
	}

	public void CreateAvatarBusts()
	{
		_currentAvatar = null;
		_avatarBustsParent.DestroyChildren();
		_avatarSelectionList.Clear();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (KeyValuePair<string, AvatarEntry> item in _avatarCatalog)
		{
			string key = item.Key;
			string listingId = item.Value.StoreItem?.PurchasingId ?? item.Value.StoreBundles?.FirstOrDefault()?.Id;
			AvatarEntry value = item.Value;
			bool flag = value.Source.HasFlag(AcquisitionFlags.DefaultLoginGrant);
			bool flag2 = _cosmetics.IsAvatarOwned(key);
			bool flag3 = value.StoreSection != EStoreSection.None;
			int siblingIndex;
			if (flag)
			{
				siblingIndex = num++;
			}
			else if (flag2)
			{
				siblingIndex = num + num2++;
			}
			else
			{
				if (!flag3)
				{
					continue;
				}
				siblingIndex = num + num2 + num3++;
			}
			CreateAvatarForSelectionData(key, value, flag2 || flag, siblingIndex, listingId);
		}
	}

	public void DestroyAvatarBusts()
	{
		_avatarBustsParent.DestroyChildren();
	}

	public void SetData(string currentSelectedAvatar, string defaultAvatar = "")
	{
		_currentSelectedAvatar = currentSelectedAvatar;
		if (string.IsNullOrEmpty(_currentSelectedAvatar))
		{
			_currentSelectedAvatar = defaultAvatar;
		}
		_defaultAvatar = defaultAvatar;
	}

	private void CreateAvatarForSelectionData(string avatarId, AvatarEntry avatarEntry, bool isOwned, int siblingIndex, string listingId)
	{
		AvatarSelection selection = UnityEngine.Object.Instantiate(_avatarBustPrefab, _avatarBustsParent);
		selection.Initialize(_assetLookupSystem);
		selection.SetAvatar(avatarId, isOwned, avatarEntry.StoreSection, listingId);
		selection.transform.SetSiblingIndex(siblingIndex);
		_avatarSelectionList.Add(selection);
		selection.Button.OnClick.AddListener(delegate
		{
			AvatarSelection_OnClick(selection);
		});
		selection.Button.OnMouseover.AddListener(delegate
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, base.gameObject);
		});
		if (avatarId == _currentSelectedAvatar)
		{
			SetSelectedAvatar(selection);
			_savedAvatar = selection;
		}
		if (_defaultAvatar == avatarId)
		{
			ShowDefaultAvatar(selection);
		}
	}

	private void AvatarSelection_OnClick(AvatarSelection selection)
	{
		if (_currentAvatar != selection || _savedAvatar == null)
		{
			SetSelectedAvatar(selection);
		}
		_avatarBust_OnClicked?.Invoke(selection);
		playAvatarSound(selection);
	}

	public void AvatarStoreButton_OnClick()
	{
		if (!(_currentAvatar != null))
		{
			return;
		}
		StoreTabType storeTab = StoreTabType.Cosmetics;
		switch (_currentAvatar.StoreSection)
		{
		case EStoreSection.Bundles:
			storeTab = StoreTabType.Bundles;
			break;
		case EStoreSection.Avatars:
			storeTab = StoreTabType.Cosmetics;
			break;
		case EStoreSection.ProgressionTracks:
			storeTab = StoreTabType.Featured;
			break;
		}
		if (_currentAvatar.ListingId != null)
		{
			Action obj = delegate
			{
				SceneLoader.GetSceneLoader().GoToStoreItem(_currentAvatar.ListingId, storeTab, "Go to avatar store item from profile popup");
			};
			_storeAction(obj);
		}
		else
		{
			Action obj2 = delegate
			{
				SceneLoader.GetSceneLoader().GoToStore(storeTab, "Go to avatar store from profile popup");
			};
			_storeAction(obj2);
		}
		Invoke("DelayClose", 1f);
	}

	private void playAvatarSound(AvatarSelection selection)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
		AudioManager.PlayAudio("stopallVO_walkers", base.gameObject);
		if (selection.VO != null)
		{
			AudioManager.PlayAudio(selection.VO.WwiseEventName, base.gameObject, 0.05f);
		}
	}

	private void DelayClose()
	{
		_backButton_OnClicked?.Invoke();
	}

	public void DoneButton_OnClick()
	{
		if (_currentAvatar != null)
		{
			_savedAvatar = _currentAvatar;
		}
		_currentSelectedAvatar = _currentAvatar.Id;
		_backButton_OnClicked?.Invoke();
		_avatarBust_OnSelected?.Invoke(_currentAvatar);
	}

	public void BackButtonClicked()
	{
		if (_currentAvatar != _savedAvatar)
		{
			SetSelectedAvatar(_savedAvatar);
		}
		_backButton_OnClicked?.Invoke();
	}

	private void SetDefaultAvatar()
	{
		ShowDefaultAvatar(_currentAvatar);
		_defaultAvatar = _currentAvatar.Id;
		UpdateButtonState(_currentAvatar);
		OnDefaultSelected?.Invoke(_currentAvatar.Id);
	}

	private void ShowDefaultAvatar(AvatarSelection selection)
	{
		if (!(selection == null))
		{
			if (_accountDefault != null)
			{
				_accountDefault.SetDefaultTrigger(on: false);
			}
			_accountDefault = selection;
			_accountDefault.SetDefaultTrigger(on: true);
		}
	}

	private void SetSelectedAvatar(AvatarSelection selection)
	{
		if (!(selection == null))
		{
			if (_currentAvatar != null)
			{
				_currentAvatar.SetToggleTrigger(on: false);
			}
			_currentAvatar = selection;
			selection.SetToggleTrigger(on: true);
			SetRightSideImage();
			_doneButton.Interactable = selection != _savedAvatar;
			_confirmButton.Interactable = selection != _savedAvatar;
			UpdateButtonState(selection);
		}
	}

	private void UpdateButtonState(AvatarSelection selection)
	{
		UpdateButtonState(selection.IsLocked(), selection.StoreSection != EStoreSection.None);
	}

	private void UpdateButtonState(bool selectionsIsLocked, bool selectionIsInStore)
	{
		_makeAccountDefaultButton.gameObject.SetActive(_isDefaultSelector && !selectionsIsLocked);
		_makeAccountDefaultButton.Interactable = _defaultAvatar != _currentAvatar.Id;
		_confirmButton.gameObject.SetActive(_isDefaultSelector && !selectionsIsLocked);
		_doneButton.gameObject.SetActive(!_isDefaultSelector && !selectionsIsLocked);
		_storeButton.SetActive(selectionsIsLocked && selectionIsInStore);
	}

	public void OnScrollValueChanged(Vector2 newScrollViewValues)
	{
		FadeTextAlphaBasedOnScrollviewValue[] fadeTexts = _fadeTexts;
		for (int i = 0; i < fadeTexts.Length; i++)
		{
			fadeTexts[i].ScrollViewValueChanged(newScrollViewValues);
		}
	}

	public void SetOnBustClicked(Action<AvatarSelection> avatarBust_OnClicked)
	{
		_avatarBust_OnClicked = avatarBust_OnClicked;
	}

	private void SetRightSideImage()
	{
		if (_avatarBodyImageSpriteTracker == null)
		{
			_avatarBodyImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("AvaterSelectBodyImageSprite");
		}
		string avatarFullImagePath = ProfileUtilities.GetAvatarFullImagePath(_assetLookupSystem, _currentAvatar.Id);
		LocalizedString localizedString = ProfileUtilities.GetAvatarLocKey(_currentAvatar.Id);
		LocalizedString localizedString2 = ProfileUtilities.GetAvatarBio(_currentAvatar.Id);
		AssetLoaderUtils.TrySetSprite(avatarBodyImage, _avatarBodyImageSpriteTracker, avatarFullImagePath);
		_avatarNameText.SetText(localizedString.mTerm);
		_avatarBioText.SetText(localizedString2.mTerm);
	}

	public void OnStoreClicked(Action<Action> storeAction)
	{
		_storeAction = storeAction;
	}

	public void SetCallbacks(Action<AvatarSelection> OnSelected, Action OnHide)
	{
		_avatarBust_OnSelected = OnSelected;
		_backButton_OnClicked = OnHide;
	}

	public void Open(bool isDefaultSelector)
	{
		AudioManager.PlayAudio("sfx_ui_main_card_cosmetic_picker_in", AudioManager.Default);
		CreateAvatarBusts();
		_isDefaultSelector = isDefaultSelector;
		Display();
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(avatarBodyImage, _avatarBodyImageSpriteTracker);
	}
}
