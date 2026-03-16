using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.Prefab;
using Core.Code.Familiar;
using GreClient.CardData;
using GreClient.Network;
using GreClient.Rules;
using InteractionSystem;
using MTGA.KeyboardManager;
using MovementSystem;
using Pooling;
using ReferenceMap;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wizards.Mtga;
using Wizards.Mtga.Assets;
using Wizards.Mtga.Platforms;
using Wizards.Mtga.UI;
using Wotc;
using Wotc.Mtga;
using Wotc.Mtga.AutoPlay;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtga.DuelScene.CardView;
using Wotc.Mtga.DuelScene.Companions;
using Wotc.Mtga.DuelScene.Emotes;
using Wotc.Mtga.DuelScene.Input;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;
using Wotc.Mtga.DuelScene.Interactions.AssignDamage;
using Wotc.Mtga.DuelScene.Interactions.SelectN;
using Wotc.Mtga.DuelScene.Interactions.SelectTargets;
using Wotc.Mtga.DuelScene.PlayerNameViews;
using Wotc.Mtga.DuelScene.UXEvents;
using Wotc.Mtga.DuelScene.VFX;
using Wotc.Mtga.DuelScene.ZoneCounts;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Hangers;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;
using Wotc.Mtga.TimedReplays;
using Wotc.Mtgo.Gre.External.Messaging;

public class GameManager : MonoBehaviour, IKeyUpSubscriber, IKeySubscriber, IKeyDownSubscriber, IKeyHeldSubscriber
{
	private IContext _matchContext = NullContext.Default;

	public IContext Context = NullContext.Default;

	private readonly IHighlightManager _highlightManager = new HighlightManager();

	private IDimmingController _dimmingController = NullDimmingController.Default;

	private IntentionLineManager _intentionLineManager;

	private IVisualStateCardProvider _visualStateCardProvider = NullVisualCardProvider.Default;

	private ICombatIconUpdater _combatIconUpdater = NullCombatIconUpdater.Default;

	private bool _sideboardSubmitted;

	private NPEState _npeState;

	private GreInterface _gre;

	private NPEDirector _npeDirector;

	private NPEController _npeController;

	private IEventTranslation _uxEventTranslator;

	private IPromptTextManager _promptTextManager = NullPromptTextManager.Default;

	private AltArtMatchSettings _altArtMatchSettings;

	private SettingsMenuHost _settingsMenuHost;

	private readonly IGameStateManager _gameStateManager = new GameStateManager();

	public readonly MapAggregate ReferenceMapAggregate = new MapAggregate();

	private IEmoteDataProvider _emoteDataProvider;

	private readonly HashSet<FamiliarRequestHandler> _familiarRequestHandlers = new HashSet<FamiliarRequestHandler>();

	private readonly HashSet<IDisposable> _disposables = new HashSet<IDisposable>();

	private readonly HashSet<IUpdate> _updateObservers = new HashSet<IUpdate>();

	private readonly SettingsMessageController _settingsMessageController = new SettingsMessageController();

	private IDuelSceneStateManager _stateManager = NullDuelSceneStateManager.Default;

	public Camera MainCamera { get; private set; }

	private StaticZoomController _staticHoverCardHolder { get; set; }

	private MatchSceneManager MatchSceneManager { get; set; }

	public DuelSceneLogger Logger { get; private set; }

	public CardHolderManager CardHolderManager { get; private set; }

	public EntityViewManager ViewManager { get; private set; }

	public UIManager UIManager { get; private set; }

	public SpinnerController SpinnerController { get; private set; }

	public TimerManager TimerManager { get; private set; }

	public UIMessageHandler UIMessageHandler { get; private set; } = new UIMessageHandler();

	public CombatAnimationPlayer CombatAnimationPlayer { get; private set; }

	public GameInteractionSystem InteractionSystem { get; private set; }

	public BrowserManager BrowserManager { get; private set; }

	public MatchManager MatchManager { get; private set; }

	public AssetCache<SplineMovementData> SplineCache { get; private set; } = new AssetCache<SplineMovementData>();

	public NPEDirector NpeDirector => _npeDirector;

	public IGREConnection GreConnection => MatchManager?.GreConnection;

	public GameSessionType SessionType => MatchManager?.SessionType ?? GameSessionType.None;

	public IClientLocProvider LocManager { get; private set; }

	public ResolutionEffectModel ActiveResolutionEffect => Context.Get<IResolutionEffectProvider>()?.ResolutionEffect;

	public MtgGameState CurrentGameState => _gameStateManager.CurrentGameState;

	public MtgGameState LatestGameState => _gameStateManager.LatestGameState;

	public IObjectPool GenericPool { get; private set; }

	public IUnityObjectPool UnityPool { get; private set; }

	public ICardDatabaseAdapter CardDatabase { get; private set; }

	public IPromptEngine PromptEngine => CardDatabase?.PromptEngine ?? NullPromptEngine.Default;

	public ISplineMovementSystem SplineMovementSystem { get; private set; }

	public UXEventQueue UXEventQueue { get; private set; } = new UXEventQueue();

	public WorkflowController WorkflowController { get; private set; }

	public IVfxProvider VfxProvider { get; private set; } = NullVfxProvider.Default;

	public WorkflowBase CurrentInteraction => WorkflowController?.CurrentWorkflow;

	private WorkflowBase PendingInteraction => WorkflowController?.PendingWorkflow;

	public AutoResponseManager AutoRespManager { get; private set; }

	public SettingsMessage CurrentSettings => AutoRespManager.CurrentSettings;

	public KeyboardManager KeyboardManager { get; private set; }

	private bool AllowInput => _stateManager.AllowInput;

	public PriorityLevelEnum Priority => PriorityLevelEnum.DuelScene;

	public AssetLookupSystem AssetLookupSystem { get; private set; }

	public MtgGameState GetCurrentGameState()
	{
		return CurrentGameState;
	}

	public WorkflowBase GetCurrentInteraction()
	{
		return CurrentInteraction;
	}

	public void Init(MatchSceneManager matchSceneManager, AssetLookupSystem assetLookupSystem, IContext matchContext)
	{
		MatchSceneManager = matchSceneManager;
		AssetLookupSystem = assetLookupSystem;
		_matchContext = matchContext;
		GenericPool = matchContext.Get<IObjectPool>() ?? NullObjectPool.Default;
		UnityPool = matchContext.Get<IUnityObjectPool>() ?? NullUnityObjectPool.Default;
		SplineMovementSystem = matchContext.Get<ISplineMovementSystem>();
		CardDatabase = matchContext.Get<ICardDatabaseAdapter>();
		LocManager = matchContext.Get<IClientLocProvider>() ?? NullLocProvider.Default;
		KeyboardManager = matchContext.Get<KeyboardManager>();
		_emoteDataProvider = matchContext.Get<IEmoteDataProvider>();
		MatchManager = matchContext.Get<MatchManager>();
		_gre = MatchManager.GreInterface;
		Logger = matchContext.Get<DuelSceneLogger>();
		_npeState = matchContext.Get<NPEState>();
		_settingsMenuHost = matchContext.Get<SettingsMenuHost>();
		_stateManager = new DuelSceneStateManager(matchContext.Get<IMatchSceneStateManager>());
		_altArtMatchSettings = new AltArtMatchSettings(CardDatabase, AssetLookupSystem);
		KeyboardManager.Subscribe(this);
		AssetLookupSystem.Blackboard.AddFillerDelegate(FillAltBlackboard);
	}

