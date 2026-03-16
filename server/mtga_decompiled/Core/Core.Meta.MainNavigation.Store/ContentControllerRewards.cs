using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using Core.Code.Input;
using Core.MainNavigation.RewardTrack;
using Core.Rewards;
using MTGA.KeyboardManager;
using TMPro;
using UnityEngine;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Utils;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;

namespace Core.Meta.MainNavigation.Store;

public class ContentControllerRewards : MonoBehaviour, IKeyDownSubscriber, IKeySubscriber, IBackActionHandler
{
	private readonly struct RewardsPage : IEnumerable<RewardsPageItem>, IEnumerable
	{
		private readonly List<RewardsPageItem> _items;

		public readonly bool LastPage;

		public RewardsPage(List<RewardsPageItem> items, bool lastPage = false)
		{
			_items = items;
			LastPage = lastPage;
		}

		public IEnumerator<RewardsPageItem> GetEnumerator()
		{
			return _items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _items.GetEnumerator();
		}
	}

	private readonly struct RewardsPageItem
	{
		public readonly IEnumerator DisplayReward;

		public RewardsPageItem(IEnumerator displayReward)
		{
			DisplayReward = displayReward;
		}
	}

	[SerializeField]
	[HideInInspector]
	private PetReward _petReward;

	[SerializeField]
	[HideInInspector]
	private VoucherReward _voucherReward;

	[SerializeField]
	[HideInInspector]
	private GoldReward _goldReward;

	[SerializeField]
	[HideInInspector]
	private GemReward _gemReward;

	[SerializeField]
	[HideInInspector]
	private EventTokenReward _eventTokenReward;

	[SerializeField]
	[HideInInspector]
	private EventTicketReward _eventTicketReward;

	[SerializeField]
	[HideInInspector]
	private XPReward _xpReward;

	[SerializeField]
	[HideInInspector]
	private MythicQualifierReward _mythicQualifierReward;

	[SerializeField]
	[HideInInspector]
	private PackReward _packReward;

	[SerializeField]
	[HideInInspector]
	private DeckBoxReward _deckBoxReward;

	[SerializeField]
	[HideInInspector]
	protected CardReward _cardReward;

	[SerializeField]
	[HideInInspector]
	protected CardRewardWithBonus _cardRewardWithBonus;

	[SerializeField]
	[HideInInspector]
	private StyleReward _styleReward;

	[SerializeField]
	[HideInInspector]
	private StyleFlavorAddon _styleFlavorAddon;

	[SerializeField]
	[HideInInspector]
	private SleeveReward _sleeveReward;

	[SerializeField]
	[HideInInspector]
	private AvatarReward _avatarReward;

	[SerializeField]
	[HideInInspector]
	private EmoteReward _emoteReward;

	[SerializeField]
	[HideInInspector]
	private TitleReward _titleReward;

	[SerializeField]
	[HideInInspector]
	private OrbReward _eppOrbReward;

	[SerializeField]
	[HideInInspector]
	private BpOrbReward _bpOrbReward;

	[SerializeField]
	[HideInInspector]
	private PrizeWallTokenReward _prizeWallTokenReward;

	[SerializeField]
	private RewardDisplay _cashTokenSmallPrefab;

	[SerializeField]
	private RewardDisplay _cashTokenLargePrefab;

	[SerializeField]
	private CompleteSetReward _completeSetReward;

	private IRewardBase[] _allRewards;

	[SerializeField]
	protected GameObject _container;

	[SerializeField]
	protected Transform _rewardParent;

	[SerializeField]
	protected TMP_Text _rewardsTitleText;

	[SerializeField]
	private TMP_Text _rewardsSubtitleText;

	[SerializeField]
	private TMP_Text _rewardsClaimButtonText;

	[SerializeField]
	private TMP_Text _endOfSeasonClaimButtonText;

	[SerializeField]
	private TMP_Text _endOfSeasonPlayButtonText;

	[SerializeField]
	public BoosterMetaCardHolder _cardHolder;

	[SerializeField]
	public BoosterMetaCardView _cardPrefab;

