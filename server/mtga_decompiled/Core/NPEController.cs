using System;
using System.Collections.Generic;
using AssetLookupTree;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.UXEvents;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Hangers;
using Wotc.Mtgo.Gre.External.Messaging;

public class NPEController : MonoBehaviour
{
	public enum Actor
	{
		ThePlayer,
		Tree,
		BrokenTree,
		Elf,
		Goblin,
		Merfolk,
		Assassin,
		Boss,
		Spark,
		Token,
		Ogre
	}

	[SerializeField]
	private AbilityHanger _AbilityHanger_Prefab;

	[SerializeField]
	private float _abilityHangerOffset = 1.5f;

	[SerializeField]
	private NPE_Card_Augment _Power_Hanger_Prefab;

	[SerializeField]
	private NPE_Card_Augment _Toughness_Hanger_Prefab;

	[SerializeField]
	private NPE_Card_Augment _Powers_Icon_Prefab;

	[SerializeField]
	private NPE_Card_Augment _Toughness_Icon_Prefab;

	private NPE_Card_Augment _powerIcon;

	private NPE_Card_Augment _toughnessIcon;

	[Space(10f)]
	[SerializeField]
	private Canvas _UXPromptsCanvas;

	[SerializeField]
	private Canvas _UXOverlayCanvas;

	[SerializeField]
	private Canvas _UXCinematicCanvas;

	[SerializeField]
	private Canvas _SparkyCanvas;

	[SerializeField]
	private Transform _UXPromptsContainer;

	[SerializeField]
	private Transform _UXPromptsContainerSafeArea;

	[SerializeField]
	private Transform _UXOverlayContainer;

	[SerializeField]
	private Transform _UXCinematicContainer;

	[SerializeField]
	private DismissableDeluxeTooltip _manaTooltip_Prefab;

	[SerializeField]
	private DismissableDeluxeTooltip _combatTooltip_Prefab;

	[SerializeField]
	private DismissableDeluxeTooltip _elf_Prefab;

	[SerializeField]
	private CinematicCurtain _cinematicCurtain_Prefab;

	[SerializeField]
	private DismissableDeluxeTooltip _dominariaFall_Prefab;

	[SerializeField]
	private NPEPrompt CenterPrompt_Prefab;

	[SerializeField]
	private NPEPrompt ButtonPrompt_Prefab;

	[SerializeField]
	private ChatBubble Opponent_DialogBubble_Prefab;

	[SerializeField]
	private ChatBubble DialogBubble_Right_Prefab;

	[SerializeField]
	private ChatBubble DialogBubble_Top_Prefab;

	[SerializeField]
	private bool _promptsUseCameraSpace;

	private CinematicCurtain _cinematicCurtain;

	private Dictionary<DeluxeTooltipType, DeluxeTooltip> _deluxeTooltips;

	private Dictionary<DeluxeTooltipType, DismissableDeluxeTooltip> _dismissableDeluxeTooltips;

	private NPEPrompt _centerPrompt;

	private NPEPrompt _buttonPrompt;

	private ChatBubble Opponent_DialogBubble;

	private ChatBubble Sparky_DialogBubble;

	private ChatBubble BattlefieldCard_DialogBubble;

	private GameManager _gameManager;

	private IMatchSceneStateProvider _matchSceneStateProvider = NullMatchSceneStateProvider.Default;

	private MatchManager _matchManager;

	private CardHolderReference<IBattlefieldCardHolder> _battlefieldRef;

	[Space(10f)]
	[SerializeField]
	public GameObject BottomQuarryHalo;

	[SerializeField]
	public GameObject TopQuarryHalo;

	[Space(10f)]
	private int _SparkyIsSpeakingRefCount;

	private Queue<NPEDialogUXEvent> _PendingSparkyVerbiage = new Queue<NPEDialogUXEvent>();

	private bool _GameIsReady;

	[SerializeField]
	private SparkyController _SparkyPrefab;

	private SparkyController _SparkyInScene;

