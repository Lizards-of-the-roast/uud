using System;
using System.Collections.Generic;
using AssetLookupTree;
using Core.Code.Input;
using MTGA.KeyboardManager;
using MTGA.Social;
using UnityEngine;
using UnityEngine.Serialization;
using Wizards.MDN;
using Wizards.Mtga.Decks;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Extensions;

public class PlayBladeController : MonoBehaviour, IKeyDownSubscriber, IKeySubscriber, IBackActionHandler
{
	public enum PlayBladeVisualStates
	{
		Hidden,
		Events,
		Challenge
	}

	private PlayBladeVisualStates _playBladeVisualState;

	public DeckSelectBlade DeckSelector;

	public CustomButton ModalFadeButton;

	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private GameObject _buttonsParent;

	[Header("Controller References")]
	[FormerlySerializedAs("_friendChallengeBladeWidget")]
	[SerializeField]
	private UnifiedChallengeBladeWidget unifiedChallengeBladeWidget;

	[SerializeField]
	private UnifiedChallengeDisplay _unifiedChallengeDisplay;

	private ContentControllerObjectives _objectivesController;

	private PlayBladeWidget _activeBladeWidget;

	private KeyboardManager _keyboardManager;

	private IActionSystem _actionSystem;

	private Animator _homeAnimator;

	private static readonly int Intro = Animator.StringToHash("Intro");

	private static readonly int ReIntro = Animator.StringToHash("ReIntro");

	private static readonly int Outro = Animator.StringToHash("Outro");

	public PlayBladeVisualStates PlayBladeVisualState
	{
		get
		{
			return _playBladeVisualState;
		}
		private set
		{
			if (value != _playBladeVisualState)
			{
				PreviousPlayBladeVisualState = _playBladeVisualState;
				_playBladeVisualState = value;
				this.OnPlayBladeVisibilityChanged?.Invoke();
			}
			else
			{
				Debug.LogWarning("PreviousPlayBladeVisualState is being set to the same value as previously: " + PreviousPlayBladeVisualState);
			}
		}
	}

	public static PlayBladeVisualStates PreviousPlayBladeVisualState { get; set; } = PlayBladeVisualStates.Hidden;

	public bool IsDeckSelected
	{
		get
		{
			if (_activeBladeWidget != null)
			{
				return _activeBladeWidget.IsDeckSelected;
			}
			return false;
		}
	}

	public Client_Deck SelectedDeckInfo
	{
		get
		{
			if (!(_activeBladeWidget != null))
			{
				return null;
			}
			return _activeBladeWidget.SelectedDeck;
		}
	}

	public UnifiedChallengeBladeWidget UnifiedChallengeBladeWidget => unifiedChallengeBladeWidget;

	public PriorityLevelEnum Priority => PriorityLevelEnum.Wrapper;

	public ISocialManager SocialManager { get; private set; }

	public bool IsInitialized { get; private set; }

	public Guid CurrentChallengeId => unifiedChallengeBladeWidget.CurrentChallengeId;

	public event Action OnPlayBladeVisibilityChanged;

	private void Awake()
	{
		ModalFadeButton.OnClick.AddListener(Hide);
		_animator = base.gameObject.GetComponent<Animator>();
	}