	[SerializeField]
	public float _sequenceDelay = 0.15f;

	[SerializeField]
	public float _preDisplayDelay = 0.23f;

	[SerializeField]
	private Animator _endOfSeasonAnimator;

	[SerializeField]
	private Animator _rewardsAnimator;

	[SerializeField]
	private GameObject _endOfSeasonGameObject;

	[SerializeField]
	private SeasonEndRankDisplay _endSeasonRankDisplayConstructed;

	[SerializeField]
	private SeasonEndRankDisplay _endSeasonRankDisplayLimited;

	[SerializeField]
	private GameObject _sparkTierRank;

	[SerializeField]
	private CustomButton _standardClaimButton;

	[SerializeField]
	private CustomButton _endOfSeasonClaimButton;

	[SerializeField]
	private GameObject _endOfSeasonPlayButton;

	private bool _visible;

	private KeyboardManager _keyboardManager;

	private IActionSystem _actionSystem;

	protected bool SubPageDisplayed;

	private bool _stillDisplayingSubpagesForRewardItem;

	private bool _stillDisplayingPagesForRewardItem;

	private Coroutine _claimRewardsCoroutine;

	private Coroutine _seasonRewardCoroutine;

	private SeasonPayoutData _endSeasonData;

	private SeasonEndState _endOfSeasonDisplayState;

	private SeasonEndState _lastEndOfSeasonDisplayState;

	private SetMasteryDataProvider _masteryPassProvider;

	private ICustomTokenProvider _customTokenProvider;

	public static readonly int QuantityUpdate = Animator.StringToHash("QuantityUpdate");

	private static readonly int ReIntro = Animator.StringToHash("reIntro");

	private static readonly int Dissolve = Animator.StringToHash("Dissolve");

	private static readonly int Intro = Animator.StringToHash("Intro");

	private static readonly int Outro = Animator.StringToHash("Outro");

	private Action _onRewardsWillClose;

	private int _stuckOnClaimClickedCounter;

	public bool Visible
	{
		get
		{
			return _visible;
		}
		protected set
		{
			_container.gameObject.UpdateActive(value);
			if (_visible == value)
			{
				return;
			}
			_visible = value;
			if (_visible)
			{
				_actionSystem.PushFocus(this);
				return;
			}
			_actionSystem.PopFocus(this);
			if (_sparkTierRank != null)
			{
				_sparkTierRank.SetActive(value: false);
			}
		}
	}

	private int AddingRewards { get; set; }

	public Transform RewardParent => _rewardParent;

	public AssetLookupSystem AssetLookupSystem { get; private set; }

	public ICardRolloverZoom ZoomHandler { get; private set; }

	public virtual PriorityLevelEnum Priority => PriorityLevelEnum.Wrapper_PopUps;

	public CardCollection CardHolderCollection { get; private set; }

	public CardDatabase CardDatabase { get; private set; }

	public CardViewBuilder CardViewBuilder { get; private set; }

	public bool AutoFlipping { get; set; }

	public event Action OnRewardsDisplayed;

	public event Action OnRewardsClosed;

	public void Clear(bool closeRewards = true)
	{
		if (!Visible)
		{
			return;
		}
		IRewardBase[] allRewards = _allRewards;
		for (int i = 0; i < allRewards.Length; i++)
		{
			allRewards[i].ClearInstances();
		}
		CardHolderCollection = new CardCollection(_cardHolder.CardDatabase);
		ZoomHandler.Close();
		if (closeRewards)
		{
			_stillDisplayingPagesForRewardItem = false;
			AutoFlipping = false;
			if (!IsHandlingEndOfSeasonDisplay())
			{
				Visible = false;
			}
			this.OnRewardsClosed?.Invoke();
			if (!IsHandlingEndOfSeasonDisplay())
			{
				Resources.UnloadUnusedAssets();
			}
		}
	}

	private void OnDestroy()
	{
		_onRewardsWillClose = null;
		this.OnRewardsDisplayed = null;
		this.OnRewardsClosed = null;
	}