	[SerializeField]
	private GameObject _SparkyHighlightInHand_Prefab;

	[SerializeField]
	private GameObject _SparkyHighlightOnBattlefield_Prefab;

	[SerializeField]
	private GameObject _SparkyHighlightButton_Prefab;

	public CombatState _SparkyCombatState;

	public SparkySpots DesiredSparkySpot = SparkySpots.Default;

	[SerializeField]
	private Transform _SparkyCinematicCenterPosition;

	[SerializeField]
	private Transform _SparkyDefaultPosition;

	[SerializeField]
	private Transform _SparkyCombatBegunPosition;

	[SerializeField]
	private Transform _SparkyCombatActivePosition;

	[SerializeField]
	private Vector3 _SparkyDialogOffset;

	[SerializeField]
	private Vector3 _SparkyPerchOffset_Hand;

	[SerializeField]
	private Vector3 _SparkyPerchOffset_Battlefield;

	[SerializeField]
	private Vector3 _SparkyPerchOffset_Button;

	private float _DynamicSparkyCombatBegunX;

	[SerializeField]
	private float _DynamicSparkyCombatBegunXOffset = 4f;

	private float _DynamicSparkyCombatActiveX;

	[SerializeField]
	private float _DynamicSparkyCombatActiveXOffset = 4f;

	private List<AbilityHanger> _activeBattlefieldAbilityHangers = new List<AbilityHanger>();

	private List<GameObject> _activeEmphasisArrows = new List<GameObject>();

	private List<GameObject> _NPEHighlights = new List<GameObject>();

	private List<DuelScene_CDC> _animatedCards = new List<DuelScene_CDC>();

	public DuelScene_CDC SparkyTargetCardView;

	[SerializeField]
	private RuntimeAnimatorController _emphasisController;

	public NPEDialogUXEvent CurrentDialog;

	public NPEPauseUXEvent CurrentPause;

	public NPEShowBattlefieldHangerUXEvent CurrentHangerEvent;

	private NPEReminder CurrentReminder;

	private DelayedReminderPhase reminderPhase;

	private Actor OpponentPortrait;

	[SerializeField]
	private GameObject _SpinningHourglassPrefab;

	private GameObject _hourglassInstance;

	private AssetLookupSystem _assetLookupSystem;

	private IFaceInfoGenerator _faceInfoGenerator;

	private bool ShowCinematicAtStart;

	private IBattlefieldCardHolder Battlefield
	{
		get
		{
			if (_battlefieldRef != null)
			{
				return _battlefieldRef.Get();
			}
			if (_gameManager.CardHolderManager != null)
			{
				_battlefieldRef = CardHolderReference<IBattlefieldCardHolder>.Battlefield(_gameManager.CardHolderManager);
				return _battlefieldRef.Get();
			}
			return null;
		}
	}

	public bool AutoSkipTooltips { get; set; }

