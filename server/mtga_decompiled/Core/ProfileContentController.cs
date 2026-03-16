using System;
using System.Collections;
using System.Collections.Generic;
using AssetLookupTree;
using Core.Code.Promises;
using Core.MainNavigation.RewardTrack;
using Core.Meta.MainNavigation;
using Core.Meta.MainNavigation.Cosmetics;
using Core.Meta.MainNavigation.Profile;
using Pooling;
using ProfileUI;
using SharedClientCore.SharedClientCore.Code.Providers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.Models.Network;
using Wizards.Arena.Promises;
using Wizards.MDN.DeckManager;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Models.ClientModels;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;

public class ProfileContentController : NavContentController
{
	private IAccountClient _accountClient;

	[SerializeField]
	private Image AvatarBodyImage;

	[SerializeField]
	private GameObject AvatarRoot;

	[SerializeField]
	private Localize AvatarNameText;

	[SerializeField]
	private Localize AvatarBioText;

	[SerializeField]
	private Animator _fullAvatarAnimator;

	[SerializeField]
	private Color _mythicOrangeColor = Color.white;

	[SerializeField]
	private TMP_Text UsernameText;

	private ProfileScreenModeEnum _profileScreenMode;

	private ProfileScreenModeEnum _previousProfileScreenMode;

	private ProfileScreenModeEnum _queuedScreenMode;

	private Promise<CombinedRankInfo> _combinedRankHandle;

	private AssetLoader.AssetTracker<Sprite> _avatarBodyImageSpriteTracker;

	[Header("Nested Prefabs")]
	[SerializeField]
	private SeasonRewardsPanel SeasonRewardsPanel;

	[SerializeField]
	private SeasonRewardsPanel SparkRankRewardsPanel;

	[SerializeField]
	private AvatarSelectPanel AvatarSelectPanel;

	[SerializeField]
	private ProfileDetailsPanel ProfileDetailsPanel;

	[SerializeField]
	private EmoteSelectionScreenView EmoteSelectPanel;

	[SerializeField]
	private SetCollectionScreenView SetCollectionPanel;

	private IBILogger _biLogger;

	private AssetLookupSystem _assetLookupSystem;

	private CosmeticsProvider _cosmeticsProvider;

	private StoreManager _store;

	private ProfileCompassGuide _compassGuide;

	[SerializeField]
	private CosmeticSelectorController _cosmeticSelectorController;

	[SerializeField]
	private DisplayItemAvatar _avatarDisplayItem;

	[SerializeField]
	private DisplayItemEmote _emoteDisplayItem;

	[SerializeField]
	private DisplayItemPet _petDisplayItem;

	[SerializeField]
	private DisplayItemSleeve _sleeveDisplayItem;

	[SerializeField]
	private DisplayItemTitle _titleDisplayItem;

	[SerializeField]
	private Transform _cosmeticSelectorsTransform;

	[SerializeField]
	private CustomButton _avatarButton;

	[SerializeField]
	private CustomButton _emoteButton;

	[SerializeField]
	private CustomButton _petButton;

	[SerializeField]
	private CustomButton _sleeveButton;

	[SerializeField]
	private CustomButton _titleButton;

	private IDeckSleeveProvider _sleeveProvider;

	public override NavContentType NavContentType => NavContentType.Profile;

	public override bool IsReadyToShow
	{
		get
		{
			if (_combinedRankHandle != null)
			{
				return _combinedRankHandle.IsDone;
			}
			return false;
		}
	}