	private void Start()
	{
		if (MatchManager == null || MatchSceneManager == null)
		{
			Debug.Log("GameManager default load");
			Scenes.LoadScene("DuelSceneDebugLauncher");
			return;
		}
		SceneManager.SetActiveScene(SceneManager.GetSceneByName(MatchSceneManager.BattlefieldSceneName));
		bool fog = RenderSettings.fog;
		float fogDensity = RenderSettings.fogDensity;
		float fogEndDistance = RenderSettings.fogEndDistance;
		float fogStartDistance = RenderSettings.fogStartDistance;
		UnityEngine.Color fogColor = RenderSettings.fogColor;
		FogMode fogMode = RenderSettings.fogMode;
		SceneManager.SetActiveScene(base.gameObject.scene);
		RenderSettings.fog = fog;
		RenderSettings.fogDensity = fogDensity;
		RenderSettings.fogEndDistance = fogEndDistance;
		RenderSettings.fogStartDistance = fogStartDistance;
		RenderSettings.fogColor = fogColor;
		RenderSettings.fogMode = fogMode;
		CameraAdapter cameraAdapter = null;
		GameObject[] rootGameObjects = base.gameObject.scene.GetRootGameObjects();
		foreach (GameObject gameObject in rootGameObjects)
		{
			if (gameObject != null && gameObject.TryGetComponent<CameraAdapter>(out var component))
			{
				cameraAdapter = component;
				MainCamera = component.CameraReference;
				break;
			}
		}
		BattlefieldManager battlefieldManager = UnityEngine.Object.FindObjectOfType<BattlefieldManager>();
		battlefieldManager.InitBattlefieldReactions(UXEventQueue);
		CosmeticsProvider cosmeticsProvider = _matchContext.Get<CosmeticsProvider>();
		CardViewBuilder cardViewBuilder = _matchContext.Get<CardViewBuilder>();
		SignalBase<GameStatePlaybackCompleteSignalArgs> signalBase = new GameStatePlaybackCompleteSignal();
		SignalBase<GameStatePlaybackStartedSignalArgs> playbackStarted = new GameStatePlaybackStartedSignal();
		SignalBase<PlayerCreatedSignalArgs> playerCreatedEvent = new PlayerCreatedSignal();
		SignalBase<PlayerDeletedSignalArgs> playerDeletedEvent = new PlayerDeletedSignal();
		SignalBase<CompanionCreatedSignalArgs> companionCreatedEvent = new CompanionCreatedSignal();
		SignalBase<CardHolderCreatedSignalArgs> signalBase2 = new CardHolderCreatedSignal();
		SignalBase<ZoneCardHolderCreatedSignalArgs> signalBase3 = new ZoneCardHolderCreatedSignal();
		SignalBase<CardHolderDeletedSignalArgs> signalBase4 = new CardHolderDeletedSignal();
		SignalBase<PromptTextUpdatedSignalArgs> signalBase5 = new PromptTextUpdatedSignal();
		SignalBase<ZoneCountCreatedSignalArgs> signalBase6 = new ZoneCountCreatedSignal();
		SignalBase<ZoneCountDeletedSignalArgs> signalBase7 = new ZoneCountDeletedSignal();
		SignalBase<CameraViewportChangedSignalArgs> signalBase8 = new CameraViewportChangedSignal();
		SignalBase<IncrementPlayerLifeSignalArgs> signalBase9 = new IncrementPlayerLifeSignal();
		TurnController turnController = new TurnController();
		MutableBrowserProvider mutableBrowserProvider = new MutableBrowserProvider();
		MutableWorkflowProvider mutableWorkflowProvider = new MutableWorkflowProvider();
		MutableCardViewProvider mutableCardViewProvider = new MutableCardViewProvider();
		MutableFakeCardViewProvider mutableFakeCardViewProvider = new MutableFakeCardViewProvider();
		MutableAvatarViewProvider mutableAvatarViewProvider = new MutableAvatarViewProvider();
		MutableEntityViewProvider mutableEntityViewProvider = new MutableEntityViewProvider(mutableCardViewProvider, mutableFakeCardViewProvider, mutableAvatarViewProvider);
		MutableCardHolderProvider mutableCardHolderProvider = new MutableCardHolderProvider();
		MutableZoneCountProvider mutableZoneCountProvider = new MutableZoneCountProvider();
		ICardMovementController cardMovementController = new CardMovementController(mutableCardHolderProvider, new AltCardHolderCalculator(GenericPool, _gameStateManager, mutableCardHolderProvider));
		ICompanionDataProvider dataProvider = new CompanionDataProvider(_gameStateManager, MatchManager);
		ICompanionViewManager companionViewManager = new CompanionViewManager(new MutableCompanionViewProvider(), new CompanionViewController(new CompanionBuilder(AssetLookupSystem, cosmeticsProvider, this), dataProvider, companionCreatedEvent));
		IGameStatePlaybackController gameStatePlaybackController = new GameStatePlaybackController(_gameStateManager, _stateManager, playbackStarted, signalBase);
		ILifeTotalController lifeTotalController = new LifeTotalController(mutableAvatarViewProvider, signalBase, signalBase9);
		IPromptTextProvider promptTextProvider = new PromptTextProvider(CardDatabase, PromptEngine, _gameStateManager);
		_promptTextManager = new PromptTextManager(promptTextProvider, new PromptTextController(CardDatabase.ClientLocProvider, promptTextProvider, signalBase5));
		CanvasManager canvasManager = new CanvasManager(MainCamera);
		ICanvasRootProvider canvasRootProvider = new CanvasRootProvider(canvasManager);
		IZoneCountManager zoneCountManager = new ZoneCountManager(mutableZoneCountProvider, new ZoneCountViewBuilder(_gameStateManager, signalBase6, signalBase7, canvasRootProvider, AssetLookupSystem));
		IPlayerFocusController playerFocusController;
		if (MatchManager.Players.Count <= 2)
		{
			playerFocusController = NullPlayerFocusController.Default;
		}
		else
		{
			IPlayerFocusController playerFocusController2 = new PlayerFocusController(GenericPool, _gameStateManager, mutableAvatarViewProvider, mutableCardHolderProvider, mutableZoneCountProvider, playerDeletedEvent);
			playerFocusController = playerFocusController2;
		}
		IPlayerFocusController playerFocusController3 = playerFocusController;
		_dimmingController = new DimmingController(GenericPool, mutableCardViewProvider);
		VfxProvider = new VfxProvider(GenericPool, UnityPool, cardViewBuilder.CardMaterialBuilder, AssetLookupSystem, SplineMovementSystem, _gameStateManager, mutableEntityViewProvider, SpaceConverterFactory.Create(_gameStateManager, mutableCardHolderProvider, mutableEntityViewProvider, battlefieldManager.transform));
		CombatAnimationPlayer = new CombatAnimationPlayer(UnityPool, VfxProvider, SplineMovementSystem, mutableCardHolderProvider, AssetLookupSystem, SplineCache);
		ICardDissolveController cardDissolveController = new CardDissolveController(AssetLookupSystem, VfxProvider, cardViewBuilder, SplineMovementSystem);
		IEntityNameProvider<MtgCardInstance> cardNameProvider = new CardInstanceNameProvider(new AbilityInstanceNameProvider(CardDatabase.AbilityTextProvider), new CardNameProvider(CardDatabase.CardTitleProvider));
		IEntityNameProvider<MtgPlayer> playerNameProvider = new PlayerNameProvider(MatchManager);
		IEntityNameProvider<MtgEntity> entityNameProvider = new EntityNameProvider(cardNameProvider, playerNameProvider);
		IEntityNameProvider<uint> entityNameProvider2 = new IdNameProvider(entityNameProvider, _gameStateManager);
		ICardBuilder<DuelScene_CDC> cardBuilder = new DuelSceneCardBuilder(cardViewBuilder, _gameStateManager, mutableWorkflowProvider, VfxProvider, entityNameProvider2);
		IPlayerNameViewManager playerNameViewManager = new PlayerNameViewManager(cosmeticsProvider, AssetLookupSystem, LocManager, MatchManager, MatchManager, new PlayerNameViewBuilder(AssetLookupSystem));
		Transform transform = new GameObject("Players").transform;
		transform.SetSiblingIndex(3);
		CardViewManager cardViewManager = new CardViewManager(mutableCardViewProvider, cardBuilder);
		FakeCardViewManager fakeCardViewManager = new FakeCardViewManager(mutableFakeCardViewProvider, cardBuilder);
		Wotc.Mtga.DuelScene.AvatarBuilder avatarBuilder = new Wotc.Mtga.DuelScene.AvatarBuilder(playerCreatedEvent, playerDeletedEvent, AssetLookupSystem, transform);
		IAvatarLayout avatarLayout2;
		if (!PlatformUtils.IsHandheld())
		{
			IAvatarLayout avatarLayout = new AvatarLayout(GenericPool, new AvatarLayoutComparer(MatchManager.LocalPlayerInfo));
			avatarLayout2 = avatarLayout;
		}
		else
		{
			avatarLayout2 = NullAvatarLayout.Default;
		}
		ViewManager = new EntityViewManager(mutableEntityViewProvider, cardViewManager, fakeCardViewManager, new AvatarViewManager(mutableAvatarViewProvider, avatarBuilder, avatarLayout2, MatchManager, this));
		CardHolderManager = new CardHolderManager(mutableCardHolderProvider, new CardHolderBuilder(this, AssetLookupSystem, ViewManager, SplineMovementSystem, cardViewBuilder, LocManager, MatchManager, signalBase2, signalBase4), signalBase3);
		MatchManager.SideboardSubmitted += OnSideboardSubmitted;
		if (_npeState.ActiveNPEGame != null)
		{
			NPE_Game activeNPEGame = _npeState.ActiveNPEGame;
			_npeState.BI_NPEProgressUpdate(new NPEState.NPEProgressContext(NPEState.NPEProgressMarker.Started_Game, _npeState.ActiveNPEGameNumber));
			if (AssetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<NPEControllerPrefab> loadedTree))
			{
				NPEControllerPrefab payload = loadedTree.GetPayload(AssetLookupSystem.Blackboard);
				if (payload != null)
				{
					_npeController = AssetLoader.Instantiate<NPEController>(payload.PrefabPath);
					_npeController.Init(activeNPEGame, this, _matchContext.Get<IMatchSceneStateProvider>(), AssetLookupSystem, MatchManager);
					_npeDirector = new NPEDirector(_npeController, _npeState, UXEventQueue, activeNPEGame);
				}
			}
			SceneManager.MoveGameObjectToScene(_npeController.gameObject, SceneManager.GetSceneByName("DuelScene"));
		}
		else
		{
			_npeDirector = null;
		}
		GreInterface gre = _gre;
		IGameStateManager gameStateManager = _gameStateManager;
		NPEDirector npeDirector = _npeDirector;
		ISettingsMessageGenerator settingsMessageGenerator2;
		if (npeDirector == null || npeDirector.AllowsEndTurnButton)
		{
			ISettingsMessageGenerator settingsMessageGenerator = new SettingsMessageGenerator();
			settingsMessageGenerator2 = settingsMessageGenerator;
		}
		else
		{
			settingsMessageGenerator2 = NullSettingsMessageGenerator.Default;
		}
		AutoRespManager = new AutoResponseManager(this, gre, gameStateManager, mutableWorkflowProvider, settingsMessageGenerator2, _settingsMenuHost);
		UIMessageHandler.UpdateSendUIMessageCallback(_gre.SubmitUIMessage);
		HangerController hangerController = new HangerController();
		hangerController.Init(base.transform, MainCamera, SplineMovementSystem, FaceHanger.Create(AssetLookupSystem, base.transform, LayerMask.NameToLayer("Hand")), AbilityHanger.Create(AssetLookupSystem, base.transform, LayerMask.NameToLayer("Hand")));
		CardDragController cardDragController = new CardDragController(GenericPool, AssetLookupSystem, cardViewBuilder, MainCamera, canvasManager, mutableCardHolderProvider, mutableBrowserProvider, hangerController, SplineMovementSystem);
		TooltipSystem tooltipSystem = _matchContext.Get<TooltipSystem>();
		UIManager = new UIManager(canvasManager, this, AssetLookupSystem, UnityPool, GenericPool, tooltipSystem, MatchManager, _settingsMenuHost, _npeState, playerNameViewManager);
		UIManager.PhaseLadder.Init(this, _gameStateManager, turnController, _settingsMessageController, _gre, tooltipSystem);
		UIManager.TurnChanged.Init(AssetLookupSystem);
		IEmoteManager emoteManager2;
		if (_npeDirector == null)
		{
			IEmoteManager emoteManager = new EmoteManager(AssetLookupSystem, Logger, mutableAvatarViewProvider, _emoteDataProvider, EmoteManager.GetEquippedEmotes(_emoteDataProvider, MatchManager, cosmeticsProvider), UXEventQueue, UIMessageHandler, canvasManager.GetCanvasRoot(CanvasLayer.ScreenSpace_Default, "Emotes"), UIManager);
			emoteManager2 = emoteManager;
		}
		else
		{
			emoteManager2 = NullEmoteManager.Default;
		}
		IEmoteManager emoteManager3 = emoteManager2;
		IPlayerNumericAidController playerNumericAidController = new PlayerNumericAidController(GenericPool, emoteManager3);
		UIManager.PlayerNames.SetHighlightController(_highlightManager);
		BrowserManager = new BrowserManager(mutableBrowserProvider, MatchManager, _stateManager, this, canvasManager, cardDragController, KeyboardManager, _settingsMenuHost, cardBuilder);
		CardHolderManager.BuildNonZoneCardHolders();
		CardHoverController cardHoverController = new CardHoverController(UnityPool, SplineMovementSystem, MainCamera, cardViewBuilder, cardBuilder, mutableCardViewProvider, _highlightManager, CardHolderManager, BrowserManager, hangerController, new RelatedCardIdProvider(this, GenericPool), _npeDirector);
		SpinnerController = new SpinnerController(GenericPool, UnityPool, canvasManager.GetCanvasRoot(CanvasLayer.ScreenSpace_Default), MainCamera, _gameStateManager, ViewManager, mutableCardHolderProvider, AssetLookupSystem);
		TimerManager = new TimerManager(canvasManager, BrowserManager, UIManager, AssetLookupSystem);
		RefreshTimerLayout();
		ScreenEventController.Instance.OnScreenChanged += RefreshTimerLayout;
		IGameStateManager gameStateManager2 = _gameStateManager;
		IVfxProvider vfxProvider = VfxProvider;
		EntityViewManager viewManager = ViewManager;
		CardHolderManager cardHolderManager = CardHolderManager;
		IDuelSceneStateManager stateManager = _stateManager;
		BrowserManager browserManager = BrowserManager;
		IGameStartController gameStartController;
		if (SessionType != GameSessionType.Game)
		{
			gameStartController = NullGameStartController.Default;
		}
		else
		{
			IGameStartController gameStartController2 = new GameStartController(_gameStateManager, BrowserManager);
			gameStartController = gameStartController2;
		}
		Context = new Context(GenerateGameScopedComponents(gameStateManager2, mutableWorkflowProvider, mutableWorkflowProvider, turnController, vfxProvider, cardNameProvider, playerNameProvider, entityNameProvider, entityNameProvider2, cardBuilder, viewManager, emoteManager3, companionViewManager, cardHolderManager, cardMovementController, stateManager, gameStatePlaybackController, lifeTotalController, browserManager, gameStartController, _promptTextManager, zoneCountManager, playerFocusController3, cardDissolveController, cardHoverController, cardHoverController, _highlightManager, _dimmingController, canvasRootProvider, playerNumericAidController), _matchContext);
		DeckFormat deckFormat = MatchManager.Event?.PlayerEvent?.Format;
		Transform hangerParent = base.transform;
		if (PlatformUtils.IsHandheld())
		{
			string prefabPath = AssetLookupSystem.GetPrefabPath<StaticZoomControllerPrefab, StaticZoomController>();
			_staticHoverCardHolder = AssetLoader.Instantiate<StaticZoomController>(prefabPath, canvasManager.GetCanvasRoot(CanvasLayer.ScreenSpace_UnderStack_ScaledCorrectly));
			_staticHoverCardHolder.Init(this, ViewManager, SplineMovementSystem, cardViewBuilder, LocManager, MatchManager, FaceInfoGeneratorFactory.DuelScene.Handheld.HoverGenerator(CardDatabase, _highlightManager, AssetLookupSystem, deckFormat, GenericPool));
		}
		IFaceInfoGenerator faceInfoGenerator = FaceInfoGeneratorFactory.DuelScene.HoverGenerator(CardDatabase, _highlightManager, deckFormat, GenericPool);
		IFaceInfoGenerator faceInfoGenerator2 = FaceInfoGeneratorFactory.DuelScene.ExamineGenerator(CardDatabase, AssetLookupSystem, deckFormat, GenericPool);
		hangerController.InitFaceHanger(faceInfoGenerator, cardViewBuilder);
		hangerController.InitAbilityHanger(hangerParent, Context, AssetLookupSystem, faceInfoGenerator, deckFormat, NpeDirector);
		CardHolderManager.Examine.HangerController.InitFaceHanger(faceInfoGenerator2, cardViewBuilder);
		CardHolderManager.Examine.HangerController.InitAbilityHanger(hangerParent, Context, AssetLookupSystem, faceInfoGenerator2, deckFormat, NpeDirector);
		InteractionSystem = new GameInteractionSystem(CardDatabase, _highlightManager, _gameStateManager, mutableWorkflowProvider, CardHolderManager, _staticHoverCardHolder, ViewManager, CombatAnimationPlayer, CardHolderManager.Examine, SplineMovementSystem, BrowserManager, cardHoverController, cardDragController, hangerController);
		_combatIconUpdater = new CombatIconUpdater(_gameStateManager, mutableWorkflowProvider);
		_visualStateCardProvider = new NoDuplicatesDecorator(GenericPool, new AggregateVisualCardProvider(new ViewManagerCardProvider(ViewManager), new BrowserCardProvider(CardHolderManager, BrowserManager)));
		_intentionLineManager = new IntentionLineManager(GenericPool, UnityPool, ReferenceMapAggregate, UXEventQueue, _gameStateManager, mutableWorkflowProvider, mutableEntityViewProvider, signalBase2, CombatAnimationPlayer, BrowserManager, AssetLookupSystem, base.transform);
		if (_npeDirector != null)
		{
			TimerManager.DisableView();
			UIManager.ShowEndTurnButton = _npeDirector.AllowsEndTurnButton;
			UIManager.ShowPhaseLadder = _npeDirector.ShowPhaseLadder;
			UIManager.PhaseLadder.InTutorial = true;
		}
		_uxEventTranslator = new DefaultEventTranslation(this, AssetLookupSystem, Context, UXEventPostProcessing.Generate(Context, AssetLookupSystem, this));
		WorkflowController = new WorkflowController(WorkflowTranslator(), mutableWorkflowProvider);
		foreach (HeadlessClient familiar in MatchManager.Familiars)
		{
			_familiarRequestHandlers.Add(new FamiliarRequestHandler(familiar, Context));
		}
		_disposables.Add(new CardHolderRelativeToCameraMediator(GenericPool, cameraAdapter, mutableCardHolderProvider, signalBase8, signalBase2, signalBase4));
		_disposables.Add(new CardDragStartedMediator(cardHoverController, cardDragController));
		_disposables.Add(new PlayerHandShakeMediator(GenericPool, CombatAnimationPlayer, signalBase3));
		_disposables.Add(new BattlefieldCombatDamageMediator(battlefieldManager, CombatAnimationPlayer));
		_disposables.Add(new PlayerNameIntroVFXMediator(Context.Get<IDuelSceneStateProvider>(), UIManager));
		_disposables.Add(new CompanionEmoteMediator(GenericPool, Context.Get<IEntityDialogControllerProvider>(), playerCreatedEvent, companionCreatedEvent));
		_disposables.Add(new StaticElementLayoutMediator(signalBase2, signalBase4, UIManager.BattleFieldStaticElementsLayout));
		_disposables.Add(new StackModifiedCardholderMediator(signalBase2, signalBase4, GenericPool));
		_disposables.Add(new PromptTextDisplayMediator(UIManager.UserPrompt, BrowserManager, signalBase5));
		_disposables.Add(new LanguageChangedMediator(_promptTextManager, ViewManager, mutableCardHolderProvider, Languages.LanguageChangedSignal));
		_disposables.Add(new PreCardDestroyedMediator(cardViewBuilder, SplineMovementSystem));
		_disposables.Add(new WorkflowVisualsMediator(WorkflowController, Context.Get<IHighlightController>(), _dimmingController, _intentionLineManager, _promptTextManager, UIManager));
		_disposables.Add(new InteractionAppliedMediator(WorkflowController, Context.Get<IGameStartController>(), Logger));
		_disposables.Add(new InteractionClearedMediator(WorkflowController, BrowserManager));
		_disposables.Add(new ZoneCardHolderMediator(signalBase2, signalBase4, WorkflowController, BrowserManager, Context.Get<IHighlightProvider>()));
		_disposables.Add(new FullControlMediator(UIManager.FullControl, AutoRespManager));
		_disposables.Add(new ZoneCountViewMediator(_gameStateManager, signalBase6, signalBase7, signalBase2, signalBase4));
		_disposables.Add(new EndTurnButtonMediator(UIManager.EndTurnButton, cardDragController, AutoRespManager));
		_disposables.Add(new ClearDynamicAbilitiesMediator(CardDatabase.DynamicAbilityDataProvider));
		_disposables.Add(new PlayerPresenceMediator(UIMessageHandler, Context.Get<IPlayerPresenceController>()));
		_disposables.Add(new BattlefieldInputMediator(GenericPool, UnityPool, InteractionSystem));
		_disposables.Add(new BattlefieldEventMediator(GenericPool, _gameStateManager, signalBase, signalBase9));
		_disposables.AddIfNotNull(_uxEventTranslator as IDisposable);
		_disposables.AddIfNotNull(UIManager);
		_disposables.AddIfNotNull(hangerController);
		_disposables.AddIfNotNull(cardHoverController);
		_disposables.AddIfNotNull(InteractionSystem);
		_disposables.AddIfNotNull(Context.Get<IActionEffectController>() as IDisposable);
		_disposables.AddIfNotNull(Context.Get<ISuspendLikeController>() as IDisposable);
		_disposables.AddIfNotNull(UIMessageHandler);
		_disposables.AddIfNotNull(WorkflowController);
		_disposables.AddIfNotNull(ViewManager);
		_disposables.AddIfNotNull(Context.Get<IPendingEffectController>());
		_disposables.AddIfNotNull(Context.Get<IQualificationController>() as IDisposable);
		_disposables.AddIfNotNull(Context.Get<IGamewideCountController>() as IDisposable);
		_disposables.AddIfNotNull(Context.Get<IReplacementEffectController>() as IDisposable);
		_disposables.AddIfNotNull(Context.Get<IPlayerAbilityController>() as IDisposable);
		_disposables.AddIfNotNull(_npeDirector);
		_disposables.AddIfNotNull(_intentionLineManager);
		_disposables.AddIfNotNull(Context.Get<ICardDissolveController>() as IDisposable);
		_disposables.AddIfNotNull(cardDragController);
		_disposables.AddIfNotNull(UXEventQueue);
		_disposables.AddIfNotNull(Context.Get<IHighlightManager>() as IDisposable);
		_disposables.AddIfNotNull(_altArtMatchSettings);
		_disposables.AddIfNotNull(SplineCache);
		_disposables.AddIfNotNull(Context.Get<IPlayerPresenceController>() as IDisposable);
		_disposables.AddIfNotNull(SpinnerController);
		_disposables.AddIfNotNull(Context.Get<IResolutionEffectController>() as IDisposable);
		_disposables.AddIfNotNull(Context.Get<IGameStateManager>() as IDisposable);
		_disposables.AddIfNotNull(Context.Get<ITurnController>() as IDisposable);
		_disposables.AddIfNotNull(Context.Get<IPlayerSpriteProvider>() as IDisposable);
		_disposables.AddIfNotNull(Context.Get<IWorkflowProvider>() as IDisposable);
		_disposables.AddIfNotNull(Context.Get<IEmoteManager>() as IDisposable);
		_disposables.AddIfNotNull(Context.Get<ICompanionViewManager>() as IDisposable);
		_disposables.AddIfNotNull(Context.Get<IFaceDownIdProvider>() as IDisposable);
		_disposables.AddIfNotNull(Context.Get<IDesignationController>() as IDisposable);
		_disposables.AddIfNotNull(CombatAnimationPlayer);
		_disposables.AddIfNotNull(Context.Get<ICardHoverController>() as IDisposable);
		_disposables.AddIfNotNull(Context.Get<IGameStartController>() as IDisposable);
		_disposables.AddIfNotNull(Context.Get<ICardHolderManager>() as IDisposable);
		_disposables.AddIfNotNull(Context.Get<IPromptTextManager>() as IDisposable);
		_disposables.AddIfNotNull(Context.Get<IPlayerFocusController>() as IDisposable);
		_disposables.AddIfNotNull(Context.Get<IRevealedCardsManager>() as IDisposable);
		_disposables.AddIfNotNull(Context.Get<IZoneCountManager>() as IDisposable);
		_disposables.AddIfNotNull(Context.Get<IDuelSceneStateManager>() as IDisposable);
		_disposables.AddIfNotNull(Context.Get<IDimmingController>() as IDisposable);
		_disposables.AddIfNotNull(Context.Get<ILifeTotalController>() as IDisposable);
		_disposables.AddIfNotNull(Context.Get<IPlayerNumericAidController>() as IDisposable);
		_updateObservers.Add(new CameraViewportChangedDispatcher(cameraAdapter, signalBase8));
		_updateObservers.AddIfNotNull(Context.Get<IPlayerFocusController>() as IUpdate);
		_updateObservers.Add(BrowserManager);
		_updateObservers.AddIfNotNull(Logger);
		_updateObservers.AddIfNotNull(SpinnerController);
		_updateObservers.AddIfNotNull(Context.Get<ICardDissolveController>() as IUpdate);
		_updateObservers.Add(hangerController);
		_updateObservers.Add(new HoverEndHandler(mutableWorkflowProvider, UIManager.ManaColorSelector, CombatAnimationPlayer, CardHolderManager.Examine, mutableBrowserProvider, cardHoverController));
		_updateObservers.Add(new DragEndHandler(cardDragController, UIManager.ManaColorSelector, CombatAnimationPlayer, mutableBrowserProvider));
		_updateObservers.Add(new ButtonDisplayHandler(UIManager, mutableBrowserProvider, UIManager.ManaColorSelector));
		_updateObservers.Add(cardHoverController);
		_updateObservers.Add(cardDragController);
		foreach (FamiliarRequestHandler familiarRequestHandler in _familiarRequestHandlers)
		{
			_updateObservers.Add(familiarRequestHandler);
		}
		if (CanDisplayDebugUI())
		{
			new GameObject("DebugUI").AddComponent<DebugUI>().AddDebugModules(DebugModule.BasicModules(this, _gameStateManager, MatchManager));
		}
		if (MatchManager.IsPracticeGame)
		{
			BotControlConfigurationSO botControlConfigurationSO = BotControlManager.FetchBotConfig(AssetLookupSystem);
			SparkyChatter item = new SparkyChatter(new ChatterConfig(botControlConfigurationSO.sparkyIdleTimer, botControlConfigurationSO.sparkyIdleChatterOptions, botControlConfigurationSO.sparkyThinkingTimer, botControlConfigurationSO.sparkyThinkingChatterOptions, botControlConfigurationSO.sparkyCreatureDeathChatterOptions, botControlConfigurationSO.gameStartChatterOptions, botControlConfigurationSO.emoteReplyChatterOptions), CardDatabase.ClientLocProvider, _gameStateManager, emoteManager3, UXEventQueue, UIMessageHandler);
			_disposables.Add(item);
			_updateObservers.Add(item);
		}
		ScreenKeepAlive.KeepScreenAwake();
	}

	private IReadOnlyDictionary<Type, object> GenerateGameScopedComponents(IGameStateManager gameStateManager, IWorkflowProvider workflowProvider, IClickableWorkflowProvider clickableWorkflowProvider, TurnController turnController, IVfxProvider vfxProvider, IEntityNameProvider<MtgCardInstance> cardNameProvider, IEntityNameProvider<MtgPlayer> playerNameProvider, IEntityNameProvider<MtgEntity> entityNameProvider, IEntityNameProvider<uint> idNameProvider, ICardBuilder<DuelScene_CDC> dsCardBuilder, IEntityViewManager viewManager, IEmoteManager emoteManager, ICompanionViewManager companionViewManager, ICardHolderManager cardHolderManager, ICardMovementController cardMovementController, IDuelSceneStateManager duelSceneStateManager, IGameStatePlaybackController gameStatePlaybackController, ILifeTotalController lifeTotalController, IBrowserManager browserManager, IGameStartController gameStartController, IPromptTextManager promptTextManager, IZoneCountManager zoneCountManager, IPlayerFocusController playerFocusController, ICardDissolveController cardDissolveController, IAvatarHoverController avatarHoverController, ICardHoverController cardHoverController, IHighlightManager highlightManager, IDimmingController dimmingController, ICanvasRootProvider canvasRootProvider, IPlayerNumericAidController playerNumericAidController)
	{
		IStaticListLocProvider staticListLocProvider = new StaticListLocProvider(CardDatabase.GreLocProvider);
		ResolutionEffectController value = new ResolutionEffectController();
		IGameEffectBuilder gameEffectBuilder = new GameEffectBuilder(viewManager, cardHolderManager);
		IRevealedCardsManager value2 = new RevealedCardsManager(new MutableRevealedCardsProvider(), CardDatabase, gameStateManager, cardHolderManager);
		Dictionary<Type, object> dictionary = new Dictionary<Type, object>();
		Type typeFromHandle = typeof(IGameStateManager);
		dictionary[typeFromHandle] = gameStateManager;
		Type typeFromHandle2 = typeof(IGameStateProvider);
		dictionary[typeFromHandle2] = gameStateManager;
		Type typeFromHandle3 = typeof(IGameStateController);
		dictionary[typeFromHandle3] = gameStateManager;
		Type typeFromHandle4 = typeof(IWorkflowProvider);
		dictionary[typeFromHandle4] = workflowProvider;
		Type typeFromHandle5 = typeof(IClickableWorkflowProvider);
		dictionary[typeFromHandle5] = clickableWorkflowProvider;
		Type typeFromHandle6 = typeof(IGameplaySettingsProvider);
		dictionary[typeFromHandle6] = AutoRespManager;
		Type typeFromHandle7 = typeof(IReferenceMap);
		dictionary[typeFromHandle7] = ReferenceMapAggregate;
		Type typeFromHandle8 = typeof(UXEventQueue);
		dictionary[typeFromHandle8] = UXEventQueue;
		Type typeFromHandle9 = typeof(ICardBuilder<DuelScene_CDC>);
		dictionary[typeFromHandle9] = dsCardBuilder;
		Type typeFromHandle10 = typeof(IEntityViewManager);
		dictionary[typeFromHandle10] = viewManager;
		Type typeFromHandle11 = typeof(IEntityViewProvider);
		dictionary[typeFromHandle11] = viewManager;
		Type typeFromHandle12 = typeof(ICardViewProvider);
		dictionary[typeFromHandle12] = viewManager;
		Type typeFromHandle13 = typeof(ICardViewController);
		dictionary[typeFromHandle13] = viewManager;
		Type typeFromHandle14 = typeof(ICardViewManager);
		dictionary[typeFromHandle14] = viewManager;
		Type typeFromHandle15 = typeof(IFakeCardViewProvider);
		dictionary[typeFromHandle15] = viewManager;
		Type typeFromHandle16 = typeof(IFakeCardViewController);
		dictionary[typeFromHandle16] = viewManager;
		Type typeFromHandle17 = typeof(IFakeCardViewManager);
		dictionary[typeFromHandle17] = viewManager;
		Type typeFromHandle18 = typeof(IAvatarViewProvider);
		dictionary[typeFromHandle18] = viewManager;
		Type typeFromHandle19 = typeof(IAvatarViewController);
		dictionary[typeFromHandle19] = viewManager;
		Type typeFromHandle20 = typeof(IAvatarViewManager);
		dictionary[typeFromHandle20] = viewManager;
		Type typeFromHandle21 = typeof(IRevealedCardsProvider);
		dictionary[typeFromHandle21] = value2;
		Type typeFromHandle22 = typeof(IRevealedCardsController);
		dictionary[typeFromHandle22] = value2;
		Type typeFromHandle23 = typeof(IRevealedCardsManager);
		dictionary[typeFromHandle23] = value2;
		Type typeFromHandle24 = typeof(IEmoteManager);
		dictionary[typeFromHandle24] = emoteManager;
		Type typeFromHandle25 = typeof(IEmoteControllerProvider);
		dictionary[typeFromHandle25] = emoteManager;
		Type typeFromHandle26 = typeof(IEntityDialogControllerProvider);
		dictionary[typeFromHandle26] = emoteManager;
		Type typeFromHandle27 = typeof(ICompanionViewManager);
		dictionary[typeFromHandle27] = companionViewManager;
		Type typeFromHandle28 = typeof(ICompanionViewProvider);
		dictionary[typeFromHandle28] = companionViewManager;
		Type typeFromHandle29 = typeof(ICompanionViewController);
		dictionary[typeFromHandle29] = companionViewManager;
		Type typeFromHandle30 = typeof(IEntityNameProvider<uint>);
		dictionary[typeFromHandle30] = idNameProvider;
		Type typeFromHandle31 = typeof(IEntityNameProvider<MtgEntity>);
		dictionary[typeFromHandle31] = entityNameProvider;
		Type typeFromHandle32 = typeof(IEntityNameProvider<MtgPlayer>);
		dictionary[typeFromHandle32] = playerNameProvider;
		Type typeFromHandle33 = typeof(IEntityNameProvider<MtgCardInstance>);
		dictionary[typeFromHandle33] = cardNameProvider;
		Type typeFromHandle34 = typeof(ICardHolderProvider);
		dictionary[typeFromHandle34] = cardHolderManager;
		Type typeFromHandle35 = typeof(ICardHolderController);
		dictionary[typeFromHandle35] = cardHolderManager;
		Type typeFromHandle36 = typeof(ICardHolderManager);
		dictionary[typeFromHandle36] = cardHolderManager;
		Type typeFromHandle37 = typeof(IGameEffectController);
		dictionary[typeFromHandle37] = cardHolderManager;
		Type typeFromHandle38 = typeof(ICardMovementController);
		dictionary[typeFromHandle38] = cardMovementController;
		Type typeFromHandle39 = typeof(IStaticListLocProvider);
		dictionary[typeFromHandle39] = staticListLocProvider;
		Type typeFromHandle40 = typeof(IBrowserProvider);
		dictionary[typeFromHandle40] = browserManager;
		Type typeFromHandle41 = typeof(IBrowserController);
		dictionary[typeFromHandle41] = browserManager;
		Type typeFromHandle42 = typeof(IBrowserManager);
		dictionary[typeFromHandle42] = browserManager;
		Type typeFromHandle43 = typeof(IZoneCountManager);
		dictionary[typeFromHandle43] = zoneCountManager;
		Type typeFromHandle44 = typeof(IZoneCountController);
		dictionary[typeFromHandle44] = zoneCountManager;
		Type typeFromHandle45 = typeof(IZoneCountProvider);
		dictionary[typeFromHandle45] = zoneCountManager;
		Type typeFromHandle46 = typeof(IHighlightProvider);
		dictionary[typeFromHandle46] = highlightManager;
		Type typeFromHandle47 = typeof(IHighlightController);
		dictionary[typeFromHandle47] = highlightManager;
		Type typeFromHandle48 = typeof(IHighlightManager);
		dictionary[typeFromHandle48] = highlightManager;
		Type typeFromHandle49 = typeof(IDimmingController);
		dictionary[typeFromHandle49] = dimmingController;
		Type typeFromHandle50 = typeof(IGameStartController);
		dictionary[typeFromHandle50] = gameStartController;
		Type typeFromHandle51 = typeof(IAvatarHoverController);
		dictionary[typeFromHandle51] = avatarHoverController;
		Type typeFromHandle52 = typeof(ICardHoverController);
		dictionary[typeFromHandle52] = cardHoverController;
		Type typeFromHandle53 = typeof(ICanvasRootProvider);
		dictionary[typeFromHandle53] = canvasRootProvider;
		Type typeFromHandle54 = typeof(IActionEffectController);
		dictionary[typeFromHandle54] = new ActionEffectController(new LimboActionEffectController(CardDatabase, gameStateManager, gameEffectBuilder), new SideboardActionEffectController(CardDatabase.CardDataProvider, gameStateManager, cardHolderManager, dsCardBuilder, LocManager, AssetLookupSystem));
		Type typeFromHandle55 = typeof(IPendingEffectController);
		dictionary[typeFromHandle55] = new PendingEffectController(CardDatabase, CardDatabase.ClientLocProvider, dsCardBuilder, cardHolderManager, AssetLookupSystem);
		Type typeFromHandle56 = typeof(IDelayedTriggerController);
		dictionary[typeFromHandle56] = new DelayedTriggerController(CardDatabase, viewManager, cardHolderManager, AssetLookupSystem);
		Type typeFromHandle57 = typeof(IDesignationController);
		dictionary[typeFromHandle57] = new DesignationController(CardDatabase.CardDataProvider, gameStateManager, gameEffectBuilder, viewManager, PromptEngine, UIManager.PlayerNames, AssetLookupSystem, VfxProvider);
		Type typeFromHandle58 = typeof(IQualificationController);
		dictionary[typeFromHandle58] = new QualificationController(CardDatabase, LocManager, viewManager, gameEffectBuilder, gameStateManager, AssetLookupSystem);
		Type typeFromHandle59 = typeof(IGamewideCountController);
		dictionary[typeFromHandle59] = new GamewideCountController(CardDatabase, gameStateManager, gameEffectBuilder);
		Type typeFromHandle60 = typeof(ISuspendLikeController);
		dictionary[typeFromHandle60] = new SuspendLikeController(CardDatabase, gameStateManager, gameEffectBuilder);
		Type typeFromHandle61 = typeof(IReplacementEffectController);
		dictionary[typeFromHandle61] = new ReplacementEffectController(CardDatabase, gameStateManager, gameEffectBuilder, entityNameProvider);
		Type typeFromHandle62 = typeof(ITurnController);
		dictionary[typeFromHandle62] = turnController;
		Type typeFromHandle63 = typeof(ITurnInfoProvider);
		dictionary[typeFromHandle63] = turnController;
		Type typeFromHandle64 = typeof(IExtraTurnRenderer);
		dictionary[typeFromHandle64] = new ExtraTurnRenderer(viewManager, cardHolderManager, LocManager);
		Type typeFromHandle65 = typeof(IPlayerAbilityController);
		dictionary[typeFromHandle65] = new PlayerAbilityController(CardDatabase, gameStateManager, gameEffectBuilder, AssetLookupSystem);
		Type typeFromHandle66 = typeof(IPlayerPresenceController);
		dictionary[typeFromHandle66] = new PlayerPresenceController(viewManager, cardHolderManager);
		Type typeFromHandle67 = typeof(IFaceDownIdProvider);
		dictionary[typeFromHandle67] = new FaceDownIdProvider(gameStateManager);
		Type typeFromHandle68 = typeof(TimerManager);
		dictionary[typeFromHandle68] = TimerManager;
		Type typeFromHandle69 = typeof(IVfxProvider);
		dictionary[typeFromHandle69] = vfxProvider;
		Type typeFromHandle70 = typeof(IResolutionEffectController);
		dictionary[typeFromHandle70] = value;
		Type typeFromHandle71 = typeof(IResolutionEffectProvider);
		dictionary[typeFromHandle71] = value;
		Type typeFromHandle72 = typeof(IActionProvider);
		dictionary[typeFromHandle72] = new ActionProvider(gameStateManager, workflowProvider);
		Type typeFromHandle73 = typeof(IPlayerSpriteProvider);
		dictionary[typeFromHandle73] = new PlayerSpriteProvider(AssetLookupSystem, gameStateManager, MatchManager);
		Type typeFromHandle74 = typeof(IAvatarInputController);
		dictionary[typeFromHandle74] = IAvatarInputController.Create(emoteManager, cardHolderManager, avatarHoverController, clickableWorkflowProvider, UIManager.FullControl);
		Type typeFromHandle75 = typeof(IDuelSceneStateManager);
		dictionary[typeFromHandle75] = duelSceneStateManager;
		Type typeFromHandle76 = typeof(IDuelSceneStateProvider);
		dictionary[typeFromHandle76] = duelSceneStateManager;
		Type typeFromHandle77 = typeof(IDuelSceneStateController);
		dictionary[typeFromHandle77] = duelSceneStateManager;
		Type typeFromHandle78 = typeof(IGameStatePlaybackController);
		dictionary[typeFromHandle78] = gameStatePlaybackController;
		Type typeFromHandle79 = typeof(ILifeTotalController);
		dictionary[typeFromHandle79] = lifeTotalController;
		Type typeFromHandle80 = typeof(IPromptTextProvider);
		dictionary[typeFromHandle80] = promptTextManager;
		Type typeFromHandle81 = typeof(IPromptTextController);
		dictionary[typeFromHandle81] = promptTextManager;
		Type typeFromHandle82 = typeof(IPromptTextManager);
		dictionary[typeFromHandle82] = promptTextManager;
		Type typeFromHandle83 = typeof(IModalBrowserCardHeaderProvider);
		dictionary[typeFromHandle83] = new ModalBrowserCardHeaderProvider(CardDatabase, staticListLocProvider, ModalBrowserCardHeaderProvider.DefaultSubProviders(CardDatabase, gameStateManager, entityNameProvider, AssetLookupSystem));
		Type typeFromHandle84 = typeof(IAutoSubmitActionCalculator);
		dictionary[typeFromHandle84] = new AutoSubmitActionCalculatorAggregate(new FreeCastAutoSubmitCalculator(GenericPool, AutoRespManager), new IgnorableInactiveActionsCalculator(GenericPool, AutoRespManager, CardDatabase.AbilityDataProvider));
		Type typeFromHandle85 = typeof(IListFilter<GreInteraction>);
		dictionary[typeFromHandle85] = new ActionFilterAggregate(new IrrelevantActionFilter(), new FreeCastActionFilter(AutoRespManager));
		Type typeFromHandle86 = typeof(IPlayerFocusController);
		dictionary[typeFromHandle86] = playerFocusController;
		Type typeFromHandle87 = typeof(IBrowserHeaderTextProvider);
		dictionary[typeFromHandle87] = new BrowserHeaderTextProvider(AssetLookupSystem, CardDatabase.ClientLocProvider, promptTextManager);
		Type typeFromHandle88 = typeof(ICardDissolveController);
		dictionary[typeFromHandle88] = cardDissolveController;
		Type typeFromHandle89 = typeof(IPlayerNumericAidController);
		dictionary[typeFromHandle89] = playerNumericAidController;
		return dictionary;
	}

	private IWorkflowTranslator WorkflowTranslator()
	{
		IWorkflowTranslator nestedTranslation;
		if (MatchManager.LocalPlayerStrategy != null)
		{
			nestedTranslation = new WorkflowTranslator_BotBattle(Context, MatchManager.LocalPlayerStrategy, AssetLookupSystem);
		}
		else if (SessionType == GameSessionType.Replay)
		{
			nestedTranslation = new WorkflowTranslator_Replay(Context, AssetLookupSystem);
		}
		else if (_npeDirector != null)
		{
			IWorkflowTranslation<SelectNRequest> selectNTranslation = new SelectNWorkflowTranslation(new NPESelectionWorkflowTranslation(Context, AssetLookupSystem, _npeDirector), Context, AssetLookupSystem, this);
			IWorkflowTranslation<ActionsAvailableRequest> actionsAvailableTranslation = new ActionsAvailableTranslation(Context, AssetLookupSystem, this, Logger, new NPEActionsAvailableWorkflowTranslation(this, Context, _npeDirector));
			IWorkflowTranslation<AssignDamageRequest> assignDamageTranslation = new AutoAssignDamageTranslation();
			IWorkflowTranslation<DeclareBlockersRequest> declareBlockersTranslation = new DeclareBlockersTranslation_NPE(_npeDirector, Context, UIManager);
			IWorkflowTranslation<DeclareAttackerRequest> declareAttackersTranslation = new DeclareAttackersTranslation_NPE(_npeDirector, Context, UIManager, this);
			IWorkflowTranslation<SelectTargetsRequest> selectTargetsTranslation = new SelectTargetsTranslation_NPE(Context, AssetLookupSystem, _npeDirector);
			nestedTranslation = new WorkflowTranslator(this, Context, AssetLookupSystem, selectNTranslation, actionsAvailableTranslation, assignDamageTranslation, declareBlockersTranslation, declareAttackersTranslation, selectTargetsTranslation);
		}
		else
		{
			IWorkflowTranslation<SelectNRequest> selectNTranslation2 = new SelectNWorkflowTranslation(new SelectionWorkflowTranslation(Context, AssetLookupSystem), Context, AssetLookupSystem, this);
			IWorkflowTranslation<ActionsAvailableRequest> actionsAvailableTranslation2 = new ActionsAvailableTranslation(Context, AssetLookupSystem, this, Logger, new DefaultTranslation(this, Context));
			IWorkflowTranslation<AssignDamageRequest> assignDamageTranslation2 = new AssignDamageTranslation(Context);
			IWorkflowTranslation<DeclareBlockersRequest> declareBlockersTranslation2 = new DeclareBlockersTranslation(Context, UIManager);
			IWorkflowTranslation<DeclareAttackerRequest> declareAttackersTranslation2 = new DeclareAttackersTranslation(Context, UIManager, this);
			IWorkflowTranslation<SelectTargetsRequest> selectTargetsTranslation2 = new SelectTargetsTranslation(Context, new AutoTargetingSolution(Context), AssetLookupSystem);
			nestedTranslation = new WorkflowTranslator(this, Context, AssetLookupSystem, selectNTranslation2, actionsAvailableTranslation2, assignDamageTranslation2, declareBlockersTranslation2, declareAttackersTranslation2, selectTargetsTranslation2);
		}
		nestedTranslation = new InformationalWorkflowDecorator(NullWorkflowTranslation<BaseUserRequest>.Default, nestedTranslation);
		if (TimedReplayPlayer.IsReplayAvailable())
		{
			WorkflowTranslator_TimedReplay workflowTranslator_TimedReplay = new WorkflowTranslator_TimedReplay(nestedTranslation);
			nestedTranslation = workflowTranslator_TimedReplay;
			TimedReplayPlayer.Create(MatchManager, MatchSceneManager, _npeController, workflowTranslator_TimedReplay);
		}
		if (HasDebugPermissions())
		{
			nestedTranslation = new WorkflowSourceLogger(nestedTranslation, Context.Get<IEntityNameProvider<uint>>());
		}
		return nestedTranslation;
	}

	private void RefreshTimerLayout()
	{
		UIManager.BattleFieldStaticElementsLayout.StartCoroutine(UIManager.BattleFieldStaticElementsLayout.UpdateTimerPositions(TimerManager));
	}

	private void OnDestroy()
	{
		foreach (IDisposable disposable in _disposables)
		{
			disposable.Dispose();
		}
		_disposables.Clear();
		_updateObservers.Clear();
		BrowserManager?.Clear();
		if ((bool)_npeController)
		{
			AudioManager.ExecuteActionOnEvent(WwiseEvents.sfx_npe_intro_blackspace.EventName, AkActionOnEventType.AkActionOnEventType_Stop, _npeController.gameObject);
			UnityEngine.Object.Destroy(_npeController);
		}
		if ((bool)MainCamera)
		{
			MainCamera.enabled = false;
		}
		OpeningHandActionWorkflow.ClearPendingActions();
		RigidbodyWind.ToggleAll(enabled: false);
		if (MatchManager != null)
		{
			MatchManager.SideboardSubmitted -= OnSideboardSubmitted;
		}
		if (AudioManager.Instance != null)
		{
			AudioManager.PostEvent("StopAllDSSounds", AudioManager.Default);
		}
		if (ScreenEventController.Instance != null)
		{
			ScreenEventController.Instance.OnScreenChanged -= RefreshTimerLayout;
		}
		if (AssetLookupSystem.Blackboard != null)
		{
			AssetLookupSystem.Blackboard.RemoveFillerDelegate(FillAltBlackboard);
			AssetLookupSystem = default(AssetLookupSystem);
		}
		KeyboardManager.Unsubscribe(this);
		ScreenKeepAlive.AllowScreenTimeout();
		BrowserManager = null;
		_npeController = null;
		_npeDirector = null;
		ViewManager = null;
		_uxEventTranslator = null;
		WorkflowController = null;
		_intentionLineManager = null;
		InteractionSystem = null;
		UXEventQueue = null;
		_altArtMatchSettings = null;
		AutoRespManager = null;
	}

	private void Undo()
	{
		if (!_settingsMenuHost.IsOpen() && !SocialUI.IsSendFriendInviteShowing() && !BrowserManager.IsAnyBrowserOpen)
		{
			WorkflowController.CurrentWorkflow?.TryUndo();
		}
	}

	public void TogglePhaseLadder()
	{
		if (!SocialUI.IsSendFriendInviteShowing())
		{
			MDNPlayerPrefs.ShowPhaseLadder = !MDNPlayerPrefs.ShowPhaseLadder;
			MDNPlayerPrefs.SeenPhaseLadderHint = true;
			UIManager.PhaseLadder.OnEnable();
		}
	}

	private void OnEscapePressed()
	{
		bool flag = WorkflowController.PendingWorkflow is IntermissionWorkflow || WorkflowController.CurrentWorkflow is IntermissionWorkflow;
		if (_settingsMenuHost.IsOpen())
		{
			_settingsMenuHost.Close();
		}
		else if (UIManager.ManaColorSelector.IsOpen)
		{
			UIManager.ManaColorSelector.TryCloseSelector();
		}
		else if (UIManager.ConfirmWidget.IsOpen)
		{
			UIManager.ConfirmWidget.Cancel();
		}
		else if (!flag && !_sideboardSubmitted)
		{
			InteractionSystem.CancelAnyDrag();
			_settingsMenuHost.Open();
		}
	}

	private void Update()
	{
		if (MatchManager == null || _gre == null)
		{
			return;
		}
		foreach (ClientUpdateBase item in _gre.RetrieveClientUpdates())
		{
			Update_LoggerOnClientUpdate(item);
			if (item is GreClient.Rules.GameStateUpdate { NewState: var newState } gameStateUpdate)
			{
				Update_NPEDirector(gameStateUpdate);
				Update_ReferenceMap(gameStateUpdate);
				Update_Timers(newState);
				_gameStateManager.SetLatestGameState(newState);
				GenerateGameStateUxEvents(gameStateUpdate);
				Update_ProcessInteraction(gameStateUpdate);
			}
			else if (item is EdictalUpdate)
			{
				HandleAutoRespond();
			}
			else if (SessionType == GameSessionType.Game)
			{
				if (item is SettingsNotification settings)
				{
					Update_AutoRespSettings(settings);
				}
				else if (item is UIMessageNotification messageNotification)
				{
					Update_MessageNotification(messageNotification);
				}
				else if (item is TimeoutNotification tn)
				{
					Update_TimerNotification(tn);
				}
				else if (item is TimerUpdate tu)
				{
					Update_TimerUpdate(tu);
				}
				else if (item is PromptNotification promptNotification)
				{
					Update_PromptNotification(promptNotification);
				}
			}
		}
		Update_UpdateObservers(Time.deltaTime);
		bool wasRunning = Update_UXEventQueue();
		Update_WorkflowController();
		Update_HighlightsAndDimming(wasRunning);
		Update_InteractionSystemVisualState();
		Update_SplineMovement();
		Update_IntentionLineManager();
		Update_ProcessInteraction();
	}

	private void Update_NPEDirector(GreClient.Rules.GameStateUpdate gsUpdate)
	{
		if (_npeDirector != null)
		{
			_npeDirector.React_GameStateUpdate(gsUpdate, ViewManager);
		}
	}

	private void Update_ReferenceMap(GreClient.Rules.GameStateUpdate gsUpdate)
	{
		ReferenceMapAggregate.AddReferenceMap(gsUpdate);
	}

	private void Update_ProcessInteraction(GreClient.Rules.GameStateUpdate gsUpdate)
	{
		BaseUserRequest interaction = gsUpdate.Interaction;
		_promptTextManager.SetPrompt((interaction != null) ? null : gsUpdate.Prompt);
		if (interaction != null)
		{
			if (_npeDirector != null)
			{
				interaction.OnSubmit = (Action<ClientToGREMessage>)Delegate.Combine(interaction.OnSubmit, new Action<ClientToGREMessage>(_npeDirector.React_OutgoingMessage));
				_npeDirector.React_IncomingRequest(interaction);
			}
			if (interaction is IntermissionRequest)
			{
				WorkflowController?.CleanUpCurrentWorkflow();
			}
			WorkflowController?.EnqueueRequest(interaction);
		}
	}

	private void Update_LoggerOnClientUpdate(ClientUpdateBase clientUpdate)
	{
		Logger?.OnClientUpdate(clientUpdate);
	}

	private void Update_UpdateObservers(float timeStep)
	{
		foreach (IUpdate updateObserver in _updateObservers)
		{
			updateObserver.OnUpdate(timeStep);
		}
	}

	private void Update_Timers(MtgGameState gameState)
	{
		TimerManager.UpdateTimers(gameState.LocalPlayer, gameState.Opponent, gameState.GameInfo);
	}

	private void Update_AutoRespSettings(SettingsNotification settings)
	{
		_settingsMessageController.SetSettings(settings.Settings);
		AutoRespManager.UpdateSettings(settings.Settings);
	}

	private void Update_MessageNotification(UIMessageNotification messageNotification)
	{
		UIMessageHandler.OnUIMessage(messageNotification.Message);
	}

	private void Update_TimerNotification(TimeoutNotification tn)
	{
		TimerManager.DisplayTimeoutNotification(tn.UpdatedTimer, tn.TriggeredByLocaPlayer, tn.CurrentTimeoutCountForPlayer);
	}

	private void Update_TimerUpdate(TimerUpdate tu)
	{
		TimerManager.UpdateTimer(tu.UpdatedPlayer);
	}

	private void Update_PromptNotification(PromptNotification promptNotification)
	{
		Prompt prompt = promptNotification.Prompt;
		if (!PromptTextProvider.ShouldSkipPrompt(prompt))
		{
			UXEventQueue.EnqueuePending(new GrePromptUXEvent(prompt, Context, AssetLookupSystem));
		}
	}

	private bool Update_UXEventQueue()
	{
		bool isRunning = UXEventQueue.IsRunning;
		if (_npeDirector == null || !_npeDirector.ShouldBePaused)
		{
			UXEventQueue.Update(Time.deltaTime);
		}
		return isRunning;
	}

	private void Update_WorkflowController()
	{
		WorkflowController?.Update(UXEventQueue.Events);
	}

	private void Update_HighlightsAndDimming(bool wasRunning)
	{
		if (wasRunning && !UXEventQueue.IsRunning)
		{
			_highlightManager.SetDirty();
			_dimmingController.SetDirty();
		}
	}

	private void Update_InteractionSystemVisualState()
	{
		IEnumerable<DuelScene_CDC> cardViews = _visualStateCardProvider.GetCardViews();
		_combatIconUpdater.UpdateCombatIcons(cardViews);
		_highlightManager.UpdateHighlights(cardViews, ViewManager.GetAllAvatars());
		_dimmingController.UpdateDimming(cardViews);
		Context.Get<IPlayerPresenceController>()?.Update(cardViews);
	}

	private void Update_SplineMovement()
	{
		SplineMovementSystem?.UpdateMovement();
	}

	private void Update_IntentionLineManager()
	{
		_intentionLineManager?.Update();
	}

	private void Update_ProcessInteraction()
	{
		InteractionSystem?.ProcessInteraction();
	}

	private void GenerateGameStateUxEvents(GreClient.Rules.GameStateUpdate gameStateUpdate)
	{
		IReadOnlyList<UXEvent> evts = _uxEventTranslator.GenerateEvents(gameStateUpdate);
		UXEventQueue.EnqueuePending(evts);
	}

	private void HandleAutoRespond()
	{
		if (CurrentInteraction != null)
		{
			WorkflowController.CleanUpCurrentWorkflow();
		}
		if (UIManager.ManaColorSelector.IsOpen)
		{
			UIManager.ManaColorSelector.CloseSelector();
		}
		while (BrowserManager.IsAnyBrowserOpen)
		{
			BrowserManager.CloseCurrentBrowser();
		}
	}

	public MatchManager.PlayerInfo GetPlayerInfoForNum(GREPlayerNum playerNum)
	{
		return MatchManager.PlayerInfoForNum(playerNum);
	}

	private void OnSideboardSubmitted()
	{
		_sideboardSubmitted = true;
	}

	public bool TryGetMainButton(out StyledButton button)
	{
		button = UIManager.GetButtonWithStyle(ButtonStyle.StyleType.Main);
		if (button == null)
		{
			button = UIManager.GetButtonWithTag(ButtonTag.Primary);
		}
		return button != null;
	}

	public bool HandleKeyUp(KeyCode curr, Modifiers mods)
	{
		if (!AllowInput)
		{
			return true;
		}
		if (!_settingsMenuHost.IsOpen() && !SocialUI.IsSendFriendInviteShowing())
		{
			if (CurrentInteraction.CanKeyUp(curr))
			{
				if (CurrentInteraction.AppliedState == InteractionAppliedState.Applied)
				{
					CurrentInteraction.OnKeyUp(curr);
				}
			}
			else if (curr == KeyCode.Space && _npeDirector == null && !BrowserManager.IsAnyBrowserOpen)
			{
				if (TryGetMainButton(out var button))
				{
					button.SimulateClickRelease();
				}
			}
			else if (curr == KeyCode.Return)
			{
				AutoRespManager.ToggleAutoPass();
			}
			else if (curr == KeyCode.L && _npeDirector == null)
			{
				TogglePhaseLadder();
			}
			else if (curr == KeyCode.Z && _npeDirector == null)
			{
				Undo();
			}
			else if (curr == KeyCode.Tab && HasDebugPermissions())
			{
				CurrentInteraction?.BaseRequest?.AutoRespond();
			}
		}
		return true;
	}

	public bool HandleKeyDown(KeyCode curr, Modifiers mods)
	{
		if (curr == KeyCode.Escape)
		{
			OnEscapePressed();
		}
		else if (AllowInput && !_settingsMenuHost.IsOpen() && !SocialUI.IsSendFriendInviteShowing())
		{
			if (CurrentInteraction.CanKeyDown(curr))
			{
				CurrentInteraction.OnKeyDown(curr);
			}
			else if (curr == KeyCode.Space && _npeDirector == null && !BrowserManager.IsAnyBrowserOpen)
			{
				StyledButton styledButton = UIManager.GetButtonWithStyle(ButtonStyle.StyleType.Main);
				if (styledButton == null)
				{
					styledButton = UIManager.GetButtonWithTag(ButtonTag.Primary);
				}
				if (styledButton != null && curr == KeyCode.Space)
				{
					styledButton.SimulateClickDown();
				}
			}
			else
			{
				switch (curr)
				{
				case KeyCode.RightControl:
				case KeyCode.LeftControl:
					if (mods.Shift)
					{
						AutoRespManager.ToggleLockedFullControl();
					}
					else
					{
						AutoRespManager.ToggleFullControl();
					}
					break;
				case KeyCode.RightShift:
				case KeyCode.LeftShift:
					if (mods.Ctrl)
					{
						AutoRespManager.ToggleLockedFullControl();
					}
					break;
				}
			}
		}
		return true;
	}

	public bool HandleKeyHeld(KeyCode key, float totalMS)
	{
		if (AllowInput && !_settingsMenuHost.IsOpen() && !SocialUI.IsSendFriendInviteShowing() && CurrentInteraction.CanKeyHeld(key, totalMS))
		{
			CurrentInteraction.OnKeyHeld(key, totalMS);
		}
		return true;
	}

	private void FillAltBlackboard(IBlackboard bb)
	{
		bb.GameState = CurrentGameState;
		bb.ActiveResolution = ActiveResolutionEffect;
		bb.BattlefieldId = BattlefieldUtil.BattlefieldId;
		bb.Language = Languages.CurrentLanguage;
		bb.InWrapper = false;
		bb.InDuelScene = true;
		WorkflowBase workflowBase = CurrentInteraction ?? PendingInteraction;
		if (workflowBase != null)
		{
			bb.Interaction = workflowBase;
			bb.Request = workflowBase.BaseRequest;
		}
		if (BrowserManager?.CurrentBrowser is ICardBrowser cardBrowser)
		{
			bb.CardBrowserType = cardBrowser.BrowserType;
			bb.CardBrowserLayoutID = cardBrowser.CardHolderLayoutKey;
		}
		bb.Prompt = bb.Interaction?.Prompt;
		bb.IdNameProvider = Context.Get<IEntityNameProvider<uint>>();
	}

	private bool HasDebugPermissions()
	{
		bool flag = Context.Get<IAccountClient>().AccountInformation?.HasRole_Debugging() ?? false;
		return Debug.isDebugBuild || flag;
	}

	private bool CanDisplayDebugUI()
	{
		if (!HasDebugPermissions())
		{
			return OverridesConfiguration.Local.GetFeatureToggleValue("debug.showSparkyMenu");
		}
		return true;
	}
}
