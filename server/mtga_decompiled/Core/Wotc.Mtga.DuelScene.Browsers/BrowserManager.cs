using System;
using System.Collections.Generic;
using MTGA.KeyboardManager;
using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Browsers;

public class BrowserManager : IBrowserManager, IBrowserProvider, IBrowserController, IKeyUpSubscriber, IKeySubscriber, IKeyDownSubscriber, IUpdate
{
	private class BrowserFactory
	{
		private readonly IReadOnlyDictionary<DuelSceneBrowserType, Func<BrowserManager, GameManager, IDuelSceneBrowserProvider, ICardBuilder<DuelScene_CDC>, BrowserBase>> _funcMap = new Dictionary<DuelSceneBrowserType, Func<BrowserManager, GameManager, IDuelSceneBrowserProvider, ICardBuilder<DuelScene_CDC>, BrowserBase>>
		{
			{
				DuelSceneBrowserType.Mulligan,
				(BrowserManager controller, GameManager gameManager2, IDuelSceneBrowserProvider provider, ICardBuilder<DuelScene_CDC> _) => new MulliganBrowser(controller, provider, gameManager2)
			},
			{
				DuelSceneBrowserType.Scry,
				(BrowserManager controller, GameManager gameManager2, IDuelSceneBrowserProvider provider, ICardBuilder<DuelScene_CDC> cardBuilder2) => new ScryBrowser(controller, provider, gameManager2, cardBuilder2)
			},
			{
				DuelSceneBrowserType.ViewDismiss,
				(BrowserManager controller, GameManager gameManager2, IDuelSceneBrowserProvider provider, ICardBuilder<DuelScene_CDC> _) => new ViewDismissBrowser(controller, provider, gameManager2)
			},
			{
				DuelSceneBrowserType.Split,
				(BrowserManager controller, GameManager gameManager2, IDuelSceneBrowserProvider provider, ICardBuilder<DuelScene_CDC> _) => new SplitCardsBrowser(controller, provider, gameManager2)
			},
			{
				DuelSceneBrowserType.SelectGroup,
				(BrowserManager controller, GameManager gameManager2, IDuelSceneBrowserProvider provider, ICardBuilder<DuelScene_CDC> _) => new SelectGroupBrowser(controller, provider, gameManager2)
			},
			{
				DuelSceneBrowserType.OptionalAction,
				(BrowserManager controller, GameManager gameManager2, IDuelSceneBrowserProvider provider, ICardBuilder<DuelScene_CDC> _) => new OptionalActionBrowser(controller, provider, gameManager2)
			},
			{
				DuelSceneBrowserType.Riot,
				(BrowserManager controller, GameManager gameManager2, IDuelSceneBrowserProvider provider, ICardBuilder<DuelScene_CDC> _) => new OptionalActionBrowser(controller, provider, gameManager2)
			},
			{
				DuelSceneBrowserType.Order,
				(BrowserManager controller, GameManager gameManager2, IDuelSceneBrowserProvider provider, ICardBuilder<DuelScene_CDC> cardBuilder2) => new OrderCardsBrowser(controller, provider, gameManager2, cardBuilder2)
			},
			{
				DuelSceneBrowserType.TriggerOrder,
				(BrowserManager controller, GameManager gameManager2, IDuelSceneBrowserProvider provider, ICardBuilder<DuelScene_CDC> cardBuilder2) => new OrderCardsBrowser(controller, provider, gameManager2, cardBuilder2)
			},
			{
				DuelSceneBrowserType.SelectCards,
				(BrowserManager controller, GameManager gameManager2, IDuelSceneBrowserProvider provider, ICardBuilder<DuelScene_CDC> _) => new SelectCardsBrowser(controller, provider, gameManager2.Context.Get<IHighlightController>(), gameManager2.Context.Get<IDimmingController>(), gameManager2)
			},
			{
				DuelSceneBrowserType.SelectCardsMultiZone,
				(BrowserManager controller, GameManager gameManager2, IDuelSceneBrowserProvider provider, ICardBuilder<DuelScene_CDC> _) => new SelectCardsBrowser_MultiZone(controller, provider, gameManager2.Context.Get<IHighlightController>(), gameManager2.Context.Get<IDimmingController>(), gameManager2)
			},
			{
				DuelSceneBrowserType.Surveil,
				(BrowserManager controller, GameManager gameManager2, IDuelSceneBrowserProvider provider, ICardBuilder<DuelScene_CDC> _) => new SurveilBrowser(controller, provider, gameManager2)
			},
			{
				DuelSceneBrowserType.OpeningHand,
				(BrowserManager controller, GameManager gameManager2, IDuelSceneBrowserProvider provider, ICardBuilder<DuelScene_CDC> _) => new OpeningHandBrowser(controller, provider, gameManager2)
			},
			{
				DuelSceneBrowserType.KeywordSelection,
				(BrowserManager controller, GameManager gameManager2, IDuelSceneBrowserProvider provider, ICardBuilder<DuelScene_CDC> _) => new KeywordSelectionBrowser(controller, provider, gameManager2)
			},
			{
				DuelSceneBrowserType.Informational,
				(BrowserManager controller, GameManager gameManager2, IDuelSceneBrowserProvider provider, ICardBuilder<DuelScene_CDC> _) => new InformationalBrowser(controller, provider, gameManager2)
			},
			{
				DuelSceneBrowserType.RepeatSelection,
				(BrowserManager controller, GameManager gameManager2, IDuelSceneBrowserProvider provider, ICardBuilder<DuelScene_CDC> _) => new RepeatSelectionBrowser(controller, provider, gameManager2.Context.Get<IHighlightController>(), gameManager2.Context.Get<IDimmingController>(), gameManager2)
			},
			{
				DuelSceneBrowserType.ButtonScrollList,
				(BrowserManager controller, GameManager gameManager2, IDuelSceneBrowserProvider provider, ICardBuilder<DuelScene_CDC> _) => new ButtonScrollListBrowser(controller, provider, gameManager2)
			},
			{
				DuelSceneBrowserType.ButtonSelection,
				(BrowserManager controller, GameManager gameManager2, IDuelSceneBrowserProvider provider, ICardBuilder<DuelScene_CDC> cardBuilder2) => new ButtonSelectionBrowser(controller, provider, gameManager2, cardBuilder2)
			},
			{
				DuelSceneBrowserType.London,
				(BrowserManager controller, GameManager gameManager2, IDuelSceneBrowserProvider provider, ICardBuilder<DuelScene_CDC> cardBuilder2) => new LondonBrowser(controller, provider, gameManager2, cardBuilder2)
			},
			{
				DuelSceneBrowserType.KeywordSelectionWithContext,
				(BrowserManager controller, GameManager gameManager2, IDuelSceneBrowserProvider provider, ICardBuilder<DuelScene_CDC> _) => new SelectNKeywordWithContextBrowser(controller, provider, gameManager2)
			},
			{
				DuelSceneBrowserType.AttachmentAndExileStack,
				(BrowserManager controller, GameManager gameManager2, IDuelSceneBrowserProvider provider, ICardBuilder<DuelScene_CDC> _) => new AttachmentAndExileStackBrowser(controller, provider, gameManager2)
			},
			{
				DuelSceneBrowserType.MutateOptionalAction,
				(BrowserManager controller, GameManager gameManager2, IDuelSceneBrowserProvider provider, ICardBuilder<DuelScene_CDC> _) => new MutateOptionalActionBrowser(controller, provider, gameManager2)
			},
			{
				DuelSceneBrowserType.ColorSection,
				(BrowserManager controller, GameManager gameManager2, IDuelSceneBrowserProvider provider, ICardBuilder<DuelScene_CDC> _) => new ColorSelectionBrowser(controller, provider, gameManager2)
			},
			{
				DuelSceneBrowserType.SelectManaType,
				(BrowserManager controller, GameManager gameManager2, IDuelSceneBrowserProvider provider, ICardBuilder<DuelScene_CDC> _) => new SelectManaTypeBrowser(controller, provider, gameManager2)
			},
			{
				DuelSceneBrowserType.LibrarySideboard,
				(BrowserManager controller, GameManager gameManager2, IDuelSceneBrowserProvider provider, ICardBuilder<DuelScene_CDC> _) => new LibrarySideboardBrowser(controller, provider, gameManager2)
			},
			{
				DuelSceneBrowserType.DungeonRoomSelection,
				(BrowserManager controller, GameManager gameManager2, IDuelSceneBrowserProvider provider, ICardBuilder<DuelScene_CDC> _) => new DungeonRoomSelectBrowser(controller, provider, gameManager2)
			},
			{
				DuelSceneBrowserType.ReadAhead,
				(BrowserManager controller, GameManager gameManager2, IDuelSceneBrowserProvider provider, ICardBuilder<DuelScene_CDC> _) => new ReadAheadBrowser(controller, provider, gameManager2)
			},
			{
				DuelSceneBrowserType.Scryish,
				(BrowserManager controller, GameManager gameManager2, IDuelSceneBrowserProvider provider, ICardBuilder<DuelScene_CDC> cardBuilder2) => new ScryishBrowser(controller, provider, gameManager2, cardBuilder2)
			}
		};