	public void Init(NPE_Game npeGame, GameManager gameManager, IMatchSceneStateProvider matchSceneStateProvider, AssetLookupSystem assetLookupSystem, MatchManager matchManager)
	{
		_gameManager = gameManager;
		_matchSceneStateProvider = matchSceneStateProvider ?? NullMatchSceneStateProvider.Default;
		_matchManager = matchManager;
		OpponentPortrait = npeGame.OpponentPortrait;
		ShowCinematicAtStart = npeGame.ShowCinematicAtStart;
		if (ShowCinematicAtStart)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_npe_intro_blackspace, base.gameObject);
			AudioManager.SetState("NPE_Intro", "None");
		}
		if (_promptsUseCameraSpace)
		{
			_UXPromptsCanvas.worldCamera = gameManager.MainCamera;
			_UXPromptsCanvas.sortingLayerName = "Foreground";
		}
		_UXOverlayCanvas.worldCamera = gameManager.MainCamera;
		_UXCinematicCanvas.worldCamera = gameManager.MainCamera;
		_SparkyCanvas.worldCamera = gameManager.MainCamera;
		_assetLookupSystem = assetLookupSystem;
		_faceInfoGenerator = FaceInfoGeneratorFactory.HoverGenerator(_gameManager.CardDatabase, _assetLookupSystem, _gameManager.GenericPool);
	}

	private void Start()
	{
		_deluxeTooltips = new Dictionary<DeluxeTooltipType, DeluxeTooltip>();
		_dismissableDeluxeTooltips = new Dictionary<DeluxeTooltipType, DismissableDeluxeTooltip>();
		DismissableDeluxeTooltip dismissableDeluxeTooltip = UnityEngine.Object.Instantiate(_manaTooltip_Prefab, _UXOverlayContainer);
		DismissableDeluxeTooltip dismissableDeluxeTooltip2 = UnityEngine.Object.Instantiate(_combatTooltip_Prefab, _UXOverlayContainer);
		DismissableDeluxeTooltip dismissableDeluxeTooltip3 = UnityEngine.Object.Instantiate(_elf_Prefab, _UXOverlayContainer);
		DismissableDeluxeTooltip dismissableDeluxeTooltip4 = UnityEngine.Object.Instantiate(_dominariaFall_Prefab, base.transform);
		_dismissableDeluxeTooltips.Add(DeluxeTooltipType.Mana, dismissableDeluxeTooltip);
		_dismissableDeluxeTooltips.Add(DeluxeTooltipType.Combat, dismissableDeluxeTooltip2);
		_dismissableDeluxeTooltips.Add(DeluxeTooltipType.Elf, dismissableDeluxeTooltip3);
		_dismissableDeluxeTooltips.Add(DeluxeTooltipType.DominariaFall, dismissableDeluxeTooltip4);
		_SparkyInScene = UnityEngine.Object.Instantiate(_SparkyPrefab, base.transform);
		dismissableDeluxeTooltip.Hide();
		dismissableDeluxeTooltip4.Hide();
		dismissableDeluxeTooltip2.Hide();
		dismissableDeluxeTooltip3.Hide();
		_cinematicCurtain = UnityEngine.Object.Instantiate(_cinematicCurtain_Prefab, _UXCinematicContainer);
		if (ShowCinematicAtStart)
		{
			_cinematicCurtain.gameObject.SetActive(value: true);
			_SparkyInScene.Travel(_SparkyCinematicCenterPosition.position, immediate: true);
			DesiredSparkySpot = SparkySpots.CinematicCenter;
		}
		else
		{
			_cinematicCurtain.gameObject.SetActive(value: false);
			_SparkyInScene.Travel(_SparkyDefaultPosition.position, immediate: true);
			DesiredSparkySpot = SparkySpots.Default;
		}
		_centerPrompt = UnityEngine.Object.Instantiate(CenterPrompt_Prefab, _UXPromptsContainerSafeArea);
		_centerPrompt.InjectedGameManager = _gameManager;
		_centerPrompt.Hide();
		_buttonPrompt = UnityEngine.Object.Instantiate(ButtonPrompt_Prefab, _UXPromptsContainerSafeArea);
		_buttonPrompt.InjectedGameManager = _gameManager;
		_buttonPrompt.Hide();
		Opponent_DialogBubble = UnityEngine.Object.Instantiate(Opponent_DialogBubble_Prefab, _UXPromptsContainer);
		Sparky_DialogBubble = UnityEngine.Object.Instantiate(DialogBubble_Right_Prefab, _UXPromptsContainer);
		BattlefieldCard_DialogBubble = UnityEngine.Object.Instantiate(DialogBubble_Top_Prefab, _UXPromptsContainer);
		_powerIcon = UnityEngine.Object.Instantiate(_Powers_Icon_Prefab);
		_powerIcon.CleanUp_Hover();
		_toughnessIcon = UnityEngine.Object.Instantiate(_Toughness_Icon_Prefab);
		_toughnessIcon.CleanUp_Hover();
		CardHoverController.OnHoveredCardUpdated += OnHoveredCardUpdated;
	}

	private void OnHoveredCardUpdated(DuelScene_CDC cdc)
	{
		if ((bool)_gameManager)
		{
			if (cdc == null)
			{
				_gameManager.NpeDirector.Play();
				UnityEngine.Object.Destroy(_hourglassInstance);
			}
			else if (cdc.CurrentCardHolder.CardHolderType == CardHolderType.Stack)
			{
				_gameManager.NpeDirector.Pause();
				cdc.UpdateHighlight(HighlightType.Cold);
				_hourglassInstance = UnityEngine.Object.Instantiate(_SpinningHourglassPrefab, cdc.transform);
				_hourglassInstance.transform.localPosition = new Vector3(-3.5f, 0f, 0f);
			}
		}
	}

	private void OnDisable()
	{
		CardHoverController.OnHoveredCardUpdated -= OnHoveredCardUpdated;
		ClearReminder();
	}

	private void OnDestroy()
	{
		if ((bool)_centerPrompt)
		{
			_centerPrompt.InjectedGameManager = null;
		}
		if ((bool)_buttonPrompt)
		{
			_buttonPrompt.InjectedGameManager = null;
		}
		_battlefieldRef?.ClearCache();
		_matchSceneStateProvider = NullMatchSceneStateProvider.Default;
	}

	public void LaunchDismissableDeluxeTooltip(DeluxeTooltipType type, System.Action dismissButtonPayload)
	{
		if (_dismissableDeluxeTooltips.TryGetValue(type, out var value))
		{
			value.Launch(dismissButtonPayload, AutoSkipTooltips);
		}
		else
		{
			dismissButtonPayload?.Invoke();
		}
	}

	public void ShowDeluxeTooltip(DeluxeTooltipType type)
	{
		if (_deluxeTooltips.TryGetValue(type, out var value))
		{
			value.Show();
		}
	}

	public void HideDeluxeTooltip(DeluxeTooltipType type)
	{
		if (_deluxeTooltips.TryGetValue(type, out var value))
		{
			value.Hide();
		}
	}

	public void ReminderInFinalForm()
	{
		reminderPhase = DelayedReminderPhase.HighlightsGoing;
	}

	public void SetDynamicSparkyCombatXPosition()
	{
		MtgGameState currentGameState = _gameManager.CurrentGameState;
		List<uint> cardIds = currentGameState.Battlefield.CardIds;
		float num = 0f;
		float num2 = 0f;
		foreach (uint item in cardIds)
		{
			MtgCardInstance cardById = currentGameState.GetCardById(item);
			if (cardById != null && cardById.CardTypes.Contains(CardType.Creature) && _gameManager.ViewManager.TryGetCardView(cardById.InstanceId, out var cardView))
			{
				num = Math.Max(num, cardView.transform.position.x);
				num2 = Math.Min(num2, cardView.transform.position.x);
			}
		}
		_DynamicSparkyCombatBegunX = num + 3f;
		_DynamicSparkyCombatActiveX = num2 - 3f;
	}

	public void Update()
	{
		if (CurrentReminder != null)
		{
			TimeSpan timeSpan = DateTime.UtcNow - CurrentReminder.TimeOfActivation;
			TimeSpan timeSpan2 = TimeSpan.FromSeconds(CurrentReminder.TimeToWaitForToolTip);
			TimeSpan timeSpan3 = TimeSpan.FromSeconds(CurrentReminder.TimeToWaitForToolTip + CurrentReminder.TimeToWaitForSparkyDispatch);
			if (timeSpan > timeSpan2 && (bool)_gameManager)
			{
				if (reminderPhase == DelayedReminderPhase.Resting)
				{
					reminderPhase = DelayedReminderPhase.ShowingText;
					CurrentReminder.ShowToolTip(this);
				}
				if (reminderPhase == DelayedReminderPhase.ShowingText && timeSpan > timeSpan3)
				{
					reminderPhase = DelayedReminderPhase.SparkyDispatched;
					CurrentReminder.DispatchSparky(this, _gameManager);
				}
			}
		}
		if (_SparkyIsSpeakingRefCount > 0)
		{
			PutSparkyDialogBubbleOnSparky();
		}
		Vector3 location;
		switch (DesiredSparkySpot)
		{
		case SparkySpots.CinematicCenter:
			location = _SparkyCinematicCenterPosition.position;
			break;
		case SparkySpots.ACard:
		{
			Vector3 vector = ((SparkyTargetCardView.Model.Zone.Type == ZoneType.Hand) ? _SparkyPerchOffset_Hand : _SparkyPerchOffset_Battlefield);
			location = SparkyTargetCardView.PartsRoot.position + vector;
			break;
		}
		case SparkySpots.PromptButton:
			location = _gameManager.UIManager.GetMainPromptButton().transform.position + _SparkyPerchOffset_Button;
			break;
		case SparkySpots.MinorButton:
			location = _gameManager.UIManager.GetSecondaryPromptButton().transform.position + _SparkyPerchOffset_Button;
			break;
		default:
			if (_SparkyCombatState == CombatState.CombatBegun)
			{
				location = _SparkyCombatBegunPosition.position;
				if (_DynamicSparkyCombatBegunX > location.x)
				{
					location.x = _DynamicSparkyCombatBegunX + _DynamicSparkyCombatBegunXOffset;
				}
			}
			else if (_SparkyCombatState == CombatState.CreaturesActive)
			{
				location = _SparkyCombatActivePosition.position;
				if (_DynamicSparkyCombatActiveX < location.x)
				{
					location.x = _DynamicSparkyCombatActiveX - _DynamicSparkyCombatActiveXOffset;
				}
			}
			else
			{
				location = _SparkyDefaultPosition.position;
			}
			break;
		}
		if (_SparkyIsSpeakingRefCount == 0)
		{
			_SparkyInScene.Travel(location);
		}
		if (_SparkyInScene.TargetVector.sqrMagnitude < 0.5f)
		{
			if (_PendingSparkyVerbiage.Count > 0)
			{
				LetSparkySpeak(_PendingSparkyVerbiage.Dequeue());
			}
			if (reminderPhase == DelayedReminderPhase.SparkyDispatched)
			{
				reminderPhase = DelayedReminderPhase.HighlightsGoing;
				if (DesiredSparkySpot == SparkySpots.ACard)
				{
					EmphasizeCards();
				}
				if (DesiredSparkySpot == SparkySpots.PromptButton || DesiredSparkySpot == SparkySpots.MinorButton)
				{
					StyledButton styledButton = _gameManager.UIManager.GetMainPromptButton();
					if (DesiredSparkySpot == SparkySpots.MinorButton)
					{
						styledButton = _gameManager.UIManager.GetSecondaryPromptButton();
					}
					GameObject gameObject = UnityEngine.Object.Instantiate(_SparkyHighlightButton_Prefab, styledButton.transform, worldPositionStays: true);
					gameObject.transform.ZeroOut();
					_NPEHighlights.Add(gameObject);
				}
			}
		}
		if (!_GameIsReady && _gameManager.NpeDirector != null && _gameManager.NpeDirector.ShouldBePaused && _matchSceneStateProvider.SubScene == MatchSceneManager.SubScene.DuelScene)
		{
			_GameIsReady = true;
			_gameManager.NpeDirector.Play();
		}
	}

	private void EmphasizeCards()
	{
		if (!(SparkyTargetCardView != null))
		{
			return;
		}
		if (CurrentReminder is DeclareReminder)
		{
			foreach (uint sparkySuggestedInstance in ((DeclareReminder)CurrentReminder).SparkySuggestedInstances)
			{
				EmphasizeCardHighlight(_gameManager.ViewManager.GetCardView(sparkySuggestedInstance), useAnimation: false);
			}
			return;
		}
		_ = SparkyTargetCardView.Model.InstanceId;
		EmphasizeCardHighlight(SparkyTargetCardView, useAnimation: true);
	}

	private void EmphasizeCardHighlight(DuelScene_CDC cardView, bool useAnimation)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate((cardView.Model.Zone.Type == ZoneType.Hand) ? _SparkyHighlightInHand_Prefab : _SparkyHighlightOnBattlefield_Prefab, cardView.PartsRoot, worldPositionStays: true);
		gameObject.transform.ZeroOut();
		_NPEHighlights.Add(gameObject);
		if (useAnimation)
		{
			cardView.gameObject.AddComponent<Animator>();
			cardView.GetComponent<Animator>().runtimeAnimatorController = _emphasisController;
			_animatedCards.Add(cardView);
		}
	}

	public void KillEmphasisAnimationOnHoverCard(DuelScene_CDC card)
	{
		if (card.GetComponent<Animator>() != null)
		{
			card.GetComponent<Animator>().runtimeAnimatorController = null;
			card.PartsRoot.ZeroOut();
		}
	}

	private void KillEmphasisAnims()
	{
		foreach (GameObject activeEmphasisArrow in _activeEmphasisArrows)
		{
			UnityEngine.Object.Destroy(activeEmphasisArrow);
		}
		_activeEmphasisArrows.Clear();
		foreach (GameObject nPEHighlight in _NPEHighlights)
		{
			UnityEngine.Object.Destroy(nPEHighlight);
		}
		_NPEHighlights.Clear();
		foreach (DuelScene_CDC animatedCard in _animatedCards)
		{
			if (animatedCard != null)
			{
				animatedCard.GetComponent<Animator>().runtimeAnimatorController = null;
				animatedCard.PartsRoot.ZeroOut();
			}
		}
		_animatedCards.Clear();
	}

	public void ClearReminder()
	{
		DesiredSparkySpot = SparkySpots.Default;
		reminderPhase = DelayedReminderPhase.Resting;
		SparkyTargetCardView = null;
		CurrentReminder = null;
		KillEmphasisAnims();
	}

	public void ShowPrompt(MTGALocalizedString text = null, PromptType type = PromptType.Button)
	{
		switch (type)
		{
		case PromptType.Button:
			_buttonPrompt.ShowFade(text);
			break;
		case PromptType.Center:
			_centerPrompt.ShowFade(text);
			break;
		case PromptType.CenterPop:
			_centerPrompt.ShowPop(text);
			break;
		}
	}

	public void HidePrompt(PromptType type = PromptType.Button)
	{
		switch (type)
		{
		case PromptType.Button:
			_buttonPrompt.Hide();
			break;
		case PromptType.Center:
		case PromptType.CenterPop:
			_centerPrompt.Hide();
			break;
		}
	}

	public void HideAllPrompts()
	{
		_centerPrompt.Hide();
		_buttonPrompt.Hide();
	}

	private void PutSparkyDialogBubbleOnSparky()
	{
		if ((bool)_gameManager && (bool)Sparky_DialogBubble)
		{
			SetDialogPosition(Sparky_DialogBubble, _SparkyInScene.transform.position);
			if (!_promptsUseCameraSpace)
			{
				Sparky_DialogBubble.transform.position += _SparkyDialogOffset;
			}
		}
	}

	private void LetSparkySpeak(NPEDialogUXEvent whatToSay)
	{
		if (whatToSay == null)
		{
			return;
		}
		PutSparkyDialogBubbleOnSparky();
		AudioManager.PlayAudio(whatToSay.WwiseEvent, AudioManager.Default);
		_SparkyIsSpeakingRefCount++;
		whatToSay.SpeakingStarted = true;
		if (!Sparky_DialogBubble)
		{
			return;
		}
		Sparky_DialogBubble.Show(whatToSay.Line);
		if ((bool)_gameManager)
		{
			NPEDirector npeDirector = _gameManager.NpeDirector;
			if (npeDirector != null)
			{
				Sparky_DialogBubble.Clicked += npeDirector.ClearNPEUXPrompts;
				Sparky_DialogBubble.Clicked += FinishUpSparkySay;
			}
		}
		void FinishUpSparkySay()
		{
			whatToSay.Complete();
			Sparky_DialogBubble.Clicked -= FinishUpSparkySay;
		}
	}

	public void ShowDialog(NPEDialogUXEvent uxevent)
	{
		CurrentDialog = uxevent;
		Actor character = uxevent.Character;
		MTGALocalizedString line = uxevent.Line;
		string wwiseEvent = uxevent.WwiseEvent;
		bool followCard = uxevent.FollowCard;
		if (character == Actor.Spark)
		{
			_PendingSparkyVerbiage.Enqueue(uxevent);
		}
		else if (character == OpponentPortrait)
		{
			EntityViewManager viewManager = _gameManager.ViewManager;
			NPEDirector npeDirector = _gameManager.NpeDirector;
			DuelScene_AvatarView avatarByPlayerSide = viewManager.GetAvatarByPlayerSide(GREPlayerNum.Opponent);
			SetDialogPosition(Opponent_DialogBubble, avatarByPlayerSide.NPEChatBubble_TargetPosition.position);
			Opponent_DialogBubble.Show(line);
			Opponent_DialogBubble.Clicked += npeDirector.ClearNPEUXPrompts;
			AudioManager.PlayAudio(wwiseEvent, AudioManager.Default);
		}
		else
		{
			ShowCardDialog(character, line, followCard);
			BattlefieldCard_DialogBubble.Clicked += _gameManager.NpeDirector.ClearNPEUXPrompts;
			AudioManager.PlayAudio(wwiseEvent, AudioManager.Default);
		}
	}

	private void SetDialogPosition(ChatBubble dialogBubble, Vector3 targetWorldPosition)
	{
		Camera mainCamera = _gameManager.MainCamera;
		Vector3 vector = mainCamera.WorldToScreenPoint(targetWorldPosition);
		if (_promptsUseCameraSpace)
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(_UXPromptsCanvas.transform as RectTransform, vector, mainCamera, out var localPoint);
			dialogBubble.transform.localPosition = localPoint;
		}
		else
		{
			dialogBubble.transform.position = vector;
		}
	}

	public void HideDialog(Actor character = Actor.Elf)
	{
		if (character == Actor.Spark)
		{
			Sparky_DialogBubble.Hide();
			Sparky_DialogBubble.Clicked -= _gameManager.NpeDirector.ClearNPEUXPrompts;
			_SparkyIsSpeakingRefCount--;
		}
		else if (character == OpponentPortrait)
		{
			Opponent_DialogBubble.Hide();
			Opponent_DialogBubble.Clicked -= _gameManager.NpeDirector.ClearNPEUXPrompts;
		}
		else
		{
			BattlefieldCard_DialogBubble.Hide();
			BattlefieldCard_DialogBubble.Clicked -= _gameManager.NpeDirector.ClearNPEUXPrompts;
		}
	}

	public void ShowWarning(MTGALocalizedString text)
	{
		_centerPrompt.ShowPop(text);
	}

	public void StopShowingWarning()
	{
		_centerPrompt.Hide();
	}

	private void ShowCardDialog(Actor character, MTGALocalizedString text, bool followCard = false)
	{
		List<DuelScene_CDC> list = Battlefield.CardViews.FindAll((DuelScene_CDC x) => !x.Model.Controller.IsLocalPlayer && x.Model.CardTypes.Contains(CardType.Creature));
		DuelScene_CDC duelScene_CDC = null;
		Camera mainCamera = null;
		if (list.Count > 0)
		{
			duelScene_CDC = list[0];
			if (character == Actor.Ogre)
			{
				foreach (DuelScene_CDC item in list)
				{
					if (item.Model.Power.Value >= 7)
					{
						duelScene_CDC = item;
					}
				}
			}
			if ((bool)_gameManager)
			{
				SetDialogPosition(BattlefieldCard_DialogBubble, duelScene_CDC.transform.position);
			}
		}
		BattlefieldCard_DialogBubble.Show(text, followCard, duelScene_CDC, mainCamera);
	}

	public void ShowPTIconsOnHoverCard(DuelScene_CDC card)
	{
		_powerIcon.ShowUp_Hover(card.PartsRoot.transform);
		_toughnessIcon.ShowUp_Hover(card.PartsRoot.transform);
	}

	public NPE_Card_Augment ShowExplainerOnBattlefield(uint anchorCard, bool power)
	{
		MtgGameState currentGameState = _gameManager.CurrentGameState;
		foreach (uint cardId in currentGameState.Battlefield.CardIds)
		{
			MtgCardInstance cardById = currentGameState.GetCardById(cardId);
			if (anchorCard.Equals(cardById.GrpId) && !cardById.HasSummoningSickness)
			{
				DuelScene_CDC cardView = _gameManager.ViewManager.GetCardView(cardId);
				if (power)
				{
					NPE_Card_Augment nPE_Card_Augment = UnityEngine.Object.Instantiate(_Power_Hanger_Prefab);
					nPE_Card_Augment.ShowUp_OnBattlefield(cardView.PartsRoot.transform);
					return nPE_Card_Augment;
				}
				NPE_Card_Augment nPE_Card_Augment2 = UnityEngine.Object.Instantiate(_Toughness_Hanger_Prefab);
				nPE_Card_Augment2.ShowUp_OnBattlefield(cardView.PartsRoot.transform);
				return nPE_Card_Augment2;
			}
		}
		return null;
	}

	public void ShowHangerOnBattlefield(uint anchorCard, HangerSituation hangerSituation, bool showLeftSide)
	{
		MtgGameState currentGameState = _gameManager.CurrentGameState;
		foreach (uint cardId in currentGameState.Battlefield.CardIds)
		{
			MtgCardInstance cardById = currentGameState.GetCardById(cardId);
			if (anchorCard.Equals(cardById.GrpId) && (!hangerSituation.ShowOnlyTapped || cardById.IsTapped))
			{
				DuelScene_CDC cardView = _gameManager.ViewManager.GetCardView(cardId);
				AbilityHanger abilityHanger = UnityEngine.Object.Instantiate(_AbilityHanger_Prefab);
				abilityHanger.transform.ZeroOut();
				float x = -8f;
				if (showLeftSide)
				{
					x = 8f;
				}
				abilityHanger.transform.position = cardView.transform.position + new Vector3(x, _abilityHangerOffset, 6f);
				abilityHanger.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
				abilityHanger.transform.localEulerAngles = new Vector3(90f, 180f, 0f);
				abilityHanger.gameObject.SetLayer(LayerMask.NameToLayer("CardsExamine"));
				abilityHanger.Init(_gameManager.gameObject.transform, _gameManager.Context, _gameManager.AssetLookupSystem, _faceInfoGenerator, _matchManager?.Event?.PlayerEvent?.Format, _gameManager.NpeDirector);
				abilityHanger.ActivateHanger(cardView, cardView.Model, hangerSituation);
				if (showLeftSide)
				{
					abilityHanger.IsDisplayedOnLeftSide = true;
				}
				_activeBattlefieldAbilityHangers.Add(abilityHanger);
				break;
			}
		}
	}

	public void ClearPTIconsOnHoverCard()
	{
		_powerIcon.CleanUp_Hover();
		_toughnessIcon.CleanUp_Hover();
	}

	public void FadeOutBattlefieldAbilityHangers()
	{
		foreach (AbilityHanger activeBattlefieldAbilityHanger in _activeBattlefieldAbilityHangers)
		{
			activeBattlefieldAbilityHanger.Shutdown();
			UnityEngine.Object.Destroy(activeBattlefieldAbilityHanger);
		}
		_activeBattlefieldAbilityHangers.Clear();
	}

	public void DropCurtain()
	{
		_cinematicCurtain.Hide();
		DesiredSparkySpot = SparkySpots.Default;
	}

	public void BeginQueuedReminder(NPEReminder reminder)
	{
		reminder.TimeOfActivation = DateTime.UtcNow;
		CurrentReminder = reminder;
	}
}