	public void Initialize(SeasonAndRankDataProvider seasonDataProvider, IClientLocProvider localizationManager, IEmoteDataProvider emoteDataProvider, AssetLookupSystem assetLookupSystem, IAccountClient accountClient, CosmeticsProvider cosmetics, SetMasteryDataProvider masteryPassProvider, StoreManager store, IBILogger biLogger, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, CardMaterialBuilder cardMaterialBuilder, IUnityObjectPool unityObjectPool, ICardRolloverZoom cardRolloverZoom, IDeckSleeveProvider sleeveProvider, StoreManager storeManager, ISetMetadataProvider setMetadataProvider, IFrontDoorConnectionServiceWrapper frontDoorWrapper, ITitleCountManager titleCountManager)
	{
		_accountClient = accountClient;
		_assetLookupSystem = assetLookupSystem;
		_cosmeticsProvider = cosmetics;
		_store = store;
		_biLogger = biLogger;
		_compassGuide = Pantry.Get<WrapperCompass>().GetGuide<ProfileCompassGuide>();
		SetCollectionController setCollectionController = new SetCollectionController(cardDatabase, assetLookupSystem, setMetadataProvider, titleCountManager, WrapperController.Instance.InventoryManager.Cards);
		_sleeveProvider = sleeveProvider;
		ProfileDetailsPanel.Initialize(masteryPassProvider, localizationManager, seasonDataProvider, setCollectionController, cardDatabase, cardViewBuilder, cardMaterialBuilder, _assetLookupSystem, _accountClient.AccountInformation != null && _accountClient.AccountInformation.HasRole_InvitationalQualified(), setToModeSeasonRewards, SetMode, setMetadataProvider);
		SeasonRewardsPanel.Initialize(seasonDataProvider.SeasonInfo?.currentSeason, backClicked, assetLookupSystem, setToModeSeasonRewards);
		SparkRankRewardsPanel.InitializeWithoutSeason(backClicked, assetLookupSystem, setToModeSeasonRewards);
		SetCollectionPanel.Init(setCollectionController, setMetadataProvider, backClicked, _biLogger);
		_cosmeticSelectorController.Init(_cosmeticSelectorsTransform, localizationManager, cosmetics, _store.AvatarCatalog, _store.PetCatalog, assetLookupSystem, cardRolloverZoom, biLogger, cardDatabase, cardViewBuilder, emoteDataProvider, unityObjectPool, sleeveProvider, storeManager);
		_cosmeticSelectorController.InitAvatar(_avatarDisplayItem);
		_cosmeticSelectorController.SetOnAvatarSelected(avatarSelected);
		_avatarButton.OnClick.AddListener(delegate
		{
			SetMode(ProfileScreenModeEnum.AvatarSelect);
		});
		_cosmeticSelectorController.InitEmote(_emoteDisplayItem);
		_cosmeticSelectorController.SetOnEmoteSelected(onEmotesSelected);
		_emoteButton.OnClick.AddListener(delegate
		{
			SetMode(ProfileScreenModeEnum.EmoteSelect);
		});
		_cosmeticSelectorController.InitPet(_petDisplayItem);
		_cosmeticSelectorController.SetOnPetSelected(onPetSelected);
		_petButton.OnClick.AddListener(delegate
		{
			SetMode(ProfileScreenModeEnum.PetSelect);
		});
		_cosmeticSelectorController.InitSleeve(_sleeveDisplayItem);
		_cosmeticSelectorController.SetOnSleeveSelected(onSleeveSelected);
		_sleeveButton.OnClick.AddListener(delegate
		{
			SetMode(ProfileScreenModeEnum.CardBackSelect);
		});
		bool flag = IsTitlesEnabled(frontDoorWrapper);
		_titleButton.Interactable = flag;
		if (flag)
		{
			_cosmeticSelectorController.InitTitle(_titleDisplayItem);
			_titleButton.OnClick.AddListener(delegate
			{
				SetMode(ProfileScreenModeEnum.TitleSelect);
			});
			_titleButton.Interactable = true;
		}
		_cosmeticSelectorController.SetData(cosmetics._vanitySelections);
		setAvatarVisuals(cosmetics._vanitySelections.avatarSelection);
		void avatarSelected(AvatarSelection selection)
		{
			cosmetics.SetAvatarSelection(selection.Id);
			setAvatarVisuals(selection.Id);
		}
		void backClicked()
		{
			GoBackToPreviousMode();
		}
		void setToModeSeasonRewards(RankType rank)
		{
			SparkRankRewardsPanel.ShowLimitedRankDisplay(ProfileDetailsPanel.ShouldShowLimitedRank());
			SeasonRewardsPanel.SetRankType(rank);
			ToggleMode();
			SeasonRewardsPanel.RankTypeToggled((int)rank);
		}
	}

	public void ToggleMode()
	{
		SetMode(ProfileScreenModeEnum.SeasonRewards);
	}

	private bool IsTitlesEnabled(IFrontDoorConnectionServiceWrapper frontDoorWrapper)
	{
		Client_KillSwitchNotification killswitch = frontDoorWrapper.Killswitch;
		if (killswitch != null)
		{
			_ = killswitch.IsTitlesDisabled;
			if (0 == 0)
			{
				return !frontDoorWrapper.Killswitch.IsTitlesDisabled;
			}
		}
		return true;
	}

