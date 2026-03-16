using System;
using System.Collections;
using Core.Code.Familiar;
using Core.Code.Promises;
using EventPage.Components;
using UnityEngine;
using UnityEngine.Events;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace EventPage.CampaignGraph;

public class CampaignGraphMainButtonModule : EventModule
{
	[SerializeField]
	private CustomButtonWithTooltip _playButton;

	[SerializeField]
	private CustomButtonWithTooltip _startButton;

	private Localize _playButtonLoc;

	private SceneLoader _sceneLoader;

	private IColorChallengePlayerEvent _event;

	private CampaignGraphContentController _campaignGraphController;

	private BotTool _botTool;

	private SceneLoader SceneLoader
	{
		get
		{
			if (_sceneLoader == null)
			{
				_sceneLoader = SceneLoader.GetSceneLoader();
			}
			return _sceneLoader;
		}
	}

	public event Action OnStartButtonClicked;

	private void Awake()
	{
		_playButtonLoc = _playButton.GetComponentInChildren<Localize>();
		_startButton.OnClick.AddListener(_onClick_StartButton);
		_botTool = Pantry.Get<BotTool>();
	}

	private void _onClick_StartButton()
	{
		_startButton.enabled = false;
		this.OnStartButtonClicked?.Invoke();
		StartCoroutine(_coroutine_playAnimationsAndStartEvent());
	}

	private IEnumerator _coroutine_playAnimationsAndStartEvent()
	{
		_event.SelectMatchNode(_event.CurrentTrack.Nodes[0].Id);
		StartCoroutine(_parentTemplate.PlayAnimation(EventTemplateAnimation.Intro));
		yield return _parentTemplate.PlayAnimation(EventTemplateAnimation.ModuleOutro);
		_parentTemplate.UpdateModules();
		yield return _parentTemplate.PlayAnimation(EventTemplateAnimation.ModuleIntro);
	}

	public override void Show()
	{
		_event = _parentTemplate.EventContext.PlayerEvent as IColorChallengePlayerEvent;
		_campaignGraphController = _parentTemplate.EventPage;
		base.gameObject.SetActive(value: true);
	}

	public override void UpdateModule()
	{
		if (!_event.InPlayingMatchesModule)
		{
			SetPlayButtonState(PlayButtonState.Hidden);
			_startButton.enabled = true;
			_startButton.gameObject.UpdateActive(active: true);
			return;
		}
		SetPlayButtonState(PlayButtonState.Enabled);
		_startButton.gameObject.UpdateActive(active: false);
		if (_event.CourseData.CourseDeck == null)
		{
			_playButtonLoc.SetText(new MTGALocalizedString
			{
				Key = "MainNav/EventPage/Button_SelectDeck"
			});
			AddPlayButtonListener(DeckSelectButtonClicked);
		}
		else
		{
			AddPlayButtonListener(PlayButtonClicked);
			_playButtonLoc.SetText(new MTGALocalizedString
			{
				Key = "MainNav/EventPage/Button_PlayMatch"
			});
		}
		void AddPlayButtonListener(UnityAction callback)
		{
			_playButton.OnClick.RemoveAllListeners();
			_playButton.OnClick.AddListener(callback);
		}
	}

	public override void LateUpdateModule()
	{
		UpdateModule();
	}

	public override void Hide()
	{
		base.gameObject.SetActive(value: false);
		_playButton.Hide();
	}

	public void SetPlayButtonState(PlayButtonState state, bool showTooltip = false)
	{
		if (state == PlayButtonState.Hidden)
		{
			_playButton.Hide();
		}
		else
		{
			_playButton.Show(state == PlayButtonState.Enabled, showTooltip);
		}
	}

	private void DeckSelectButtonClicked()
	{
		SceneLoader.GoToConstructedDeckSelect(new DeckSelectContext
		{
			EventContext = base.EventContext
		});
	}

	private void PlayButtonClicked()
	{
		AudioManager.PlayAudio(WwiseEvents.match_making_find_match, AudioManager.Default);
		if (!_event.CurrentMatchNode.IsPvpMatch)
		{
			BotControlManager.SetUpBotTool(_botTool, _assetLookupSystem);
		}
		WrapperController.Instance.Matchmaking.SetExpectedEvent(base.EventContext);
		WrapperController.EnableLoadingIndicator(enabled: true);
		base.EventContext.PlayerEvent.JoinNewMatchQueue().ThenOnMainThread(delegate(Promise<string> p)
		{
			WrapperController.EnableLoadingIndicator(enabled: false);
			if (p.Successful)
			{
				WrapperController.Instance.Matchmaking.SetupEventMatch(base.EventContext);
			}
			else
			{
				Debug.LogError("Error joining event queue. Error message: " + p.Error.Message);
				SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Event_Join_Error_Text"));
			}
		});
	}
}