		private readonly GameManager _gameManager;

		private readonly BrowserManager _manager;

		private readonly ICardBuilder<DuelScene_CDC> _cardBuilder;

		private readonly Transform _mainCameraTransform;

		public BrowserFactory(GameManager gameManager, BrowserManager manager, ICardBuilder<DuelScene_CDC> cardBuilder, Transform mainCameraTransform)
		{
			_gameManager = gameManager;
			_manager = manager;
			_cardBuilder = cardBuilder ?? NullCardBuilder<DuelScene_CDC>.Default;
			_mainCameraTransform = mainCameraTransform;
		}

		public BrowserBase Make(IDuelSceneBrowserProvider provider)
		{
			BrowserBase browserBase = ToBrowser(provider);
			if (browserBase != null)
			{
				return browserBase;
			}
			if (!_funcMap.TryGetValue(provider.GetBrowserType(), out var value))
			{
				throw new InvalidOperationException($"BrowserFactory cannot create browser type \"{provider.GetBrowserType()}\".");
			}
			if (value == null)
			{
				throw new NullReferenceException($"BrowserFactory contains an invalid factory function for \"{provider.GetBrowserType()}\".");
			}
			return value(_manager, _gameManager, provider, _cardBuilder);
		}

		private BrowserBase ToBrowser(IDuelSceneBrowserProvider provider)
		{
			if (!(provider is YesNoProvider provider2))
			{
				if (provider is AssignDamageProvider provider3)
				{
					return new AssignDamageBrowser(_mainCameraTransform, provider3, _manager, _gameManager);
				}
				return null;
			}
			return new YesNoBrowser(provider2, _manager, _gameManager);
		}
	}