	private void OnDisable()
	{
		if (IsInitialized)
		{
			if (PlayBladeVisualState != PlayBladeVisualStates.Hidden && AudioManager.Instance != null)
			{
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_play_shelf_close, AudioManager.Default);
			}
			_objectivesController.ResetAnimations();
			PlayBladeVisualState = PlayBladeVisualStates.Hidden;
		}
	}

	private void OnDestroy()
	{
		_keyboardManager?.Unsubscribe(this);
	}

	public void Initialize(ContentControllerObjectives objectivesController, KeyboardManager keyboardManager, IActionSystem actionSystem, AssetLookupSystem assetLookupSystem, CardViewBuilder cardViewBuilder)
	{
		base.gameObject.SetActive(value: true);
		_objectivesController = objectivesController;
		unifiedChallengeBladeWidget.Initialize(objectivesController);
		_keyboardManager = keyboardManager;
		_actionSystem = actionSystem;
		InitPlatformHelper();
		IsInitialized = true;
		base.gameObject.SetActive(value: false);
	}

	public void SetHomeAnimator(Animator animator)
	{
		_homeAnimator = animator;
	}

	private void InitPlatformHelper()
	{
		(PlatformUtils.IsHandheld() ? ((PlayBladeHelper_Base)base.gameObject.AddComponent<PlayBladeHelper_Mobile>()) : ((PlayBladeHelper_Base)base.gameObject.AddComponent<PlayBladeHelper_PC>())).Init(this);
	}

	public void Show(EventContext eventContext = null)
	{
		Show(PlayBladeVisualStates.Events);
	}

	public void Show(PlayBladeVisualStates visualState)
	{
		int trigger = ((base.gameObject.activeSelf && PlayBladeVisualState == PlayBladeVisualStates.Hidden) ? Intro : ReIntro);
		_animator.SetTrigger(trigger);
		if (DeckSelector.IsShowing)
		{
			HideDeckSelector();
		}
		_homeAnimator.SetTrigger("Outro");
		base.gameObject.UpdateActive(active: true);
		PlayBladeVisualState = visualState;
		_activeBladeWidget = WidgetForState(PlayBladeVisualState);
		foreach (PlayBladeWidget item in AllWidgets())
		{
			if (!(item == _activeBladeWidget))
			{
				item.gameObject.UpdateActive(active: false);
				item.SetMainButton(enable: false);
			}
		}
		_activeBladeWidget.Show();
		_keyboardManager?.Subscribe(this);
		_actionSystem.PushFocus(this);
		if (visualState != PlayBladeVisualStates.Challenge)
		{
			AudioManager.PlayAudio("Music_SetState_Scenes_Click_Play", AudioManager.Default);
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_play_shelf_open, AudioManager.Default);
		}
	}

	public void Hide()
	{
		if (PlayBladeVisualState != PlayBladeVisualStates.Hidden && _activeBladeWidget != null && _activeBladeWidget.Hide())
		{
			_keyboardManager?.Unsubscribe(this);
			_actionSystem.PopFocus(this);
			HideDeckSelector();
			_animator.SetTrigger(Outro);
			_activeBladeWidget.SetMainButton(enable: false, "MainNav/Landing/Play");
			if (PlayBladeVisualState != PlayBladeVisualStates.Challenge)
			{
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_play_shelf_close, AudioManager.Default);
			}
			PlayBladeVisualState = PlayBladeVisualStates.Hidden;
			_objectivesController.SetDailyWeeklyStatus(enabled: true);
			_homeAnimator.SetTrigger("Intro");
		}
	}

	public void ToggleVisualState(PlayBladeVisualStates visualStateToToggle)
	{
		if (visualStateToToggle == PlayBladeVisualState || visualStateToToggle == PlayBladeVisualStates.Hidden)
		{
			Hide();
		}
		else
		{
			Show(visualStateToToggle);
		}
	}

	public bool HandleKeyDown(KeyCode curr, Modifiers mods)
	{
		if (curr == KeyCode.Escape)
		{
			if (_activeBladeWidget == null || _activeBladeWidget.ProceedWithHide())
			{
				Hide();
			}
			return true;
		}
		return false;
	}

	public void OnBack(ActionContext context)
	{
		if (!(_activeBladeWidget != null) || _activeBladeWidget.ProceedWithHide())
		{
			Hide();
		}
	}

	public void SetDeck(Client_Deck model, DeckFormat deckFormat)
	{
		_activeBladeWidget.SetChallengeDeck(model, deckFormat);
	}

	public void SetDeckBoxSelected(bool isSelected)
	{
		_activeBladeWidget.SetDeckBoxSelected(isSelected);
	}

	public void SetSocialClient(ISocialManager socialManager)
	{
		SocialManager = socialManager;
	}

	private PlayBladeWidget WidgetForState(PlayBladeVisualStates state)
	{
		if (state == PlayBladeVisualStates.Challenge)
		{
			return unifiedChallengeBladeWidget;
		}
		return null;
	}

	private IEnumerable<PlayBladeWidget> AllWidgets()
	{
		return new PlayBladeWidget[1] { unifiedChallengeBladeWidget };
	}

	public void ShowDeckSelector(EventContext eventContext, DeckFormat deckFormat, Action onHideDeckSelector, bool allowRefresh = false)
	{
		DeckSelector.Show(eventContext, deckFormat, onHideDeckSelector, allowRefresh);
		_unifiedChallengeDisplay.gameObject.SetActive(value: false);
	}

	public void HideDeckSelector()
	{
		DeckSelector.Hide();
		_unifiedChallengeDisplay.gameObject.SetActive(value: true);
	}

	public void OnSelectDeckClicked(EventContext eventContext, DeckFormat deckFormat, Action onHideDeckSelector)
	{
		if (DeckSelector.IsShowing)
		{
			HideDeckSelector();
		}
		else
		{
			ShowDeckSelector(eventContext, deckFormat, onHideDeckSelector);
		}
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
	}

	public void ViewFriendChallenge(string playerId)
	{
		Show(PlayBladeVisualStates.Challenge);
		unifiedChallengeBladeWidget.ViewFriendChallenge(playerId);
		_unifiedChallengeDisplay.ViewChallenge(playerId);
	}

	public void ViewFriendChallenge(Guid challengeId)
	{
		Show(PlayBladeVisualStates.Challenge);
		unifiedChallengeBladeWidget.ViewFriendChallenge(challengeId);
		_unifiedChallengeDisplay.ViewChallenge(challengeId);
	}

	private void AnimationEvent_OnPlayBladeShown()
	{
	}

	private void AnimationEvent_OnPlayBladeStartedMove()
	{
		_buttonsParent.SetActive(value: true);
	}

	private void AnimationEvent_OnPlayBladeHidden()
	{
		if (PlayBladeVisualState == PlayBladeVisualStates.Hidden)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void AnimationEvent_OnPlayBladeFinishedMove()
	{
		_buttonsParent.SetActive(value: false);
	}
}