	private void setAvatarVisuals(string avatarId)
	{
		if (avatarId != null)
		{
			if (_avatarBodyImageSpriteTracker == null)
			{
				_avatarBodyImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("ProfileAvatarBodyImageSprite");
			}
			string avatarFullImagePath = ProfileUtilities.GetAvatarFullImagePath(_assetLookupSystem, avatarId);
			LocalizedString localizedString = ProfileUtilities.GetAvatarLocKey(avatarId);
			LocalizedString localizedString2 = ProfileUtilities.GetAvatarBio(avatarId);
			AssetLoaderUtils.TrySetSprite(AvatarBodyImage, _avatarBodyImageSpriteTracker, avatarFullImagePath);
			AvatarNameText.SetText(localizedString.mTerm);
			AvatarBioText.SetText(localizedString2.mTerm);
			AvatarRoot.gameObject.SetActive(value: false);
			AvatarRoot.gameObject.SetActive(value: true);
		}
	}

	public void SetFullAvatarFaded(bool doFade)
	{
		if (_fullAvatarAnimator.isActiveAndEnabled)
		{
			_fullAvatarAnimator.SetTrigger("FadeAvatar", doFade);
		}
	}

	public void GoBackToPreviousMode()
	{
		SetMode(_previousProfileScreenMode);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_back, base.gameObject);
	}

	public void GoToProfileDetails()
	{
		if (IsReadyToShow)
		{
			SetMode(ProfileScreenModeEnum.ProfileDetails);
		}
		else
		{
			_queuedScreenMode = ProfileScreenModeEnum.ProfileDetails;
		}
	}

	protected override void Start()
	{
		base.Start();
		if (_compassGuide != null)
		{
			SetProfileScreenMode(_compassGuide.ScreenMode, _compassGuide.RankType);
		}
	}

	public void SetProfileScreenMode(ProfileScreenModeEnum profileScreenMode, RankType rankType)
	{
		SeasonRewardsPanel.RankTypeToggled((int)rankType);
		SeasonRewardsPanel.SetRankType(rankType);
		if (IsReadyToShow)
		{
			SetMode(profileScreenMode);
		}
		else
		{
			_queuedScreenMode = profileScreenMode;
		}
	}

	private void SetMode(ProfileScreenModeEnum profileScreenMode)
	{
		base.gameObject.SetActive(value: true);
		if (_profileScreenMode != profileScreenMode)
		{
			_previousProfileScreenMode = _profileScreenMode;
		}
		_profileScreenMode = profileScreenMode;
		ProfileDetailsPanel.gameObject.UpdateActive(active: false);
		SeasonRewardsPanel.gameObject.UpdateActive(active: false);
		AvatarSelectPanel.gameObject.UpdateActive(active: false);
		EmoteSelectPanel.gameObject.UpdateActive(active: false);
		SetCollectionPanel.gameObject.UpdateActive(active: false);
		SparkRankRewardsPanel.gameObject.UpdateActive(active: false);
		AvatarRoot.SetActive(value: true);
		UsernameText.gameObject.SetActive(value: true);
		_cosmeticSelectorController.SetData(_cosmeticsProvider._vanitySelections);
		_cosmeticSelectorController.CloseAllCosmeticSelectors();
		setAvatarVisuals(_cosmeticsProvider._vanitySelections.avatarSelection);
		switch (_profileScreenMode)
		{
		case ProfileScreenModeEnum.AvatarSelect:
			ProfileDetailsPanel.Display();
			_cosmeticSelectorController.OpenCosmeticSelector(DisplayCosmeticsTypes.Avatar);
			SetFullAvatarFaded(doFade: false);
			break;
		case ProfileScreenModeEnum.ProfileDetails:
			ProfileDetailsPanel.Display();
			SetFullAvatarFaded(doFade: false);
			break;
		case ProfileScreenModeEnum.SeasonRewards:
			if (SeasonRewardsPanel.GetRankType() == RankType.Constructed && isSparkRank())
			{
				SparkRankRewardsPanel.DisplayAndRefreshConstructedRank();
			}
			else
			{
				SeasonRewardsPanel.Display(UsernameText.text);
			}
			SetFullAvatarFaded(doFade: true);
			break;
		case ProfileScreenModeEnum.EmoteSelect:
			ProfileDetailsPanel.Display();
			SetFullAvatarFaded(doFade: false);
			_cosmeticSelectorController.OpenCosmeticSelector(DisplayCosmeticsTypes.Emote);
			break;
		case ProfileScreenModeEnum.PetSelect:
			ProfileDetailsPanel.Display();
			SetFullAvatarFaded(doFade: false);
			_cosmeticSelectorController.OpenCosmeticSelector(DisplayCosmeticsTypes.Pet);
			break;
		case ProfileScreenModeEnum.CardBackSelect:
			ProfileDetailsPanel.Display();
			SetFullAvatarFaded(doFade: false);
			_cosmeticSelectorController.OpenCosmeticSelector(DisplayCosmeticsTypes.Sleeve);
			break;
		case ProfileScreenModeEnum.SetCollection:
			SetCollectionPanel.Display();
			SetFullAvatarFaded(doFade: false);
			break;
		case ProfileScreenModeEnum.TitleSelect:
			ProfileDetailsPanel.Display();
			SetFullAvatarFaded(doFade: false);
			_cosmeticSelectorController.OpenCosmeticSelector(DisplayCosmeticsTypes.Title);
			break;
		}
	}

	public bool isSparkRank()
	{
		return _combinedRankHandle.Result.constructedClass == RankingClassType.Spark;
	}

	public override void OnBeginOpen()
	{
		StartCoroutine(Coroutine_RefreshCombinedRank());
	}

	public override void OnFinishOpen()
	{
		base.OnFinishOpen();
		if (_combinedRankHandle.Successful)
		{
			Pantry.Get<IPlayerRankServiceWrapper>().CombinedRank = _combinedRankHandle.Result;
		}
		Activate(active: true);
	}

	private IEnumerator Coroutine_RefreshCombinedRank()
	{
		WrapperController.EnableLoadingIndicator(enabled: true);
		_combinedRankHandle = Pantry.Get<IPlayerRankServiceWrapper>().GetPlayerRankInfo();
		yield return new WaitUntil(() => _combinedRankHandle.IsDone);
		WrapperController.EnableLoadingIndicator(enabled: false);
	}

	public override void Activate(bool active)
	{
		if (active)
		{
			AccountInformation accountInformation = _accountClient.AccountInformation;
			if (accountInformation != null)
			{
				int num = accountInformation.DisplayName.LastIndexOf('#');
				if (num != -1)
				{
					UsernameText.text = accountInformation.DisplayName.Substring(0, num) + "<color=#696969>#" + accountInformation.DisplayName.Substring(num + 1) + "</color>";
				}
				else
				{
					UsernameText.text = accountInformation.DisplayName;
					Wizards.Models.ClientBusinessEvents.AccountError payload = new Wizards.Models.ClientBusinessEvents.AccountError
					{
						EventTime = DateTime.UtcNow,
						DisplayName = accountInformation.DisplayName,
						AccountId = accountInformation.PersonaID,
						Error = "Account name without #"
					};
					_biLogger.Send(ClientBusinessEventType.AccountError, payload);
				}
				UsernameText.color = (_accountClient.AccountInformation.HasRole_MythicOrange() ? _mythicOrangeColor : Color.white);
			}
			SetMode(_queuedScreenMode);
			_queuedScreenMode = ProfileScreenModeEnum.ProfileDetails;
		}
		else
		{
			AvatarSelectPanel.DestroyAvatarBusts();
		}
	}

	private void OnSkipOnboarding()
	{
		StartCoroutine(Coroutine_ReloadRank());
		IEnumerator Coroutine_ReloadRank()
		{
			yield return StartCoroutine(Coroutine_RefreshCombinedRank());
			if (_combinedRankHandle.Successful)
			{
				Pantry.Get<IPlayerRankServiceWrapper>().CombinedRank = _combinedRankHandle.Result;
				SetMode(ProfileScreenModeEnum.ProfileDetails);
			}
		}
	}

	private void onPetSelected(PetEntry PetSelected)
	{
		if (PetSelected == null)
		{
			_cosmeticsProvider.SetPetSelection(null, null);
		}
		else
		{
			_cosmeticsProvider.SetPetSelection(PetSelected.Name, PetSelected.Variant);
		}
	}

	private void onSleeveSelected(string sleeveName)
	{
		_cosmeticsProvider.SetCardbackSelection(sleeveName).ThenOnMainThread(delegate(Promise<PreferredCosmetics> p)
		{
			if (p.Successful)
			{
				StartCoroutine(_sleeveProvider.Coroutine_UpdateAllDecksWithDefaultSleeve());
			}
		});
	}

	private void onEmotesSelected(List<string> emotes)
	{
		_cosmeticsProvider.SetEmoteSelections(emotes);
	}

	private void OnPetPopUpHide()
	{
		SetMode(ProfileScreenModeEnum.ProfileDetails);
	}

	public override void OnNavBarScreenChange(Action screenChangeAction)
	{
		screenChangeAction?.Invoke();
	}

	public void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(AvatarBodyImage, _avatarBodyImageSpriteTracker);
	}

	public override void OnHandheldBackButton()
	{
		if (_profileScreenMode == ProfileScreenModeEnum.ProfileDetails)
		{
			base.OnHandheldBackButton();
		}
		else
		{
			GoBackToPreviousMode();
		}
	}
}