	private const KeyCode ContinueKeyCode = KeyCode.Space;

	private readonly MutableBrowserProvider _provider;

	private readonly BrowserFactory _browserFactory;

	private readonly MatchManager _matchManager;

	private readonly CardDragController _dragController;

	private readonly KeyboardManager _keyboardManager;

	private readonly SettingsMenuHost _settingsMenuHost;

	private readonly IDuelSceneStateProvider _duelSceneStateProvider;

	private BrowserBase _workflowBrowser;

	private float _consecutiveBrowserTimer = 1f;

	private const float TimeBetweenBrowsers = 1f;

	public PriorityLevelEnum Priority => PriorityLevelEnum.DuelScene_PopUps;

	public BrowserBase CurrentBrowser => _provider.CurrentBrowser;

	public bool IsAnyBrowserOpen => CurrentBrowser != null;

	public bool IsBrowserVisible
	{
		get
		{
			if (IsAnyBrowserOpen)
			{
				return CurrentBrowser.IsVisible;
			}
			return false;
		}
	}

	public bool CanShowStack
	{
		get
		{
			if (CurrentBrowser != null)
			{
				return CurrentBrowser.CanShowStack();
			}
			return false;
		}
	}

	public Transform WorkflowBrowserRoot { get; private set; }

	public Transform WorkflowBrowserRootScaledCorrectly { get; private set; }

