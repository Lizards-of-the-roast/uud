using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Code.PrizeWall;
using Core.MainNavigation.RewardTrack;
using Pooling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga;
using Wizards.Unification.Models.Mercantile;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class RewardTrackView : MonoBehaviour
{
	private class PageLevels
	{
		public int LevelStart;

		public List<ProgressionTrackLevel> Levels = new List<ProgressionTrackLevel>();

		public int LevelEnd => LevelStart + Levels.Count - 1;
	}

	private class ContainerItems
	{
		public GameObject TrackOrnament;

		public List<RewardTrackItemView> ItemInstances = new List<RewardTrackItemView>();

		public RewardTrackItemView ItemViewEndL;

		public RewardTrackItemView ItemViewEndR;
	}

	public string TrackName;

	public NotificationPopup Hanger;

	public CustomButton ClickShield;

	[NonSerialized]
	public MTGALocalizedString TrackLabel;

	[SerializeField]
	private int _targetItemsPerPage = 10;

	[SerializeField]
	private float _autoscalePadding = 35f;

	[SerializeField]
	private float _levelProgressSpeed = 2f;

	[SerializeField]
	private float _levelProgressEase = 2f;

	[SerializeField]
	private float _levelUpCenterIconGap = 0.09f;

	[SerializeField]
	private ObjectiveBubble _objectiveBubble;

	[SerializeField]
	private TooltipTrigger _tooltipTrigger;

	[SerializeField]
	private Image _backgroundImage;

	[SerializeField]
	private TooltipTrigger _petHitboxTooltipTrigger;

	[SerializeField]
	private Animator[] _itemContainers;

	[SerializeField]
	private RewardTrackItemView _itemViewEndLPrefab;

	[SerializeField]
	private RewardTrackItemView _itemViewPrefab;

	[SerializeField]
	private RewardTrackItemView _itemViewEndRPrefab;

	[SerializeField]
	private GameObject _trackOrnamentPrefab;

	[SerializeField]
	private Localize _titleLabel;

	[SerializeField]
	private Localize _expiredMessage;

	[SerializeField]
	private CustomButton _masteryTreeButton;

	[SerializeField]
	private Localize _masteryTreeLabel;

	[SerializeField]
	private CustomButton _previousTreeButton;

	[SerializeField]
	private Localize _previousTreeLabel;

	[SerializeField]
	private GameObject _masteryTreeButtonPip;

	[SerializeField]
	private Transform _petAnchor;

	[SerializeField]
	private CustomButton _petHitbox;

	private GameObject saveCurrentPetInstance;

	[Header("Purchasing (empty if non-purchasable)")]
	[SerializeField]
	private GameObject _purchaseCenter;

	[SerializeField]
	private CustomButton _purchaseButton;

	[SerializeField]
	private TextMeshProUGUI _purchaseButtonLabel;

	[SerializeField]
	private GameObject _purchasePremiumTierLabel;

	[SerializeField]
	private GameObject _purchaseLevelUpLabel;

	[SerializeField]
	private GameObject _showcasePetReward;

	[Header("Level 1 Premium Tile")]
	[SerializeField]
	private MTGALocalizedString _level1PopupTitle;

	[SerializeField]
	private MTGALocalizedString _level1PopupDescription;

	[SerializeField]
	private NotificationPopupReward _popupPremium3DObject;

	[SerializeField]
	private GameObject _level1CustomPrefab;

	private string _previousTrackName;

	private bool _initialized;

	private bool _canPurchase;

	private Coroutine _reloadCoroutine;

	private int _currentContainer;

	private ContainerItems[] _itemsByContainer;

	private List<ProgressionTrackLevel> _levels;

	private List<RewardDisplayData[]> _levelRewardData;

	private List<PageLevels> _pages;

	private static readonly int InLeft = Animator.StringToHash("InLeft");

	private static readonly int InRight = Animator.StringToHash("InRight");

	private static readonly int OutRight = Animator.StringToHash("OutRight");

	private static readonly int OutLeft = Animator.StringToHash("OutLeft");

	private Coroutine _pageOutCoroutine;

	private static readonly int PetState = Animator.StringToHash("InWrapper");

	private static readonly int PetClick = Animator.StringToHash("Wrapper_Click");

	private static readonly int PetHover = Animator.StringToHash("Wrapper_Hover");

	private ConnectionManager _connectionManager;

	private CardDatabase _cardDatabase;

	private CardMaterialBuilder _cardMaterialBuilder;

	private IUnityObjectPool _objectPool;

	private int _currentPage;

	private bool _firstTime;

	private SetMasteryDataProvider _masteryPassProvider => Pantry.Get<SetMasteryDataProvider>();

	public int CurrentPage
	{
		get
		{
			return _currentPage;
		}
		set
		{
			value = Mathf.Clamp(value, 0, PagesCount - 1);
			int currentContainer = (_currentContainer + 1) % _itemContainers.Length;
			if (_currentPage > value)
			{
				DisableCurrentContainerItemsHover();
				_pageOutCoroutine = StartCoroutine(Coroutine_PageOut(_itemContainers[_currentContainer], OutRight));
				_currentContainer = currentContainer;
				_itemContainers[_currentContainer].gameObject.UpdateActive(active: true);
				_itemContainers[_currentContainer].SetTrigger(InLeft);
			}
			else
			{
				if (_currentPage >= value)
				{
					return;
				}
				DisableCurrentContainerItemsHover();
				_pageOutCoroutine = StartCoroutine(Coroutine_PageOut(_itemContainers[_currentContainer], OutLeft));
				_currentContainer = currentContainer;
				_itemContainers[_currentContainer].gameObject.UpdateActive(active: true);
				_itemContainers[_currentContainer].SetTrigger(InRight);
			}
			_currentPage = value;
			UpdateCurrentPage();
			this.PageChange?.Invoke();
		}
	}

	public int PagesCount => _pages?.Count ?? 0;

	public event Action PageChange;

	public void InjectTrackData(RewardTrackData data)
	{
		TrackName = data.TrackName;
		_backgroundImage.sprite = data.BackgroundImage;
		_petHitboxTooltipTrigger.LocString = data.PetHitboxTooltipLocString;
		_showcasePetReward = data.ShowcasePetReward;
		_level1PopupTitle = data.Level1PopupTitle;
		_level1PopupDescription = data.Level1PopupDescription;
		_popupPremium3DObject = data.PopupPremium3DObject;
		_level1CustomPrefab = data.Level1CustomPrefab;
		UpdateLevel();
	}

	public void SetPreviousTrack(string previousTrackName)
	{
		_previousTrackName = previousTrackName;
		if (_masteryPassProvider.IsEnabled(_previousTrackName))
		{
			string localizedText = Languages.ActiveLocProvider.GetLocalizedText("MainNav/BattlePass/" + _previousTrackName);
			if (localizedText != null)
			{
				string key = (string.IsNullOrEmpty(_masteryPassProvider.PreviousPrizeWallId) ? "EPP/RewardWeb/SetXMasteryTree" : "EPP/RewardTrack/SetXSpendPrizeWallCurrency");
				_previousTreeLabel.SetText(key, new Dictionary<string, string> { { "setName", localizedText } });
				_previousTreeButton.transform.parent.gameObject.UpdateActive(active: true);
				return;
			}
		}
		_previousTreeButton.transform.parent.gameObject.UpdateActive(active: false);
	}

	private void SetCurrentButtonLoc(string locKey)
	{
		_masteryTreeLabel.SetText(locKey);
	}

	private IEnumerator Coroutine_PageOut(Animator container, int animId)
	{
		if (_pageOutCoroutine != null)
		{
			StopCoroutine(_pageOutCoroutine);
		}
		container.SetTrigger(animId);
		yield return null;
		while (container.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
		{
			yield return null;
		}
		container.gameObject.UpdateActive(active: false);
		_pageOutCoroutine = null;
	}

	public void SetPanelIntro(bool val)
	{
		_firstTime = val;
	}

	private void Awake()
	{
		if (_masteryTreeButton != null)
		{
			if (!string.IsNullOrEmpty(_masteryPassProvider.CurrentPrizeWallId))
			{
				SetCurrentButtonLoc("EPP/RewardTrack/SpendPrizeWallCurrency");
				_masteryTreeButton.OnClick.AddListener(delegate
				{
					DisplayPrizeWall(_masteryPassProvider.CurrentPrizeWallId);
				});
			}
			else
			{
				SetCurrentButtonLoc("EPP/RewardWeb/MasteryTree");
				_masteryTreeButton.OnClick.AddListener(DisplayRewardTree);
			}
		}
		if (_previousTreeButton != null)
		{
			if (!string.IsNullOrEmpty(_masteryPassProvider.PreviousPrizeWallId))
			{
				_previousTreeButton.OnClick.AddListener(delegate
				{
					DisplayPrizeWall(_masteryPassProvider.PreviousPrizeWallId);
				});
			}
			else
			{
				_previousTreeButton.OnClick.AddListener(DisplayPreviousRewardTree);
			}
		}
		if (_purchaseCenter != null)
		{
			_purchaseButton.OnClick.AddListener(OnPurchaseClick);
			_purchaseButton.OnMouseover.AddListener(OnHovers);
		}
		_connectionManager = Pantry.Get<ConnectionManager>();
		ConnectionManager connectionManager = _connectionManager;
		connectionManager.OnFdReconnected = (Action)Delegate.Combine(connectionManager.OnFdReconnected, new Action(OnFdReconnected));
		Languages.LanguageChangedSignal.Listeners += RefreshText;
	}

	private void OnEnable()
	{
		UpdateLevel();
		if (_purchaseCenter != null)
		{
			_purchaseCenter.UpdateActive(active: false);
			ReloadProductsAndProgressionData();
		}
		if (!(saveCurrentPetInstance != null))
		{
			return;
		}
		Animator componentInChildren = saveCurrentPetInstance.GetComponentInChildren<Animator>();
		if (componentInChildren != null)
		{
			if (componentInChildren.ContainsParameter(PetState))
			{
				componentInChildren.SetBool(PetState, value: true);
			}
			if (componentInChildren.ContainsParameter(PetHover))
			{
				componentInChildren.Play(PetHover);
			}
		}
	}

	private void OnDestroy()
	{
		ConnectionManager connectionManager = _connectionManager;
		connectionManager.OnFdReconnected = (Action)Delegate.Remove(connectionManager.OnFdReconnected, new Action(OnFdReconnected));
		Languages.LanguageChangedSignal.Listeners -= RefreshText;
		this.PageChange = null;
	}

	private void OnFdReconnected()
	{
		ReloadProductsAndProgressionData();
	}

	public void UpdateLevel()
	{
		if (_masteryTreeButtonPip != null)
		{
			bool active = _masteryPassProvider.ShouldShowSetMasterOrbSpendHeat(Pantry.Get<IAccountClient>());
			_masteryTreeButtonPip.UpdateActive(active);
		}
		StartCoroutine(AnimateLevel());
	}

	private IEnumerator AnimateLevel()
	{
		if (_masteryPassProvider != null)
		{
			yield return new WaitForEndOfFrame();
			_objectiveBubble.SetInactive(_masteryPassProvider.HasTrackExpired(TrackName));
			CurrentProgressionSummary currentProgressionSummary = _masteryPassProvider.GetCurrentProgressionSummary(TrackName, _cardDatabase, _cardMaterialBuilder);
			if (currentProgressionSummary.LevelInfo.IsProgressionComplete)
			{
				_objectiveBubble.SetUpAsCompletedFinalLevel();
				_objectiveBubble.SetUnOwnedRewardOverlay(active: false);
				_objectiveBubble.SetLocked(value: false);
			}
			else
			{
				_objectiveBubble.SetProgressText(currentProgressionSummary.ProgressText);
				_tooltipTrigger.TooltipData.Text = currentProgressionSummary.ProgressText;
				_objectiveBubble.SetReward(currentProgressionSummary.CurrentReward);
				_objectiveBubble.SetUnOwnedRewardOverlay(currentProgressionSummary.ShouldTease);
				_objectiveBubble.SetLocked(currentProgressionSummary.ShouldTease);
				_objectiveBubble.ActivatePremiumWreath(currentProgressionSummary.Tier > 0);
			}
			_objectiveBubble.SetSidebarVisible(visible: false);
			_objectiveBubble.SetFooterText("MainNav/General/Empty_String");
			_objectiveBubble.Reference_levelData = currentProgressionSummary.LevelInfo;
			_objectiveBubble.SetMasteryLevelLabel(currentProgressionSummary.LevelInfo);
			float startFill = 0f;
			float endFill = (float)currentProgressionSummary.LevelInfo.EXPProgressIfIsCurrent / (float)currentProgressionSummary.LevelInfo.ServerLevel.xpToComplete;
			_objectiveBubble.SetRadialFill(startFill * (1f - _levelUpCenterIconGap) + _levelUpCenterIconGap / 2f);
			float fill = startFill;
			while (fill < endFill)
			{
				fill += Time.deltaTime / _levelProgressSpeed;
				float num = Mathf.Clamp01((fill - startFill) / (endFill - startFill));
				num = ((!(num < 0.5f)) ? (1f - Mathf.Pow((1f - num) * 2f, _levelProgressEase) * 0.5f) : (Mathf.Pow(num * 2f, _levelProgressEase) * 0.5f));
				float num2 = num * (endFill - startFill) + startFill;
				_objectiveBubble.SetRadialFill(num2 * (1f - _levelUpCenterIconGap) + _levelUpCenterIconGap / 2f);
				yield return null;
			}
		}
	}

	public void InitializeRewardTrackView(CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, CardMaterialBuilder cardMaterialBuilder)
	{
		if (_initialized)
		{
			_firstTime = false;
			return;
		}
		_cardMaterialBuilder = cardMaterialBuilder;
		_cardDatabase = cardDatabase;
		_objectiveBubble.Init(cardDatabase, cardViewBuilder);
		_objectPool = Pantry.Get<IUnityObjectPool>();
		RefreshText();
		_levels = _masteryPassProvider.GetLevelTracks(TrackName);
		if (_levels == null || _levels.Count == 0)
		{
			return;
		}
		_initialized = true;
		_levelRewardData = new List<RewardDisplayData[]>();
		foreach (ProgressionTrackLevel level in _levels)
		{
			bool replaceXp = _masteryPassProvider.HasTrackExpired(TrackName);
			List<RewardDisplayData> rewardDisplayDataForLevel = _masteryPassProvider.GetRewardDisplayDataForLevel(TrackName, level, _cardDatabase, _cardMaterialBuilder, replaceXp);
			_levelRewardData.Add(rewardDisplayDataForLevel.ToArray());
		}
		int num = _levels.Count + 1;
		int num2 = num % _targetItemsPerPage;
		int num3 = 1;
		if (num > _targetItemsPerPage)
		{
			num3 = num / _targetItemsPerPage;
		}
		int[] array = new int[num3];
		for (int i = 0; i < num3; i++)
		{
			array[i] = _targetItemsPerPage;
		}
		for (int j = 0; j < num2; j++)
		{
			int num4 = num3 - 1 - j % num3;
			array[num4]++;
		}
		_pages = new List<PageLevels>();
		PageLevels pageLevels = null;
		foreach (ProgressionTrackLevel level2 in _levels)
		{
			if (pageLevels == null)
			{
				pageLevels = new PageLevels
				{
					LevelStart = level2.Index
				};
				pageLevels.Levels.Add(null);
				_pages.Add(pageLevels);
			}
			if (pageLevels.Levels.Count >= array[_pages.Count - 1])
			{
				pageLevels = new PageLevels
				{
					LevelStart = level2.Index + 1
				};
				_pages.Add(pageLevels);
			}
			pageLevels.Levels.Add(level2);
		}
		int num5 = _pages.Max((PageLevels p) => p.Levels.Count);
		_itemsByContainer = new ContainerItems[_itemContainers.Length];
		for (int num6 = 0; num6 < _itemsByContainer.Length; num6++)
		{
			ContainerItems containerItems = (_itemsByContainer[num6] = new ContainerItems());
			Animator animator = _itemContainers[num6];
			animator.transform.DestroyChildren();
			if (_trackOrnamentPrefab != null)
			{
				containerItems.TrackOrnament = UnityEngine.Object.Instantiate(_trackOrnamentPrefab, animator.transform);
			}
			containerItems.ItemViewEndL = UnityEngine.Object.Instantiate(_itemViewEndLPrefab, animator.transform);
			containerItems.ItemViewEndL.Click += delegate
			{
				CurrentPage--;
			};
			containerItems.ItemInstances = new List<RewardTrackItemView>();
			for (int num7 = 0; num7 < num5; num7++)
			{
				RewardTrackItemView rewardTrackItemView = UnityEngine.Object.Instantiate(_itemViewPrefab, animator.transform);
				rewardTrackItemView.Hanger = Hanger;
				if (ClickShield != null)
				{
					rewardTrackItemView.ClickShield = ClickShield;
				}
				rewardTrackItemView.MasteryPassProvider = _masteryPassProvider;
				rewardTrackItemView.Click += OnItemClick;
				containerItems.ItemInstances.Add(rewardTrackItemView);
			}
			containerItems.ItemViewEndR = UnityEngine.Object.Instantiate(_itemViewEndRPrefab, animator.transform);
			containerItems.ItemViewEndR.Click += delegate
			{
				CurrentPage++;
			};
		}
		int num8 = 0;
		if (_itemContainers.Length != 0)
		{
			num8 = (_currentContainer + 1) % _itemContainers.Length;
			_itemContainers[num8].gameObject.UpdateActive(active: false);
			_itemContainers[_currentContainer].gameObject.UpdateActive(active: true);
		}
		if (!(_showcasePetReward != null))
		{
			return;
		}
		GameObject gameObject = _objectPool.PopObject(_showcasePetReward, _petAnchor);
		gameObject.transform.ZeroOut();
		gameObject.transform.localRotation = Quaternion.identity;
		saveCurrentPetInstance = gameObject;
		Animator petAnimator = gameObject.GetComponentInChildren<Animator>();
		if (!(petAnimator != null) || !(_petHitbox != null))
		{
			return;
		}
		_petHitbox.OnClick.AddListener(delegate
		{
			if (petAnimator.ContainsParameter(PetState))
			{
				petAnimator.SetBool(PetState, value: true);
			}
			if (petAnimator.ContainsParameter(PetHover))
			{
				petAnimator.SetTrigger(PetClick);
			}
		});
		_petHitbox.OnMouseover.AddListener(delegate
		{
			if (petAnimator.ContainsParameter(PetState))
			{
				petAnimator.SetBool(PetState, value: true);
			}
			if (petAnimator.ContainsParameter(PetHover))
			{
				petAnimator.SetBool(PetHover, value: true);
			}
		});
		_petHitbox.OnMouseoff.AddListener(delegate
		{
			if (petAnimator.ContainsParameter(PetState))
			{
				petAnimator.SetBool(PetState, value: true);
			}
			if (petAnimator.ContainsParameter(PetHover))
			{
				petAnimator.SetBool(PetHover, value: false);
			}
		});
		if (petAnimator.ContainsParameter(PetState))
		{
			petAnimator.SetBool(PetState, value: true);
		}
		if (petAnimator.ContainsParameter(PetHover))
		{
			petAnimator.Play(PetHover);
		}
	}

	private void RefreshText()
	{
		TrackLabel = _masteryPassProvider.GetTrackTitle(TrackName);
		_titleLabel.SetText(TrackLabel);
	}

	public void DisableCurrentContainerItemsHover()
	{
		foreach (RewardTrackItemView itemInstance in _itemsByContainer[_currentContainer].ItemInstances)
		{
			itemInstance.SetHover(enable: false);
		}
	}

	public void UpdateCurrentPage(bool resetToDefault = false)
	{
		if (!_initialized)
		{
			return;
		}
		bool flag = _masteryPassProvider.PlayerHitPremiumRewardTier(TrackName);
		int curLevelIndex = _masteryPassProvider.GetCurrentLevel(TrackName).Index;
		bool showRenewal = _masteryPassProvider.DoesTrackHaveTier(TrackName, 2);
		if (resetToDefault)
		{
			_currentPage = _pages.FindIndex((PageLevels p) => curLevelIndex + 1 >= p.LevelStart && curLevelIndex + 1 <= p.LevelEnd);
			if (_currentPage == -1)
			{
				_currentPage = _pages.Count - 1;
			}
			this.PageChange?.Invoke();
		}
		PageLevels pageLevels = _pages[_currentPage];
		ContainerItems containerItems = _itemsByContainer[_currentContainer];
		if (containerItems.TrackOrnament != null)
		{
			containerItems.TrackOrnament.UpdateActive(_currentPage == 0);
		}
		containerItems.ItemViewEndL.gameObject.UpdateActive(_currentPage > 0);
		if (_currentPage > 0)
		{
			int num = pageLevels.LevelStart - 1;
			containerItems.ItemViewEndL.Completed = num <= curLevelIndex;
			containerItems.ItemViewEndL.IsCurrentLevel = num == curLevelIndex;
			containerItems.ItemViewEndL.IsNextLevel = num == curLevelIndex + 1;
			containerItems.ItemViewEndL.IsPremiumTierLocked = !flag;
			containerItems.ItemViewEndL.IsTentPoleItem = _levels[num].ServerLevel.isTentpole;
		}
		int num2;
		for (num2 = 0; num2 < pageLevels.Levels.Count; num2++)
		{
			RewardTrackItemView rewardTrackItemView = containerItems.ItemInstances[num2];
			int num3 = pageLevels.LevelStart + num2;
			rewardTrackItemView.gameObject.UpdateActive(num3 <= _levels.Count + 1);
			rewardTrackItemView.SetHover(num3 <= _levels.Count + 1);
			rewardTrackItemView.SetLevel(num3);
			rewardTrackItemView.Completed = num3 <= curLevelIndex;
			rewardTrackItemView.IsCurrentLevel = num3 == curLevelIndex;
			rewardTrackItemView.IsNextLevel = num3 == curLevelIndex + 1;
			rewardTrackItemView.IsPremiumTierLocked = !flag;
			rewardTrackItemView.IsTentPoleItem = num3 >= _levels.Count || (num3 > 0 && _levels[num3 - 1].ServerLevel.isTentpole);
			rewardTrackItemView.IsRepeatableLevel = num3 > 0 && _levels[num3 - 1].IsRepeatable;
			if (rewardTrackItemView.IsRepeatableLevel)
			{
				bool flag2 = _masteryPassProvider.PlayerHitMaxLevel(TrackName);
				RewardDisplayData[] array = _levelRewardData[num3 - 1];
				array[0] = null;
				if (array.Length > 1)
				{
					if (flag2)
					{
						array[1].DescriptionText = "MainNav/General/Empty_String";
						array[1].SecondaryText = "EPP/RewardTrack/Completed";
					}
					else if (flag)
					{
						MTGALocalizedString mTGALocalizedString = "MainNav/BattlePass/RepeatableLevel_Desc";
						mTGALocalizedString.Parameters = new Dictionary<string, string> { 
						{
							"number",
							_levels.Count.ToString()
						} };
						array[1].DescriptionText = mTGALocalizedString;
						array[1].SecondaryText = "MainNav/General/Empty_String";
					}
					else
					{
						array[1].DescriptionText = "MainNav/BattlePass/GainAccessByUnlockingPremium";
						MTGALocalizedString mTGALocalizedString2 = "MainNav/BattlePass/RepeatableLevel_Desc";
						mTGALocalizedString2.Parameters = new Dictionary<string, string> { 
						{
							"number",
							_levels.Count.ToString()
						} };
						array[1].SecondaryText = mTGALocalizedString2;
					}
					array[1].ProgressText = "MainNav/General/Empty_String";
				}
				rewardTrackItemView.SetRewards(_levelRewardData[num3 - 1], null, showRenewal, _firstTime);
				rewardTrackItemView.Completed = flag2;
			}
			else if (num3 > 0)
			{
				for (int num4 = 0; num4 < _levelRewardData[num3 - 1].Length; num4++)
				{
					RewardDisplayData rewardDisplayData = _levelRewardData[num3 - 1][num4];
					if (rewardDisplayData == null)
					{
						continue;
					}
					switch (num4)
					{
					case 0:
						if (num3 <= curLevelIndex)
						{
							rewardDisplayData.DescriptionText = "MainNav/General/Empty_String";
							rewardDisplayData.SecondaryText = "EPP/RewardTrack/Completed";
						}
						else
						{
							rewardDisplayData.DescriptionText = "MainNav/BattlePass/CompleteQuestsForXP";
							rewardDisplayData.SecondaryText = "MainNav/General/Empty_String";
						}
						break;
					case 1:
					{
						if (num3 <= curLevelIndex)
						{
							if (flag)
							{
								rewardDisplayData.DescriptionText = "MainNav/General/Empty_String";
								rewardDisplayData.SecondaryText = "EPP/RewardTrack/Completed";
							}
							else
							{
								rewardDisplayData.DescriptionText = "MainNav/BattlePass/LevelCompleted_Locked";
								rewardDisplayData.SecondaryText = "MainNav/General/Empty_String";
							}
							break;
						}
						string tentPoleName = GetTentPoleName(rewardDisplayData);
						if (string.IsNullOrEmpty(tentPoleName))
						{
							rewardDisplayData.DescriptionText = ((!flag) ? "MainNav/BattlePass/GainAccessByUnlockingPremium" : "MainNav/BattlePass/CompleteQuestsForXP");
						}
						else if (tentPoleName.Contains("Lv1"))
						{
							rewardDisplayData.DescriptionText = "MainNav/BattlePass/TentPoleDescription_LevelOne";
						}
						else
						{
							rewardDisplayData.DescriptionText = "MainNav/BattlePass/TentPoleDescription_LevelGreaterThanOne";
						}
						rewardDisplayData.SecondaryText = "MainNav/General/Empty_String";
						break;
					}
					case 2:
						if (num3 <= curLevelIndex)
						{
							rewardDisplayData.DescriptionText = "MainNav/General/Empty_String";
							rewardDisplayData.SecondaryText = "EPP/RewardTrack/Completed";
						}
						else
						{
							rewardDisplayData.DescriptionText = "MainNav/BattlePass/CompleteQuestsForXP";
							rewardDisplayData.SecondaryText = "MainNav/General/Empty_String";
						}
						break;
					}
					MTGALocalizedString mTGALocalizedString3 = "MainNav/General/Simple_Number";
					mTGALocalizedString3.Parameters = new Dictionary<string, string> { 
					{
						"number",
						(num3 + 1).ToString()
					} };
					rewardDisplayData.ProgressText = mTGALocalizedString3;
				}
				rewardTrackItemView.SetRewards(_levelRewardData[num3 - 1], null, showRenewal, _firstTime);
			}
			else
			{
				RewardDisplayData[] array2 = new RewardDisplayData[2]
				{
					null,
					new RewardDisplayData
					{
						OverridePopup3dObject = _popupPremium3DObject,
						MainText = _level1PopupTitle
					}
				};
				if (flag && num3 <= curLevelIndex)
				{
					array2[1].DescriptionText = "MainNav/General/Empty_String";
					array2[1].SecondaryText = "EPP/RewardTrack/Completed";
				}
				else
				{
					array2[1].DescriptionText = _level1PopupDescription;
					array2[1].SecondaryText = "MainNav/BattlePass/ClickForDetails";
				}
				MTGALocalizedString mTGALocalizedString4 = new MTGALocalizedString
				{
					Key = "MainNav/General/Simple_Number"
				};
				mTGALocalizedString4.Parameters = new Dictionary<string, string> { { "number", "1" } };
				array2[1].ProgressText = mTGALocalizedString4;
				rewardTrackItemView.SetRewards(array2, _level1CustomPrefab);
			}
		}
		for (; num2 < containerItems.ItemInstances.Count; num2++)
		{
			containerItems.ItemInstances[num2].gameObject.UpdateActive(active: false);
		}
		containerItems.ItemViewEndR.gameObject.UpdateActive(_currentPage < _pages.Count - 1);
		if (_currentPage < _pages.Count - 1)
		{
			int num5 = pageLevels.LevelEnd + 1;
			containerItems.ItemViewEndR.SetLevel(num5);
			containerItems.ItemViewEndR.Completed = num5 <= curLevelIndex;
			containerItems.ItemViewEndR.IsCurrentLevel = num5 == curLevelIndex;
			containerItems.ItemViewEndR.IsNextLevel = num5 == curLevelIndex + 1;
			containerItems.ItemViewEndR.IsPremiumTierLocked = !flag;
			containerItems.ItemViewEndR.IsTentPoleItem = _levels[num5].ServerLevel.isTentpole;
		}
		StartCoroutine(Coroutine_SizeItemsToFit());
	}

	private void ReloadProductsAndProgressionData()
	{
		if (base.gameObject.activeInHierarchy)
		{
			if (_reloadCoroutine != null)
			{
				StopCoroutine(_reloadCoroutine);
			}
			_reloadCoroutine = StartCoroutine(Coroutine_ReloadProductsAndProgressionData());
		}
	}

	private IEnumerator Coroutine_ReloadProductsAndProgressionData()
	{
		yield return _masteryPassProvider.Refresh();
		UpdatePurchaseOptions();
	}

	public void UpdatePurchaseOptions()
	{
		bool num = _masteryPassProvider.HasTrackExpired(TrackName);
		bool flag = _masteryPassProvider.HasTrackExpired(_previousTrackName);
		bool flag2 = _masteryPassProvider.IsEnabled(TrackName);
		bool flag3 = _masteryPassProvider.isWebEnabled(TrackName);
		bool flag4 = _masteryPassProvider.isWebEnabled(_previousTrackName);
		bool flag5 = _masteryPassProvider.PlayerHitPremiumRewardTier(TrackName);
		bool flag6 = _masteryPassProvider.PlayerHitMaxLevel(TrackName);
		bool flag7 = !num && flag2 && !WrapperController.Instance.Store.StoreStatus.DisabledTags.Contains(EProductTag.BattlePass) && WrapperController.Instance.Store.AllTrackContentEnabled();
		PrizeWallDataProvider prizeWallDataProvider = Pantry.Get<PrizeWallDataProvider>();
		bool flag8 = prizeWallDataProvider.GetPrizeWallById(_masteryPassProvider.GetPrizeWallForTrack(TrackName)) != null;
		bool flag9 = prizeWallDataProvider.GetPrizeWallById(_masteryPassProvider.GetPrizeWallForTrack(_previousTrackName)) != null;
		string purchaseType = (flag5 ? "LevelUpgrade" : "RewardTierUpgrade");
		List<List<Client_PurchaseOption>> source = (from o in WrapperController.Instance.Store.StoreListings.Values
			where o.SubType == purchaseType
			select o.PurchaseOptions).ToList();
		_canPurchase = source.Any() && flag7 && ((!flag6 && flag5) || !flag5);
		_purchaseCenter.UpdateActive(_canPurchase);
		if (_masteryTreeButton != null)
		{
			_masteryTreeButton.Interactable = flag3 || flag8;
		}
		if (_previousTreeButton != null)
		{
			_previousTreeButton.Interactable = flag4 || flag9;
		}
		if (_expiredMessage != null)
		{
			bool flag10 = flag3 || flag8;
			_expiredMessage.gameObject.UpdateActive(flag || !flag10);
			if (!flag10)
			{
				_expiredMessage.SetText("MainNav/BattlePass/SetExpired_OrbsDisabled");
			}
			else if (flag)
			{
				_expiredMessage.SetText("MainNav/BattlePass/SetExpired_OrbWarning");
			}
		}
		if (!source.Any())
		{
			return;
		}
		_purchasePremiumTierLabel.UpdateActive(!flag5);
		_purchaseLevelUpLabel.UpdateActive(flag5);
		_purchaseButton.gameObject.UpdateActive(!flag5);
		_purchaseButtonLabel.text = source.Min((List<Client_PurchaseOption> list) => list.Min((Client_PurchaseOption item) => item.Price)).ToString("N0");
	}

	private void OnItemClick(int clickedTier, int itemLevel)
	{
		if (_canPurchase)
		{
			ProgressionTrackLevel currentLevel = _masteryPassProvider.GetCurrentLevel(TrackName);
			bool flag = _masteryPassProvider.PlayerHitPremiumRewardTier(TrackName);
			int num = itemLevel - 1;
			if ((!flag || num <= 0 || !_levels[num].IsRepeatable) && (currentLevel.Index < itemLevel || !flag))
			{
				_masteryPassProvider.PurchaseTrackUpgrade(itemLevel - currentLevel.Index);
			}
		}
	}

	private IEnumerator Coroutine_SizeItemsToFit()
	{
		yield return null;
		RectTransform rectTransform = (RectTransform)_itemContainers[_currentContainer].transform;
		float a = ((RectTransform)rectTransform.parent).rect.width / (rectTransform.rect.width + _autoscalePadding * 2f);
		rectTransform.localScale = Vector3.one * Mathf.Min(a, 1f);
	}

	private void OnPurchaseClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, base.gameObject);
		_masteryPassProvider.PurchaseTrackUpgrade();
	}

	private void OnHovers()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover_big, base.gameObject);
	}

	private void DisplayRewardTree()
	{
		SceneLoader.GetSceneLoader().GoToRewardTreeScene(new RewardTreePageContext(TrackName, null, null, NavContentType.RewardTrack));
	}

	private void DisplayPrizeWall(string prizeWallId)
	{
		PrizeWallContext prizeWallContext = new PrizeWallContext(NavContentType.RewardTrack, null, new ProgressionTrackPageContext(TrackName, NavContentType.Home, NavContentType.Home));
		SceneLoader.GetSceneLoader().GoToPrizeWall(prizeWallId, prizeWallContext);
	}

	private void DisplayPreviousRewardTree()
	{
		SceneLoader.GetSceneLoader().GoToRewardTreeScene(new RewardTreePageContext(_previousTrackName, null, null, NavContentType.RewardTrack));
	}

	private string GetTentPoleName(RewardDisplayData displayData)
	{
		if (!string.IsNullOrEmpty(displayData.Thumbnail1Path) && displayData.Thumbnail1Path.Contains("TentPole"))
		{
			return Path.GetFileNameWithoutExtension(displayData.Thumbnail1Path);
		}
		if (!string.IsNullOrEmpty(displayData.Thumbnail2Path) && displayData.Thumbnail2Path.Contains("TentPole"))
		{
			return Path.GetFileNameWithoutExtension(displayData.Thumbnail2Path);
		}
		if (!string.IsNullOrEmpty(displayData.Thumbnail3Path) && displayData.Thumbnail3Path.Contains("TentPole"))
		{
			return Path.GetFileNameWithoutExtension(displayData.Thumbnail3Path);
		}
		return null;
	}
}
