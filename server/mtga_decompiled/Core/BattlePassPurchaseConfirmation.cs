using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Store;
using AssetLookupTree.Rewards;
using Assets.Core.Shared.Code;
using Core.MainNavigation.RewardTrack;
using Core.Meta.MainNavigation.Store.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wizards.Unification.Models.Mercantile;
using Wizards.Unification.Models.Player;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class BattlePassPurchaseConfirmation : PopupBase
{
	private struct StoreListing
	{
		public StoreItemDisplay Display;

		public StoreItem Data;

		public Animator DisplayAnimator;
	}

	[Header("Normal Mastery")]
	[SerializeField]
	private GameObject MasteryPrefab;

	[SerializeField]
	private CustomButton _buttonMastery_Purchase;

	[SerializeField]
	private CustomButton _buttonMastery_Hovers;

	[SerializeField]
	private TMP_Text _rewardNoticeText;

	[SerializeField]
	private Animator _textAnimator;

	[SerializeField]
	private Transform _primaryListingAnchor;

	[Header("Upsell Mastery")]
	[FormerlySerializedAs("_buttonMasteryBoost_Purchase")]
	[SerializeField]
	private CustomButton _buttonMasteryUpsell_Purchase;

	[SerializeField]
	private CustomButton _buttonMasteryUpsell_Hovers;

	[FormerlySerializedAs("_boostTextAnimator")]
	[SerializeField]
	private Animator _textUpsellAnimator;

	[FormerlySerializedAs("_primaryListingAnchor")]
	[SerializeField]
	private Transform _upsellListingAnchor;

	[Header("Level Boost")]
	[SerializeField]
	private GameObject LevelUpPrefab;

	[SerializeField]
	private CustomButton _buttonLevelUp_Purchase;

	[FormerlySerializedAs("__textLevelUp_Purchase")]
	[SerializeField]
	private TMP_Text _text_LevelBoostButton;

	[SerializeField]
	private TMP_Text _text_LevelBoostTitle;

	[SerializeField]
	private TMP_Text _text_LevelBoostDetails;

	[SerializeField]
	private GameObject _upsellButton;

	[SerializeField]
	private GameObject _upsellPanel;

	[Header("Reward Visualization")]
	[FormerlySerializedAs("RewardContainer")]
	[SerializeField]
	private GameObject _rewardContainer;

	[SerializeField]
	private RewardIcon _rewardIcon;

	[SerializeField]
	private GameObject _level1Prefab;

	[Header("Etc")]
	[SerializeField]
	private CustomButton _button_DismissTop;

	[SerializeField]
	private CustomButton _button_DismissBottom;

	[SerializeField]
	private CustomButton _button_Back;

	[SerializeField]
	private GameObject _levelFlash;

	[SerializeField]
	private bool _purchaseThroughContainerClick;

	private SetMasteryDataProvider _masteryPassProvider;

	private int _upgradeCost = 500;

	private const int XP_REWARD_AMOUNT = 500;

	private const int UPSELL_HIDE_LEVEL = 91;

	private const int BP_LEVEL_XP = 1000;

	private const int BonusLevelsFromUpsell = 10;

	private bool _hasLevelsToBePurchased;

	private int _levelsToBePurchased;

	private int _maxLevels;

	private int _currentLevelIndex;

	private int levelsFromReward;

	private int levelOffset;

	private StoreListing _primaryListing;

	private StoreListing _upsellListing;

	private IBILogger _biLogger;

	private CardDatabase _cardDatabase;

	private CardMaterialBuilder _cardMaterialBuilder;

	private AssetLookupSystem _assetLookupSystem;

	private readonly AssetTracker _assetTracker = new AssetTracker();

	public void Init(IBILogger biLogger, CardDatabase cardDatabase, CardMaterialBuilder cardMaterialBuilder, AssetLookupSystem assetLookupSystem)
	{
		_biLogger = biLogger;
		_cardDatabase = cardDatabase;
		_cardMaterialBuilder = cardMaterialBuilder;
		_assetLookupSystem = assetLookupSystem;
	}

	public void Activate(SetMasteryDataProvider masteryPassProvider, bool isBoost = false, int levelsToBePurchased = 1)
	{
		base.Activate(activate: true);
		_masteryPassProvider = masteryPassProvider;
		_currentLevelIndex = _masteryPassProvider.GetCurrentLevelIndex(_masteryPassProvider.CurrentBpName);
		_maxLevels = _masteryPassProvider.GetMaxNonRepeatLevel(_masteryPassProvider.CurrentBpName);
		_hasLevelsToBePurchased = isBoost;
		_levelsToBePurchased = levelsToBePurchased;
		if (_hasLevelsToBePurchased)
		{
			StoreItem storeItem = WrapperController.Instance.Store.ProgressionTracks.First((StoreItem x) => x.SubType == "LevelUpgrade");
			Client_PurchaseOption client_PurchaseOption = storeItem.PurchaseOptions.First();
			_upgradeCost = client_PurchaseOption.Price;
			SetStoreListing(storeItem, null, isUpsell: false);
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_xp_boost_popup, base.gameObject);
		}
		else
		{
			bool flag = _maxLevels - _currentLevelIndex >= 10;
			CreateListingVisuals(flag);
			_upsellPanel.gameObject.UpdateActive(flag);
			_upsellButton.gameObject.UpdateActive(flag);
			_levelFlash.gameObject.UpdateActive(flag);
			StartCoroutine(coroutine_selectItemAfterPopup());
		}
		MasteryPrefab.SetActive(value: false);
		LevelUpPrefab.SetActive(value: false);
		_rewardContainer.SetActive(value: true);
		StartCoroutine(coroutine_SetUpVisuals());
	}

	private void CreateListingVisuals(bool includeUpsell)
	{
		foreach (StoreItem item in WrapperController.Instance.Store.ProgressionTracks.Where((StoreItem x) => x.SubType == "RewardTierUpgrade"))
		{
			bool isUpsell = false;
			Transform parent;
			if (item.Id.Contains("Upsell"))
			{
				if (!includeUpsell)
				{
					continue;
				}
				isUpsell = true;
				parent = _upsellListingAnchor;
			}
			else
			{
				parent = _primaryListingAnchor;
			}
			BundlePayload bundlePayload = StoreDisplayUtils.StorePayloadForFlavor<BundlePayload>(item.PrefabIdentifier, _assetLookupSystem);
			StoreItemDisplay storeItemDisplay = UnityEngine.Object.Instantiate(AssetLoader.AcquireAndTrackAsset(_assetTracker, "BundleWidget - " + item.Id, bundlePayload.StoreDataRef), parent);
			StoreDisplayUtils.UpdateCardViews(storeItemDisplay, item);
			SetStoreListing(item, storeItemDisplay, isUpsell);
		}
	}

	private void SetStoreListing(StoreItem storeItem, StoreItemDisplay storeItemDisplay, bool isUpsell)
	{
		string text = storeItem.PurchaseOptions.FirstOrDefault()?.Price.ToString();
		StoreListing storeListing = new StoreListing
		{
			Data = storeItem,
			Display = storeItemDisplay,
			DisplayAnimator = storeItemDisplay?.Animator
		};
		if (isUpsell)
		{
			_upsellListing = storeListing;
			_buttonMasteryUpsell_Purchase.SetText(text);
		}
		else
		{
			_primaryListing = storeListing;
			_buttonMastery_Purchase.SetText(text);
		}
	}

	private void ResetListingVisuals()
	{
		UnityEngine.Object.Destroy(_upsellListing.Display?.gameObject);
		UnityEngine.Object.Destroy(_primaryListing.Display?.gameObject);
		_upsellListing = default(StoreListing);
		_primaryListing = default(StoreListing);
	}

	protected override void Awake()
	{
		base.Awake();
		if (_purchaseThroughContainerClick)
		{
			_buttonMastery_Hovers.OnClick.AddListener(PurchaseMastery);
			_buttonMasteryUpsell_Hovers.OnClick.AddListener(PurchaseMasteryBoost);
		}
		else
		{
			_buttonMastery_Hovers.OnMouseover.AddListener(OnNormalHover);
			_buttonMasteryUpsell_Hovers.OnMouseover.AddListener(OnUpsellHover);
		}
		_button_DismissTop.OnClick.AddListener(OnCancelClicked);
		_button_DismissBottom.OnClick.AddListener(OnCancelClicked);
		_button_Back.OnClick.AddListener(OnCancelClicked);
		_buttonLevelUp_Purchase.OnClick.AddListener(PurchaseLevels);
		_buttonMasteryUpsell_Purchase.OnClick.AddListener(PurchaseMasteryBoost);
		_buttonMastery_Purchase.OnClick.AddListener(PurchaseMastery);
	}

	private IEnumerator coroutine_SetUpVisuals()
	{
		yield return new WaitForSeconds(0.5f);
		RecalculateVisuals();
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_whoosh_01, base.gameObject);
	}

	private IEnumerator coroutine_selectItemAfterPopup()
	{
		yield return new WaitForSeconds(0.5f);
		_textAnimator.SetBool("ON", value: true);
		_textUpsellAnimator.SetBool("ON", value: false);
		_textAnimator.SetBool("ON", value: true);
		_levelFlash.SetActive(value: false);
		_primaryListing.DisplayAnimator?.SetBool("Active", !_purchaseThroughContainerClick);
		_upsellListing.DisplayAnimator?.SetBool("Active", value: false);
	}

	private void OnDestroy()
	{
		if (_purchaseThroughContainerClick)
		{
			_buttonMastery_Hovers.OnClick.RemoveListener(PurchaseMastery);
			_buttonMasteryUpsell_Hovers.OnClick.RemoveListener(PurchaseMasteryBoost);
		}
		else
		{
			_buttonMastery_Hovers.OnMouseover.RemoveListener(OnNormalHover);
			_buttonMasteryUpsell_Hovers.OnMouseover.RemoveListener(OnUpsellHover);
		}
		_button_DismissTop.OnClick.RemoveListener(OnCancelClicked);
		_button_DismissBottom.OnClick.RemoveListener(OnCancelClicked);
		_button_Back.OnClick.RemoveListener(OnCancelClicked);
		_buttonLevelUp_Purchase.OnClick.RemoveListener(PurchaseLevels);
		_buttonMasteryUpsell_Purchase.OnClick.RemoveListener(PurchaseMasteryBoost);
		_buttonMastery_Purchase.OnClick.RemoveListener(PurchaseMastery);
	}

	private void OnUpsellHover()
	{
		_primaryListing.DisplayAnimator?.SetBool("Active", value: false);
		_textAnimator.SetBool("ON", value: false);
		_upsellListing.DisplayAnimator?.SetBool("Active", value: true);
		_textUpsellAnimator.SetBool("ON", value: true);
		_levelFlash.SetActive(value: true);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, base.gameObject);
	}

	private void OnNormalHover()
	{
		_primaryListing.DisplayAnimator?.SetBool("Active", value: true);
		_textAnimator.SetBool("ON", value: true);
		_upsellListing.DisplayAnimator?.SetBool("Active", value: false);
		_textUpsellAnimator.SetBool("ON", value: false);
		_levelFlash.SetActive(value: false);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, base.gameObject);
	}

	public void EventTrigger_onAdd()
	{
		if (_currentLevelIndex + (_levelsToBePurchased - levelOffset) + levelsFromReward < _maxLevels - 1)
		{
			_levelsToBePurchased++;
			RecalculateVisuals();
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
		}
		else
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_invalid, base.gameObject);
		}
	}

	public void EventTrigger_onSub()
	{
		if (_levelsToBePurchased - 1 > 0)
		{
			_levelsToBePurchased--;
			RecalculateVisuals();
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
		}
		else
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_invalid, base.gameObject);
		}
	}

	private void PurchaseMastery()
	{
		StoreItem item = _primaryListing.Data;
		Client_PurchaseOption purchaseOption = item.PurchaseOptions.First();
		if (HasFunds(item))
		{
			string localizedText = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Confirm_Purchase_Text");
			_masteryPassProvider.GetTrackExpirationTime(_masteryPassProvider.CurrentBpName);
			string details = ExpirationDetailsForTime(_masteryPassProvider, Languages.ActiveLocProvider);
			SystemMessageManager.Instance.ShowOkCancel(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Confirm_Purchase_Title"), localizedText, delegate
			{
				ExecutePurchase(item, purchaseOption.CurrencyType);
			}, null, details);
		}
		else
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_invalid, base.gameObject);
		}
		DeactivateSelf();
	}

	private void PurchaseMasteryBoost()
	{
		StoreItem item = _upsellListing.Data;
		Client_PurchaseOption purchaseOption = item.PurchaseOptions.First();
		if (HasFunds(item))
		{
			string details = ExpirationDetailsForTime(_masteryPassProvider, Languages.ActiveLocProvider);
			SystemMessageManager.Instance.ShowOkCancel(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Confirm_Purchase_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Confirm_Purchase_Text"), delegate
			{
				ExecutePurchase(item, purchaseOption.CurrencyType);
			}, null, details);
		}
		else
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_invalid, base.gameObject);
		}
		DeactivateSelf();
	}

	private void PurchaseLevels()
	{
		StoreItem item = _primaryListing.Data;
		Client_PurchaseOption purchaseOption = item.PurchaseOptions.First();
		string details = ExpirationDetailsForTime(_masteryPassProvider, Languages.ActiveLocProvider);
		if (HasFunds(item, _levelsToBePurchased - levelOffset))
		{
			SystemMessageManager.Instance.ShowOkCancel(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Confirm_Purchase_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Confirm_Purchase_Text"), delegate
			{
				ExecutePurchase(item, purchaseOption.CurrencyType, _levelsToBePurchased - levelOffset, xpGain: true);
			}, null, details);
		}
		DeactivateSelf();
	}

	private static string ExpirationDetailsForTime(SetMasteryDataProvider masteryDataProvider, IClientLocProvider locManager)
	{
		int daysBeforeWarning = 30;
		return ExpirationDetailsForTime(masteryDataProvider.GetTrackExpirationTime(masteryDataProvider.CurrentBpName), ServerGameTime.GameTime, daysBeforeWarning, locManager);
	}

	public static string ExpirationDetailsForTime(DateTime expirationTime, DateTime gameTime, int daysBeforeWarning, IClientLocProvider locManager)
	{
		string result = null;
		if (expirationTime != default(DateTime))
		{
			TimeSpan timeSpan = expirationTime - gameTime;
			if (timeSpan.Days <= daysBeforeWarning)
			{
				result = locManager.GetLocalizedText("MainNav/Store/Confirm_Purchase_Expiring_Text", ("days", timeSpan.Days.ToString()));
			}
		}
		return result;
	}

	private void OnPurchaseCompleted(StoreItem storeItem)
	{
		if (storeItem.StoreSection == EStoreSection.ProgressionTracks)
		{
			Pantry.Get<SetMasteryDataProvider>().OnTrackRewardTierUpdateReceived_AWS();
		}
	}

	private void ExecutePurchase(StoreItem item, Client_PurchaseCurrencyType currencyType, int quantity = 1, bool xpGain = false)
	{
		PAPA.StartGlobalCoroutine(WrapperController.Instance.Store.PurchaseItemYield(item, currencyType, OnPurchaseCompleted, quantity));
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_gems_payment, base.gameObject);
		if (xpGain)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_xp_boost_claim, base.gameObject);
		}
	}

	private bool HasFunds(StoreItem item, int quantity = 1)
	{
		InventoryManager inventoryManager = WrapperController.Instance.InventoryManager;
		Client_PurchaseOption client_PurchaseOption = item.PurchaseOptions.First();
		if (((client_PurchaseOption.CurrencyType == Client_PurchaseCurrencyType.Gem) ? inventoryManager.Inventory.gems : inventoryManager.Inventory.gold) < client_PurchaseOption.Price * quantity)
		{
			_biLogger.Send(ClientBusinessEventType.PurchaseFunnel, PurchaseFunnel.Create(ClientPurchaseFunnelContext.PlayerSuggestedForGemPurchaseRedirect));
			SystemMessageManager.Instance.ShowOkCancel(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Buy_Gems_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Insufficient_Gems_Text"), OnBuyGemsForPurchase, null);
			return false;
		}
		return true;
	}

	private void OnBuyGemsForPurchase()
	{
		_biLogger.Send(ClientBusinessEventType.PurchaseFunnel, PurchaseFunnel.Create(ClientPurchaseFunnelContext.PlayerRedirectedToGemPurchase));
		SceneLoader.GetSceneLoader().GoToStore(StoreTabType.Gems, "Buy Gems from Battlepass", forceReload: true);
	}

	private void RecalculateVisuals()
	{
		int num = PopulateRewardsToGain();
		levelsFromReward = 0;
		int num2 = _currentLevelIndex + _levelsToBePurchased;
		if (num != 0)
		{
			int currentXpProgress = _masteryPassProvider.GetCurrentXpProgress(_masteryPassProvider.CurrentBpName);
			int num3 = num * 500 + currentXpProgress;
			while (num3 >= 1000)
			{
				num3 -= 1000;
				levelsFromReward++;
			}
		}
		if (levelsFromReward > 0)
		{
			if (_maxLevels >= levelsFromReward + num2)
			{
				levelOffset = 0;
				num2 += levelsFromReward;
			}
			else
			{
				levelOffset = levelsFromReward;
			}
		}
		if (_hasLevelsToBePurchased)
		{
			MasteryPrefab.SetActive(value: false);
			LevelUpPrefab.SetActive(value: true);
			_text_LevelBoostTitle.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/BattlePass/Boost_Title", ("number", (_levelsToBePurchased - levelOffset).ToString()));
			string empty = string.Empty;
			int num4 = num2 + 1;
			empty = ((_levelsToBePurchased - levelOffset != 1) ? Languages.ActiveLocProvider.GetLocalizedText("MainNav/BattlePass/Boost_Details", ("numXP", ((_levelsToBePurchased - levelOffset) * 1000).ToString()), ("newLevel", num4.ToString())) : Languages.ActiveLocProvider.GetLocalizedText("MainNav/BattlePass/Boost_Details_Single", ("numXP", ((_levelsToBePurchased - levelOffset) * 1000).ToString()), ("newLevel", num4.ToString())));
			_text_LevelBoostDetails.text = empty;
			_text_LevelBoostButton.text = (_upgradeCost * (_levelsToBePurchased - levelOffset)).ToString();
			_rewardNoticeText.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/BattlePass/Boost_Footer");
		}
		else
		{
			MasteryPrefab.SetActive(value: true);
			if (_currentLevelIndex > 91)
			{
				_upsellButton.SetActive(value: false);
				_upsellPanel.SetActive(value: false);
			}
			LevelUpPrefab.SetActive(value: false);
			_rewardNoticeText.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/BattlePass/Mastery_Footer");
		}
	}

	private int PopulateRewardsToGain()
	{
		_rewardContainer.transform.DestroyChildren();
		List<ClientTrackRewardInfo> list = new List<ClientTrackRewardInfo>();
		List<ProgressionTrackLevel> levelTracks = _masteryPassProvider.GetLevelTracks(_masteryPassProvider.CurrentBpName);
		bool flag = _masteryPassProvider.DoesTrackHaveTier(_masteryPassProvider.CurrentBpName, 2);
		if (_hasLevelsToBePurchased)
		{
			int num = _currentLevelIndex + _levelsToBePurchased;
			for (int i = _currentLevelIndex; i < num; i++)
			{
				for (int j = 0; j < levelTracks[i].ServerRewardTiers.Count; j++)
				{
					if (j != 2 || flag)
					{
						ClientTrackRewardInfo item = levelTracks[i].ServerRewardTiers[j];
						list.Add(item);
					}
				}
			}
		}
		else
		{
			for (int k = 0; k < _currentLevelIndex && k < levelTracks.Count; k++)
			{
				list.Add(levelTracks[k].ServerRewardTiers[1]);
			}
			Image[] componentsInChildren = _level1Prefab.GetComponentsInChildren<Image>();
			foreach (Image image in componentsInChildren)
			{
				UnityEngine.Object.Instantiate(_rewardIcon, _rewardContainer.transform).SetSingleReward(image.sprite);
			}
		}
		int result = 0;
		if (list.Count > 0)
		{
			foreach (KeyValuePair<string, int> item2 in ParseChestData(list))
			{
				UnityEngine.Object.Instantiate(_rewardIcon, _rewardContainer.transform).SetReward(item2.Key, item2.Value);
				if (Path.GetFileNameWithoutExtension(item2.Key) == "ObjectiveIcon_MasteryXP")
				{
					result = item2.Value;
				}
			}
		}
		return result;
	}

	private Dictionary<string, int> ParseChestData(List<ClientTrackRewardInfo> rewardsToDisplay)
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.RewardTrack = _masteryPassProvider.CurrentBpName;
		RewardDisplayDataPayload payload = _assetLookupSystem.TreeLoader.LoadTree<RewardDisplayDataPayload>().GetPayload(_assetLookupSystem.Blackboard);
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		string text = payload?.GetRewardDisplayData()?.Thumbnail1Path;
		foreach (ClientTrackRewardInfo item in rewardsToDisplay)
		{
			ClientChestDescription chest = item.chest;
			int orbsAwarded = item.OrbsAwarded;
			RewardDisplayData rewardDisplayData = TempRewardTranslation.ChestDescriptionToDisplayData(chest, _cardDatabase.CardDataProvider, _cardMaterialBuilder);
			if (rewardDisplayData != null)
			{
				string[] obj = new string[3] { rewardDisplayData.Thumbnail1Path, rewardDisplayData.Thumbnail2Path, rewardDisplayData.Thumbnail3Path };
				int num = 0;
				string[] array = obj;
				foreach (string text2 in array)
				{
					num++;
					if (!string.IsNullOrEmpty(text2))
					{
						int value = 1;
						if (chest != null && chest.locParams.Count > 0)
						{
							chest.locParams.TryGetValue($"number{num}", out value);
						}
						else if (rewardDisplayData.Quantity > 1)
						{
							value = rewardDisplayData.Quantity;
						}
						if (!string.IsNullOrEmpty(chest.image1) && !string.IsNullOrEmpty(chest.image2) && chest.image1.Contains("Pack") && chest.image2.Contains("Pack"))
						{
							value = 1;
						}
						if (!string.IsNullOrEmpty(chest.image1) && !string.IsNullOrEmpty(chest.image2) && chest.image1.Contains("MasteryOrb_THB") && Path.GetFileNameWithoutExtension(text2) == chest.image2)
						{
							value = 1;
						}
						if (dictionary.ContainsKey(text2))
						{
							dictionary[text2] += value;
						}
						else
						{
							dictionary.Add(text2, value);
						}
					}
				}
			}
			else if (orbsAwarded > 0 && !string.IsNullOrEmpty(text) && !dictionary.TryAdd(text, orbsAwarded))
			{
				dictionary[text] += orbsAwarded;
			}
		}
		return dictionary;
	}

	private void OnCancelClicked()
	{
		DeactivateSelf();
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_cancel, base.gameObject);
	}

	public override void OnEnter()
	{
	}

	public override void OnEscape()
	{
		OnCancelClicked();
	}

	private void DeactivateSelf()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_xp_boost_dismiss, base.gameObject);
		_button_DismissTop.enabled = true;
		_button_DismissBottom.enabled = true;
		ResetListingVisuals();
		base.Activate(activate: false);
	}
}