	public Transform ViewDismissRoot { get; private set; }

	public bool IsConsecutiveBrowser => _consecutiveBrowserTimer < 1f;

	public event Action<BrowserBase> BrowserOpened;

	public event Action<BrowserBase> BrowserShown;

	public event Action<BrowserBase> BrowserHidden;

	public event Action<BrowserBase> BrowserClosed;

	public BrowserManager(MutableBrowserProvider provider, MatchManager matchManager, IDuelSceneStateProvider duelSceneStateProvider, GameManager gameManager, CanvasManager canvasManager, CardDragController cardDragController, KeyboardManager keyboardManager, SettingsMenuHost settingsMenuHost, ICardBuilder<DuelScene_CDC> cardBuilder)
	{
		_provider = provider;
		_matchManager = matchManager;
		_duelSceneStateProvider = duelSceneStateProvider;
		WorkflowBrowserRoot = canvasManager.GetCanvasRoot(CanvasLayer.ScreenSpace_UnderStack, "WorkflowBrowser");
		WorkflowBrowserRootScaledCorrectly = canvasManager.GetCanvasRoot(CanvasLayer.ScreenSpace_UnderStack_ScaledCorrectly, "WorkflowBrowser");
		ViewDismissRoot = canvasManager.GetCanvasRoot(CanvasLayer.ScreenSpace_UnderStack_ScaledCorrectly, "ViewDismissBrowser");
		_dragController = cardDragController;
		_keyboardManager = keyboardManager;
		_matchManager.MatchStateChanged += OnMatchStateChanged;
		_matchManager.MatchCompleted += OnMatchCompleted;
		_settingsMenuHost = settingsMenuHost;
		_browserFactory = new BrowserFactory(gameManager, this, cardBuilder, gameManager.MainCamera.transform);
	}

	public bool HandleKeyUp(KeyCode curr, Modifiers mods)
	{
		if (_duelSceneStateProvider.AllowInput && CurrentBrowser.IsVisible)
		{
			if (curr == KeyCode.Space)
			{
				StyledButton mainButton = CurrentBrowser.GetMainButton();
				if (mainButton != null)
				{
					mainButton.SimulateClickRelease();
				}
				else
				{
					if (!(CurrentBrowser is SelectCardsBrowser selectCardsBrowser) || !selectCardsBrowser.AllowsKeyboardSelection())
					{
						return false;
					}
					selectCardsBrowser.MakeKeyboardSelection();
				}
			}
			else if (_workflowBrowser != null && _workflowBrowser == CurrentBrowser)
			{
				_workflowBrowser.OnKeyUp(curr);
			}
		}
		return true;
	}

	public bool HandleKeyDown(KeyCode curr, Modifiers mods)
	{
		switch (curr)
		{
		case KeyCode.Escape:
			if (_settingsMenuHost.IsOpen())
			{
				_settingsMenuHost.Close();
			}
			else
			{
				_settingsMenuHost.Open();
			}
			return true;
		case KeyCode.Space:
		{
			StyledButton mainButton = CurrentBrowser.GetMainButton();
			if (mainButton != null)
			{
				mainButton.SimulateClickDown();
				return true;
			}
			break;
		}
		}
		return false;
	}

	public void OnInteractionCleared()
	{
		while (IsAnyBrowserOpen)
		{
			CloseCurrentBrowser();
		}
	}

