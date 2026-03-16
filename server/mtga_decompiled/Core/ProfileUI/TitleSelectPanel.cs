using System;
using System.Collections.Generic;
using AssetLookupTree;
using Core.Code.Promises;
using Core.Meta.MainNavigation.Cosmetics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.Enums.Cosmetic;
using Wizards.Arena.Models.Network;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;

namespace ProfileUI;

public class TitleSelectPanel : PopupBase, ICosmeticSelector<string>
{
	[SerializeField]
	private CustomButton _favoriteButton;

	[SerializeField]
	private CustomButton _backButton;

	[SerializeField]
	private TitleListViewItem _titlePrefab;

	[SerializeField]
	private Transform _titlesParent;

	[SerializeField]
	private Image _avatarImage;

	[SerializeField]
	private TextMeshProUGUI _usernameText;

	[SerializeField]
	private Localize _titlePreviewText;

	private string _preferredTitleId;

	private TitleListViewItem _selectedTitle;

	private TitleListViewItem _noTitle;

	private AssetLookupSystem _assetLookupSystem;

	private AssetLoader.AssetTracker<Sprite> _avatarBodyImageSpriteTracker;

	private CosmeticsProvider CosmeticsProvider => Pantry.Get<CosmeticsProvider>();

	public event Action<string> OnSelected;

	protected override void Awake()
	{
		base.Awake();
		if (_favoriteButton != null)
		{
			_favoriteButton.OnClick.AddListener(FavoriteButton_OnClick);
			_favoriteButton.OnMouseover.AddListener(Button_OnMouseover);
		}
		if (_backButton != null)
		{
			_backButton.OnClick.AddListener(BackButton_OnClick);
			_backButton.OnMouseover.AddListener(Button_OnMouseover);
		}
		_titlesParent.DestroyChildren();
	}

	public void SetData(string preferredTitle, AssetLookupSystem assetLookupSystem)
	{
		_preferredTitleId = preferredTitle;
		_assetLookupSystem = assetLookupSystem;
		AccountInformation accountInformation = _accountClient.AccountInformation;
		if (accountInformation != null)
		{
			int num = accountInformation.DisplayName.LastIndexOf('#');
			string sourceText = ((num != -1) ? accountInformation.DisplayName.Substring(0, num) : accountInformation.DisplayName);
			_usernameText.SetText(sourceText);
		}
	}

	public void Open()
	{
		AudioManager.PlayAudio("sfx_ui_main_card_cosmetic_picker_in", AudioManager.Default);
		Activate(activate: true);
		ShowTitles();
		SetAvatar();
	}

	private void ShowTitles()
	{
		_titlesParent.DestroyChildren();
		List<TitleListViewItem> list = new List<TitleListViewItem>();
		TitleListViewItem titleListViewItem = UnityEngine.Object.Instantiate(_titlePrefab, _titlesParent);
		CosmeticTitleEntry titleData = new CosmeticTitleEntry
		{
			Id = "NoTitle",
			LocKey = string.Empty
		};
		titleListViewItem.Initialize(titleData, isOwned: true, _preferredTitleId == "NoTitle", ChangeCurrentSelection);
		_selectedTitle = titleListViewItem;
		_noTitle = titleListViewItem;
		list.Add(titleListViewItem);
		foreach (CosmeticTitleEntry title in CosmeticsProvider.AvailableTitles)
		{
			bool flag = CosmeticsProvider.PlayerOwnedTitles.Exists((CosmeticTitleEntry t) => t.Id == title.Id);
			if (flag || title.AcquisitionFlags != AcquisitionFlags.None)
			{
				TitleListViewItem titleListViewItem2 = UnityEngine.Object.Instantiate(_titlePrefab, _titlesParent);
				titleListViewItem2.Initialize(title, flag, title.Id == _preferredTitleId, ChangeCurrentSelection);
				titleListViewItem2.TitleData.Id = title.Id;
				if (_preferredTitleId == titleListViewItem2.TitleData.Id)
				{
					_selectedTitle = titleListViewItem2;
				}
				list.Add(titleListViewItem2);
			}
		}
		TitleListViewItemSorter comparer = new TitleListViewItemSorter();
		list.Sort(comparer);
		for (int num = 0; num < list.Count; num++)
		{
			list[num].transform.SetSiblingIndex(num);
		}
		UpdateUI();
	}

	private void SetAvatar()
	{
		if (_avatarBodyImageSpriteTracker == null)
		{
			_avatarBodyImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("AvaterSelectBodyImageSprite");
		}
		string avatarFullImagePath = ProfileUtilities.GetAvatarFullImagePath(_assetLookupSystem, CosmeticsProvider.PlayerAvatarSelection);
		AssetLoaderUtils.TrySetSprite(_avatarImage, _avatarBodyImageSpriteTracker, avatarFullImagePath);
	}

	private void UpdateUI()
	{
		_favoriteButton.Interactable = _preferredTitleId != _selectedTitle.TitleData.Id && _selectedTitle.IsOwned;
		if (_selectedTitle.TitleData.Id == "NoTitle")
		{
			_titlePreviewText.SetText("EMPTY");
		}
		else
		{
			_titlePreviewText.SetText(_selectedTitle.TitleData.LocKey, null, _selectedTitle.TitleData.LocKey);
		}
	}

	private void ChangeCurrentSelection(TitleListViewItem newSelection)
	{
		TitleListViewItem titleListViewItem2;
		TitleListViewItem titleListViewItem3;
		if (_selectedTitle != null)
		{
			if (newSelection == _selectedTitle)
			{
				if (newSelection == _noTitle)
				{
					return;
				}
				TitleListViewItem noTitle = _noTitle;
				TitleListViewItem titleListViewItem = newSelection;
				titleListViewItem2 = noTitle;
				titleListViewItem3 = titleListViewItem;
			}
			else
			{
				TitleListViewItem titleListViewItem = newSelection;
				TitleListViewItem selectedTitle = _selectedTitle;
				titleListViewItem2 = titleListViewItem;
				titleListViewItem3 = selectedTitle;
			}
		}
		else
		{
			titleListViewItem2 = newSelection;
			titleListViewItem3 = null;
		}
		_selectedTitle = titleListViewItem2;
		if (titleListViewItem3 != null)
		{
			titleListViewItem3.SetSelected(selected: false);
		}
		titleListViewItem2.SetSelected(selected: true);
		UpdateUI();
	}

	public void Close()
	{
		Activate(activate: false);
	}

	public override void OnEnter()
	{
		FavoriteButton_OnClick();
	}

	public override void OnEscape()
	{
		BackButton_OnClick();
	}

	private void FavoriteButton_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, base.gameObject);
		CosmeticsProvider.SetTitleSelection(_selectedTitle.TitleData.Id).ThenOnMainThread(delegate(Promise<PreferredCosmetics> p)
		{
			if (p.Successful)
			{
				_preferredTitleId = _selectedTitle.TitleData.Id;
				UpdateUI();
			}
		});
	}

	private void BackButton_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_cancel, base.gameObject);
		Activate(activate: false);
	}

	private void Button_OnMouseover()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, base.gameObject);
	}

	public void SetCallbacks(Action<string> onSelected, Action onHide)
	{
		this.OnSelected = onSelected;
		OnHide = onHide;
	}
}