	public void Init(SetMasteryDataProvider masteryPassProvider, AssetLookupSystem assetLookupSystem, ICardRolloverZoom zoomHandler, KeyboardManager keyboardManager, IActionSystem actionSystem, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		ZoomHandler = zoomHandler;
		ZoomHandler.IsActive = true;
		_cardHolder.EnsureInit(cardDatabase, cardViewBuilder);
		_cardHolder.RolloverZoomView = ZoomHandler;
		_cardHolder.ShowHighlight = (MetaCardView cardView) => false;
		_masteryPassProvider = masteryPassProvider;
		AssetLookupSystem = assetLookupSystem;
		_keyboardManager = keyboardManager;
		_actionSystem = actionSystem;
		CardDatabase = cardDatabase;
		CardViewBuilder = cardViewBuilder;
		CardHolderCollection = new CardCollection(cardDatabase);
		_allRewards = new IRewardBase[22]
		{
			_petReward, _voucherReward, _goldReward, _gemReward, _eventTokenReward, _eventTicketReward, _xpReward, _mythicQualifierReward, _sleeveReward, _avatarReward,
			_emoteReward, _packReward, _deckBoxReward, _completeSetReward, _cardRewardWithBonus, _cardReward, _styleReward, _styleFlavorAddon, _eppOrbReward, _bpOrbReward,
			_prizeWallTokenReward, _titleReward
		};
		ICardlikeReward[] array = new ICardlikeReward[2] { _cardReward, _cardRewardWithBonus };
		ICardlikeReward[] array2 = array;
		foreach (ICardlikeReward obj in array2)
		{
			obj.CardHolder = _cardHolder;
			obj.CardPrefab = _cardPrefab;
			obj.CardHolderCollection = CardHolderCollection;
			obj.CardDatabase = CardDatabase;
			obj.CardViewBuilder = CardViewBuilder;
			obj.CardlikeRewards = array;
		}
		_styleReward.StyleFlavorAddon = _styleFlavorAddon;
		_styleReward.GetUniqueId = (ArtSkin s) => s.artId + s.ccv;
		_sleeveReward.GetUniqueId = (string s) => s;
		_avatarReward.GetUniqueId = (string a) => a;
		_emoteReward.GetUniqueId = (string e) => e;
		_petReward.GetUniqueId = (PetLevel l) => l.PetName + l.VariantId + l.Level;
		_titleReward.GetUniqueId = (string t) => t;
		Visible = false;
	}

	private Coroutine StartCoroutineSafely(IEnumerator coroutine)
	{
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
		return StartCoroutine(coroutine);
	}

	private bool InTheMiddleOfSomething(bool fromEndOfSeasonRewards = false)
	{
		if (!_stillDisplayingPagesForRewardItem && AddingRewards <= 0)
		{
			if (IsHandlingEndOfSeasonDisplay())
			{
				return !fromEndOfSeasonRewards;
			}
			return false;
		}
		return true;
	}

	public Coroutine AddMythicQualifierBadgeCoroutine()
	{
		return StartCoroutineSafely(AddMythicQualifierBadge());
	}

	private IEnumerator AddMythicQualifierBadge()
	{
		yield return new WaitUntil(() => !InTheMiddleOfSomething());
		AddingRewards++;
		_mythicQualifierReward.ToAddCount = 1;
		AddingRewards--;
	}

	public Coroutine AddRewardedOrbsCoroutine(List<OrbInventoryChange> orbChanges)
	{
		return StartCoroutineSafely(AddRewardedOrbs(orbChanges));
	}

	private IEnumerator AddRewardedOrbs(List<OrbInventoryChange> orbChanges)
	{
		yield return new WaitUntil(() => !InTheMiddleOfSomething());
		AddingRewards++;
		foreach (OrbInventoryChange orbChange in orbChanges)
		{
			_bpOrbReward.ToAdd.Enqueue(orbChange);
		}
		AddingRewards--;
	}

	public Coroutine AddAndDisplayRewardsCoroutine(OneOrMany<ClientInventoryUpdateReportItem> update, string title, string claimButtonText, string subtitle = null)
	{
		return StartCoroutineSafely(AddAndDisplayRewards(update, title, claimButtonText, subtitle));
	}