	public void Clear()
	{
		while (IsAnyBrowserOpen)
		{
			CloseCurrentBrowser();
		}
		if (_matchManager != null)
		{
			_matchManager.MatchStateChanged -= OnMatchStateChanged;
			_matchManager.MatchCompleted -= OnMatchCompleted;
		}
	}

	public void OnUpdate(float time)
	{
		if (!IsAnyBrowserOpen && IsConsecutiveBrowser)
		{
			_consecutiveBrowserTimer += time;
		}
	}

	private void ResetTimer()
	{
		_consecutiveBrowserTimer = 0f;
	}

	public IBrowser OpenBrowser(IDuelSceneBrowserProvider browserTypeProvider)
	{
		if (_dragController.IsDragging)
		{
			_dragController.EndDrag();
		}
		DuelSceneBrowserType browserType = browserTypeProvider.GetBrowserType();
		if (_workflowBrowser != null && !IsWorkflowBrowser(browserType))
		{
			WorkflowBrowserRoot.gameObject.SetActive(value: false);
			WorkflowBrowserRootScaledCorrectly.gameObject.SetActive(value: false);
		}
		else
		{
			CloseCurrentBrowser();
		}
		_provider.CurrentBrowser = _browserFactory.Make(browserTypeProvider);
		if (IsWorkflowBrowser(browserType))
		{
			_workflowBrowser = CurrentBrowser;
		}
		CurrentBrowser.ShownHandlers += OnBrowserShown;
		CurrentBrowser.HiddenHandlers += OnBrowserHidden;
		CurrentBrowser.ClosedHandlers += OnBrowserClosed;
		this.BrowserOpened?.Invoke(CurrentBrowser);
		CurrentBrowser.Init();
		return CurrentBrowser;
	}

	public void CloseCurrentBrowser()
	{
		CurrentBrowser?.Close();
	}

	private void OnBrowserShown()
	{
		_keyboardManager.Subscribe(this);
		this.BrowserShown?.Invoke(CurrentBrowser);
	}

	private void OnBrowserHidden()
	{
		_keyboardManager.Unsubscribe(this);
		this.BrowserHidden?.Invoke(CurrentBrowser);
	}

	private void OnBrowserClosed()
	{
		BrowserBase currentBrowser = CurrentBrowser;
		currentBrowser.HiddenHandlers -= OnBrowserHidden;
		currentBrowser.ShownHandlers -= OnBrowserShown;
		currentBrowser.ClosedHandlers -= OnBrowserClosed;
		if (!IsWorkflowBrowser(currentBrowser.GetBrowserType()) && _workflowBrowser != null)
		{
			_provider.CurrentBrowser = _workflowBrowser;
			WorkflowBrowserRoot.gameObject.SetActive(value: true);
			WorkflowBrowserRootScaledCorrectly.gameObject.SetActive(value: true);
			return;
		}
		_provider.CurrentBrowser = null;
		_workflowBrowser = null;
		if (WorkflowBrowserRoot != null && WorkflowBrowserRoot.gameObject != null && !WorkflowBrowserRoot.gameObject.activeSelf)
		{
			WorkflowBrowserRoot.gameObject.SetActive(value: true);
		}
		if (WorkflowBrowserRootScaledCorrectly != null && WorkflowBrowserRootScaledCorrectly.gameObject != null && !WorkflowBrowserRootScaledCorrectly.gameObject.activeSelf)
		{
			WorkflowBrowserRootScaledCorrectly.gameObject.SetActive(value: true);
		}
	}

	private void OnMatchStateChanged(MatchState matchState)
	{
		if (matchState == MatchState.GameComplete)
		{
			Clear();
		}
	}

	private void OnMatchCompleted()
	{
		Clear();
	}

	private static bool IsWorkflowBrowser(DuelSceneBrowserType browserType)
	{
		return browserType switch
		{
			DuelSceneBrowserType.ViewDismiss => false, 
			DuelSceneBrowserType.AttachmentAndExileStack => false, 
			DuelSceneBrowserType.LibrarySideboard => false, 
			_ => true, 
		};
	}
}