	private IEnumerator AddAndDisplayRewards(OneOrMany<ClientInventoryUpdateReportItem> updateMessages, string title, string claimButtonText, string subtitle = null)
	{
		ClientInventoryUpdateReportItem[] array = updateMessages.ToArray();
		foreach (ClientInventoryUpdateReportItem updateMessage in array)
		{
			yield return AddInventoryUpdateMessageCoroutine(updateMessage);
		}
		yield return new WaitForSeconds(_preDisplayDelay);
		if (subtitle != null)
		{
			_rewardsSubtitleText.text = subtitle;
		}
		yield return DisplayRewards(title, claimButtonText);
	}

	private IEnumerator AddInventoryUpdateMessageCoroutine(ClientInventoryUpdateReportItem updateMessage, bool fromEndOfSeasonRewards = false)
	{
		yield return new WaitUntil(() => !InTheMiddleOfSomething(fromEndOfSeasonRewards));
		AddingRewards++;
		if (updateMessage?.delta != null)
		{
			PrecalculatedRewardUpdateInfo cache = PrecalculatedRewardUpdateInfo.Create(CardDatabase, _completeSetReward.setsOfInterest, updateMessage);
			IRewardBase[] allRewards = _allRewards;
			for (int num = 0; num < allRewards.Length; num++)
			{
				allRewards[num].AddFromInventoryUpdate(updateMessage, cache);
			}
			AddingRewards--;
		}
	}

	protected virtual IEnumerator DisplayRewards(string title, string claimButtonText, bool fromEndOfSeasonRewards = false)
	{
		yield return new WaitUntil(() => !InTheMiddleOfSomething(fromEndOfSeasonRewards));
		if (!HaveAnyRewardsToShow())
		{
			this.OnRewardsDisplayed?.Invoke();
			_onRewardsWillClose?.Invoke();
			_onRewardsWillClose = null;
			yield return null;
			yield break;
		}
		if (!Visible)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_reward_reveal, base.gameObject);
			Visible = true;
		}
		this.OnRewardsDisplayed?.Invoke();
		_standardClaimButton.gameObject.UpdateActive(active: true);
		_endOfSeasonClaimButton.gameObject.UpdateActive(active: false);
		_endOfSeasonPlayButton.UpdateActive(active: false);
		if (!IsHandlingEndOfSeasonDisplay())
		{
			_endOfSeasonGameObject.UpdateActive(active: false);
		}
		if (title != null)
		{
			_rewardsTitleText.text = title;
		}
		_rewardsSubtitleText.text = "";
		_stillDisplayingSubpagesForRewardItem = true;
		_stillDisplayingPagesForRewardItem = true;
		yield return RevealRewards(claimButtonText);
		_stillDisplayingSubpagesForRewardItem = false;
	}

	protected bool HaveAnyRewardsToShow()
	{
		return _allRewards.Any((IRewardBase _) => _.ToAddCount > 0);
	}

	private bool IsHandlingEndOfSeasonDisplay()
	{
		if (_endOfSeasonDisplayState == SeasonEndState.None)
		{
			return _seasonRewardCoroutine != null;
		}
		return true;
	}

	public Coroutine DisplayEndOfSeasonCoroutine(SeasonPayoutData endSeasonData)
	{
		return StartCoroutineSafely(DisplayEndOfSeason(endSeasonData));
	}

	private IEnumerator DisplayEndOfSeason(SeasonPayoutData endSeasonData)
	{
		yield return new WaitUntil(() => !InTheMiddleOfSomething());
		if (endSeasonData.constructedReset.oldClass == RankingClassType.Spark)
		{
			_onRewardsWillClose?.Invoke();
			yield break;
		}
		Visible = true;
		_endOfSeasonGameObject.SetActive(value: true);
		_endSeasonData = endSeasonData;
		SetSeasonEndState(SeasonEndState.OldRankDisplay);
	}

	public Coroutine DisplaySparkRankUnlockCoroutine()
	{
		return StartCoroutineSafely(DisplaySparkTierUnlock());
	}

	private IEnumerator DisplaySparkTierUnlock()
	{
		yield return new WaitUntil(() => !InTheMiddleOfSomething());
		Visible = true;
		_sparkTierRank.SetActive(value: true);
	}

	private IEnumerator DisplayLastSeasonRank()
	{
		_endOfSeasonClaimButton.gameObject.SetActive(value: false);
		_endOfSeasonPlayButton.SetActive(value: false);
		_rewardsTitleText.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/RewardsTitleSeasonRankings");
		_rewardsSubtitleText.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/RewardsSubtitleFinalResults");
		_endOfSeasonClaimButtonText.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/RewardsClaimButtonClaimRewards");
		_rewardsAnimator.SetTrigger(ReIntro, value: true);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_season_end_banners, base.gameObject);
		_endSeasonRankDisplayConstructed.SetRank(_endSeasonData.constructedReset.oldClass, _endSeasonData.constructedReset.oldLevel, _endSeasonData.constructedReset.oldStep, isConstructed: true, SeasonUtilities.GetSeasonDisplayName(_endSeasonData.oldConstructedOrdinal), AssetLookupSystem);
		if (_endSeasonData.limitedReset != null)
		{
			_endSeasonRankDisplayLimited.SetRank(_endSeasonData.limitedReset.oldClass, _endSeasonData.limitedReset.oldLevel, _endSeasonData.limitedReset.oldStep, isConstructed: false, SeasonUtilities.GetSeasonDisplayName(_endSeasonData.oldLimitedOrdinal), AssetLookupSystem);
		}
		_standardClaimButton.gameObject.SetActive(value: false);
		_endOfSeasonClaimButton.gameObject.SetActive(value: true);
		_endOfSeasonPlayButton.SetActive(value: false);
		yield return new WaitForSeconds(1f);
		_seasonRewardCoroutine = null;
	}

	private IEnumerator DisplayLastSeasonRewardsCoroutine()
	{
		_endSeasonRankDisplayConstructed.gameObject.SetActive(value: false);
		_endSeasonRankDisplayLimited.gameObject.SetActive(value: false);
		_endOfSeasonClaimButton.gameObject.SetActive(value: false);
		_endOfSeasonPlayButton.SetActive(value: false);
		AudioManager.PlayAudio("sfx_ui_boost_pack_release", base.gameObject);
		AudioManager.PlayAudio("sfx_ui_main_quest_claim_reward", base.gameObject);
		AudioManager.PlayAudio("sfx_ui_boost_card_flip_common", base.gameObject);
		_endOfSeasonAnimator.SetTrigger(Dissolve, value: true);
		_rewardsTitleText.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/SeasonRewardsTitle");
		_rewardsSubtitleText.text = "";
		_rewardsAnimator.SetTrigger(ReIntro, value: true);
		if (_endSeasonData != null)
		{
			if (_endSeasonData.limitedDelta != null)
			{
				foreach (ClientInventoryUpdateReportItem limitedDeltum in _endSeasonData.limitedDelta)
				{
					yield return AddInventoryUpdateMessageCoroutine(limitedDeltum, fromEndOfSeasonRewards: true);
				}
			}
			if (_endSeasonData.constructedDelta != null)
			{
				foreach (ClientInventoryUpdateReportItem constructedDeltum in _endSeasonData.constructedDelta)
				{
					yield return AddInventoryUpdateMessageCoroutine(constructedDeltum, fromEndOfSeasonRewards: true);
				}
			}
			string localizedText = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/EventRewards/ClaimPrizeButton");
			yield return DisplayRewards(null, localizedText, fromEndOfSeasonRewards: true);
		}
		_seasonRewardCoroutine = null;
	}

	private IEnumerator DisplayNextSeasonRanks()
	{
		if (_lastEndOfSeasonDisplayState == SeasonEndState.OldRankDisplay)
		{
			_endOfSeasonClaimButton.gameObject.SetActive(value: false);
			_endOfSeasonPlayButton.SetActive(value: false);
			_endOfSeasonAnimator.SetTrigger(Dissolve, value: true);
		}
		_standardClaimButton.gameObject.SetActive(value: false);
		_rewardsTitleText.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/RewardsTitleNewSeason");
		_rewardsSubtitleText.text = SeasonUtilities.GetSeasonDisplayName(_endSeasonData.currentSeasonOrdinal);
		_endOfSeasonPlayButtonText.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/RewardsClaimButtonPlay");
		_rewardsAnimator.SetTrigger(ReIntro, value: true);
		_endSeasonRankDisplayConstructed.gameObject.SetActive(value: true);
		_endSeasonRankDisplayConstructed.SetRank(_endSeasonData.constructedReset.newClass, _endSeasonData.constructedReset.newLevel, _endSeasonData.constructedReset.newStep, isConstructed: true, Languages.ActiveLocProvider.GetLocalizedText("MainNav/Season/PlacementLabel"), AssetLookupSystem);
		bool flag = _endSeasonData.limitedReset != null;
		_endSeasonRankDisplayLimited.gameObject.SetActive(flag);
		if (flag)
		{
			_endSeasonRankDisplayLimited.SetRank(_endSeasonData.limitedReset.newClass, _endSeasonData.limitedReset.newLevel, _endSeasonData.limitedReset.newStep, isConstructed: false, Languages.ActiveLocProvider.GetLocalizedText("MainNav/Season/PlacementLabel"), AssetLookupSystem);
		}
		_endOfSeasonAnimator.SetTrigger(Intro, value: true);
		_endOfSeasonPlayButton.SetActive(value: true);
		yield return new WaitForSeconds(1f);
		_seasonRewardCoroutine = null;
	}

	private IEnumerator DisplaySeasonOutro()
	{
		_endOfSeasonAnimator.SetTrigger(Outro, value: true);
		yield return new WaitForSeconds(0.25f);
		_standardClaimButton.gameObject.SetActive(value: false);
		_endOfSeasonClaimButton.gameObject.SetActive(value: false);
		Visible = false;
		_seasonRewardCoroutine = null;
		_onRewardsWillClose?.Invoke();
		_onRewardsWillClose = null;
	}

	private void SetSeasonEndState(SeasonEndState newEndOfSeasonState)
	{
		_lastEndOfSeasonDisplayState = _endOfSeasonDisplayState;
		IEnumerator enumerator;
		switch (newEndOfSeasonState)
		{
		case SeasonEndState.OldRankDisplay:
			enumerator = DisplayLastSeasonRank();
			break;
		case SeasonEndState.RewardsDisplay:
			enumerator = DisplayLastSeasonRewardsCoroutine();
			break;
		case SeasonEndState.NewRankDisplay:
			enumerator = DisplayNextSeasonRanks();
			break;
		case SeasonEndState.None:
			if (IsHandlingEndOfSeasonDisplay())
			{
				enumerator = DisplaySeasonOutro();
				break;
			}
			goto default;
		default:
			enumerator = null;
			break;
		}
		IEnumerator enumerator2 = enumerator;
		if (enumerator2 != null)
		{
			_seasonRewardCoroutine = StartCoroutineSafely(enumerator2);
		}
		_endOfSeasonDisplayState = newEndOfSeasonState;
	}

	private void HandleEndOfSeasonClicked()
	{
		SeasonEndState? seasonEndState = null;
		switch (_endOfSeasonDisplayState)
		{
		case SeasonEndState.OldRankDisplay:
			seasonEndState = ((_endSeasonData.constructedDelta == null && _endSeasonData.limitedDelta == null) ? SeasonEndState.NewRankDisplay : SeasonEndState.RewardsDisplay);
			break;
		case SeasonEndState.RewardsDisplay:
			Clear();
			seasonEndState = SeasonEndState.NewRankDisplay;
			break;
		case SeasonEndState.NewRankDisplay:
			seasonEndState = SeasonEndState.None;
			break;
		}
		if (seasonEndState.HasValue)
		{
			SetSeasonEndState(seasonEndState.Value);
		}
	}

	public bool OnRewardBubbleTicked(RewardObjectiveContext objectiveCircleContext, Action onModalClicked = null)
	{
		bool flag = false;
		List<ClientInventoryUpdateReportItem> list = new List<ClientInventoryUpdateReportItem>();
		if (objectiveCircleContext.ClientInventoryUpdateReportItem != null)
		{
			list.AddRange(objectiveCircleContext.ClientInventoryUpdateReportItem);
		}
		List<ClientInventoryUpdateReportItem> ts = list.Where((ClientInventoryUpdateReportItem _) => !_.IsEmpty()).ToList();
		if (ts.Count > 0)
		{
			flag = true;
			RegisterRewardWillCloseCallback(onModalClicked);
			AddAndDisplayRewardsCoroutine(ts, Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/Rewards_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/EventRewards/ClaimPrizeButton"));
		}
		if (objectiveCircleContext.contextString.Contains(MasteryPassConstants.MASTERY_CONTEXT_BP))
		{
			List<OrbInventoryChange> list2 = _masteryPassProvider.PopRewardedOrbsToDisplay(_masteryPassProvider.CurrentBpName);
			bool flag2 = list2.Any();
			flag = flag || flag2;
			if (flag2)
			{
				RegisterRewardWillCloseCallback(onModalClicked);
				AddRewardedOrbsCoroutine(list2);
				AddAndDisplayRewardsCoroutine(Array.Empty<ClientInventoryUpdateReportItem>(), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/Rewards_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/EventRewards/ClaimPrizeButton"));
			}
		}
		return flag;
	}

	public void RegisterRewardWillCloseCallback(Action onRewardsWillClose)
	{
		_onRewardsWillClose = (Action)Delegate.Remove(_onRewardsWillClose, onRewardsWillClose);
		_onRewardsWillClose = (Action)Delegate.Combine(_onRewardsWillClose, onRewardsWillClose);
	}

	public void UnregisterRewardsWillCloseCallback(Action onRewardsWillClose)
	{
		_onRewardsWillClose = (Action)Delegate.Remove(_onRewardsWillClose, onRewardsWillClose);
	}

	public void RegisterRewardClosedCallback(Action onRewardsClosed)
	{
		OnRewardsClosed -= onRewardsClosed;
		OnRewardsClosed += onRewardsClosed;
	}

	public void UnregisterRewardsClosedCallback(Action onRewardsClosed)
	{
		OnRewardsClosed -= onRewardsClosed;
	}

	private IEnumerable<RewardsPage> PlanQueuedRewards(IRewardBase[] rewardItems)
	{
		float rewardsScreenWidth = _rewardParent.GetComponent<RectTransform>().rect.width;
		int num = 0;
		float num2 = 0f;
		List<RewardsPageItem> list = null;
		bool autoFlippingCards = rewardItems.OfType<ICardlikeReward>().Sum((ICardlikeReward _) => _.ToAddCount) > 3;
		foreach (IRewardBase rewardItem in rewardItems)
		{
			if (rewardItem.InstancesCount == 0 && rewardItem.ToAddCount == 0)
			{
				continue;
			}
			float rewardMinWidth = rewardItem.GetWidth(AssetLookupSystem);
			IEnumerable<Func<RewardDisplayContext, IEnumerator>> enumerable = rewardItem.DisplayRewards(this);
			foreach (Func<RewardDisplayContext, IEnumerator> visibleReward in enumerable)
			{
				if (list != null && list.Any() && num2 + rewardMinWidth > rewardsScreenWidth)
				{
					yield return new RewardsPage(list);
					list = new List<RewardsPageItem>();
					num2 = 0f;
					num = 0;
				}
				RewardDisplayContext arg = new RewardDisplayContext(num, autoFlippingCards);
				RewardsPageItem item = new RewardsPageItem(visibleReward(arg));
				(list ?? (list = new List<RewardsPageItem>())).Add(item);
				num2 += rewardMinWidth;
				num++;
			}
			rewardItem.ClearAdded();
		}
		if (list != null && list.Count > 0)
		{
			yield return new RewardsPage(list, lastPage: true);
		}
	}

	private IEnumerator RevealRewards(string claimButtonText)
	{
		yield return null;
		_rewardsClaimButtonText.text = claimButtonText;
		_sleeveReward.ClaimButtonText = claimButtonText;
		List<RewardsPage> plannedRewardPages = PlanQueuedRewards(_allRewards).ToList();
		yield return DisplayPlannedRewardPages(plannedRewardPages);
	}

	private IEnumerator DisplayPlannedRewardPages(IEnumerable<RewardsPage> plannedRewardPages)
	{
		foreach (RewardsPage page in plannedRewardPages)
		{
			string key = (page.LastPage ? "MainNav/Rewards/EventRewards/ClaimPrizeButton" : "MainNav/Shared/More");
			_rewardsClaimButtonText.text = Languages.ActiveLocProvider.GetLocalizedText(key);
			foreach (RewardsPageItem item in page)
			{
				yield return item.DisplayReward;
				yield return new WaitForSeconds(_sequenceDelay);
			}
			if (!page.LastPage)
			{
				yield return WaitForMoreClicked();
			}
		}
	}

	private IEnumerator WaitForMoreClicked()
	{
		_rewardsClaimButtonText.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Shared/More");
		SubPageDisplayed = true;
		yield return new WaitWhile(() => SubPageDisplayed);
		Clear(closeRewards: false);
	}

	public void OnBackgroundClicked_Unity()
	{
		OnClaimClicked_Unity();
	}

	public void OnClaimClicked_Unity()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, base.gameObject);
		if (SubPageDisplayed)
		{
			_stuckOnClaimClickedCounter = 0;
			SubPageDisplayed = false;
		}
		else if ((_stillDisplayingSubpagesForRewardItem || _seasonRewardCoroutine != null || _claimRewardsCoroutine != null) && _stuckOnClaimClickedCounter < 10)
		{
			_stuckOnClaimClickedCounter++;
		}
		else if (IsHandlingEndOfSeasonDisplay())
		{
			_stuckOnClaimClickedCounter = 0;
			HandleEndOfSeasonClicked();
		}
		else if (_claimRewardsCoroutine == null)
		{
			_stuckOnClaimClickedCounter = 0;
			_claimRewardsCoroutine = StartCoroutineSafely(ClaimRewards());
		}
	}

	private IEnumerator ClaimRewards()
	{
		bool revealing = false;
		RewardDisplayCard[] array = _cardReward.Instances.Concat(_cardRewardWithBonus.Instances).ToArray();
		foreach (RewardDisplayCard rewardDisplayCard in array)
		{
			if (!rewardDisplayCard.IsFlipped())
			{
				revealing = true;
				rewardDisplayCard.FlipCard();
				yield return new WaitForSeconds(_sequenceDelay);
			}
		}
		MetaDeckView[] array2 = _deckBoxReward.Instances.ToArray();
		foreach (MetaDeckView metaDeckView in array2)
		{
			if (!metaDeckView.IsDeckBoxOpen())
			{
				revealing = true;
				metaDeckView.TriggerOpenEffect();
				yield return new WaitForSeconds(_sequenceDelay);
			}
		}
		if (!revealing)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_quest_claim_reward, _standardClaimButton.gameObject);
			if (SubPageDisplayed)
			{
				SubPageDisplayed = false;
			}
			else
			{
				_onRewardsWillClose?.Invoke();
				_onRewardsWillClose = null;
				Clear();
			}
		}
		_claimRewardsCoroutine = null;
	}

	private void OnEnable()
	{
		_keyboardManager?.Subscribe(this);
	}

	private void OnDisable()
	{
		_keyboardManager?.Unsubscribe(this);
		_onRewardsWillClose = null;
	}

	public virtual bool HandleKeyDown(KeyCode curr, Modifiers mods)
	{
		if (Visible && curr == KeyCode.Escape)
		{
			OnClaimClicked_Unity();
			return true;
		}
		return false;
	}

	public virtual void OnBack(ActionContext context)
	{
		OnClaimClicked_Unity();
	}
}
