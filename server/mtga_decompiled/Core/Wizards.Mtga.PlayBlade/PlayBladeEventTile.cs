using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Wizards.MDN.Services.Models.Event;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace Wizards.Mtga.PlayBlade;

public class PlayBladeEventTile : MonoBehaviour
{
	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private CustomButton _customButton;

	[SerializeField]
	private Localize _titleText;

	[SerializeField]
	private TMP_Text _timerText;

	[SerializeField]
	private Image _backgroundImage;

	[SerializeField]
	private Image _stopWatchImage;

	[SerializeField]
	private Image _lockImage;

	[SerializeField]
	private Image _rankImage;

	[SerializeField]
	private GameObject _calloutBadge;

	[Header("Scaffolds")]
	[SerializeField]
	private RectTransform _timerParent;

	[SerializeField]
	private RectTransform _progressParent;

	[SerializeField]
	private RectTransform _attractParent;

	[SerializeField]
	private RectTransform _newEventParent;

	[SerializeField]
	private RectTransform _bestOf3Indicator;

	[SerializeField]
	private RectTransform _rankBadge;

	[SerializeField]
	private RectTransform _eventProgressPips;

	[SerializeField]
	private RectTransform _colorChallenteProgressPips;

	[Header("EventPips")]
	[FormerlySerializedAs("_eventPip1")]
	[SerializeField]
	private Image _eventPipOn1;

	[SerializeField]
	private Image _eventPipOn2;

	[SerializeField]
	private Image _eventPipOn3;

	[SerializeField]
	private Image _eventPipOff1;

	[Header("ColorChallengePips")]
	[SerializeField]
	private Image _whitePipOn;

	[SerializeField]
	private Image _bluePipOn;

	[SerializeField]
	private Image _blackPipOn;

	[SerializeField]
	private Image _redPipOn;

	[SerializeField]
	private Image _greenPipOn;

	[Header("Colors")]
	[SerializeField]
	private Color _previewColor;

	[SerializeField]
	private Color _lockColor;

	[SerializeField]
	private Color _closeColor;

	private Action<BladeEventInfo> Clicked;

	private BladeEventInfo _model;

	private AssetLoader.AssetTracker<Sprite> _backgroundImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("PlayBladeEventTitleBackgroundSprite");

	private AssetLoader.AssetTracker<Sprite> _rankImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("PlayBladeEventTitleRankSprite");

	private Dictionary<string, string> _timerParams = new Dictionary<string, string>();

	private const string TIMERPARAM_KEY = "timeLeft";

	private void Awake()
	{
		_customButton.OnClick.AddListener(OnClicked);
		_timerParams.Add("timeLeft", "");
	}

	private void OnDestroy()
	{
		_customButton.OnClick.RemoveListener(OnClicked);
		AssetLoaderUtils.CleanupImage(_backgroundImage, _backgroundImageSpriteTracker);
		AssetLoaderUtils.CleanupImage(_rankImage, _rankImageSpriteTracker);
		Clicked = null;
		_model = null;
	}

	public void SetModel(BladeEventInfo model, IColorChallengeStrategy strategy)
	{
		_model = model;
		AssetLoaderUtils.TrySetSprite(_backgroundImage, _backgroundImageSpriteTracker, _model.BladeImagePath);
		_titleText.SetText(_model.LocTitle);
		AssetLoaderUtils.TrySetSprite(_rankImage, _rankImageSpriteTracker, _model.RankImagePath);
		_attractParent.gameObject.SetActive(_model.IsInProgress);
		base.gameObject.name = "EventTile - (" + (_model?.EventName ?? "null") + ")";
		_bestOf3Indicator.gameObject.SetActive(model.WinCondition == MatchWinCondition.BestOf3);
		_rankBadge.gameObject.SetActive(model.IsRanked);
		SetEventPips(model);
		SetColorChallengePips(model, strategy);
		SetNpeBadging(model);
	}

	private void SetNpeBadging(BladeEventInfo billboardData)
	{
		if (!(_calloutBadge == null))
		{
			_calloutBadge.SetActive(IEventInfoExtensions.ShouldBadgeOnEventItem(billboardData.EventName));
		}
	}

	private void SetEventPips(BladeEventInfo eventInfo)
	{
		if (eventInfo.IsInProgress && eventInfo.TotalProgressPips > 0)
		{
			_eventProgressPips.gameObject.SetActive(value: true);
			if (eventInfo.TotalProgressPips == 3)
			{
				SetEventPipsToOn(eventInfo.PlayerProgress, _eventPipOn1, _eventPipOn2, _eventPipOn3);
			}
			else if (eventInfo.TotalProgressPips == 2)
			{
				_eventPipOff1.gameObject.SetActive(value: false);
				SetEventPipsToOn(eventInfo.PlayerProgress, _eventPipOn2, _eventPipOn3);
			}
			else
			{
				_eventProgressPips.gameObject.SetActive(value: false);
			}
		}
		else
		{
			_eventProgressPips.gameObject.SetActive(value: false);
		}
	}

	private void SetEventPipsToOn(int numberToMark, params Image[] pips)
	{
		for (int i = 0; i < numberToMark; i++)
		{
			pips[i].gameObject.SetActive(value: true);
		}
	}

	private void SetColorChallengePips(BladeEventInfo eventInfo, IColorChallengeStrategy strategy)
	{
		if (eventInfo.EventName == "ColorChallenge")
		{
			_colorChallenteProgressPips.gameObject.SetActive(value: true);
			{
				foreach (KeyValuePair<string, IColorChallengeTrack> track in strategy.Tracks)
				{
					IColorChallengeTrack value = track.Value;
					switch (value.Name.ToLower())
					{
					case "white":
						_whitePipOn.gameObject.SetActive(value.Completed);
						break;
					case "blue":
						_bluePipOn.gameObject.SetActive(value.Completed);
						break;
					case "black":
						_blackPipOn.gameObject.SetActive(value.Completed);
						break;
					case "red":
						_redPipOn.gameObject.SetActive(value.Completed);
						break;
					case "green":
						_greenPipOn.gameObject.SetActive(value.Completed);
						break;
					default:
						Debug.LogError("Encountered unknown color challenge track name: " + value.Name);
						break;
					}
				}
				return;
			}
		}
		_colorChallenteProgressPips.gameObject.SetActive(value: false);
	}

	public void SetOnClick(Action<BladeEventInfo> action)
	{
		Clicked = action;
	}

	public void OnClicked()
	{
		Clicked?.Invoke(_model);
	}

	public void ShowNewEventFlag(bool showFlag)
	{
		_newEventParent.gameObject.SetActive(showFlag);
	}

	public void UpdateTimerText(DateTime dateTime)
	{
		_stopWatchImage.gameObject.UpdateActive(active: false);
		_lockImage.gameObject.UpdateActive(active: false);
		BladeTimerType timerType = _model.TimerType;
		if (timerType == BladeTimerType.Invalid || timerType == BladeTimerType.Hidden)
		{
			_timerText.gameObject.SetActive(value: false);
			_timerParent.gameObject.UpdateActive(active: false);
			return;
		}
		_timerText.gameObject.SetActive(value: true);
		MTGALocalizedString mTGALocalizedString;
		TimeSpan ts;
		switch (timerType)
		{
		case BladeTimerType.Preview:
			_lockImage.gameObject.UpdateActive(active: true);
			_lockImage.color = _previewColor;
			_timerText.color = _previewColor;
			mTGALocalizedString = "MainNav/HomePage/Billboards/EventStartTimer";
			ts = _model.StartTime - dateTime;
			break;
		case BladeTimerType.Unjoined_LockingSoon:
			mTGALocalizedString = "MainNav/EventPage/SignUpEndTimer";
			ts = _model.LockTime - dateTime;
			if (_model.LockTime < dateTime)
			{
				base.gameObject.UpdateActive(active: false);
				return;
			}
			break;
		case BladeTimerType.Joined_ClosingSoon:
			_stopWatchImage.gameObject.UpdateActive(active: true);
			_stopWatchImage.color = _closeColor;
			_timerText.color = _closeColor;
			mTGALocalizedString = "MainNav/EventPage/EventEndTimer";
			ts = _model.CloseTime - dateTime;
			break;
		case BladeTimerType.ClosedAndCompleted:
			_timerText.color = _closeColor;
			mTGALocalizedString = "MainNav/EventPage/EventEndTimer";
			ts = _model.CloseTime - dateTime;
			break;
		default:
			mTGALocalizedString = null;
			ts = _model.CloseTime - dateTime;
			break;
		}
		_timerParams["timeLeft"] = ts.To_HH_MM();
		if (mTGALocalizedString != null)
		{
			mTGALocalizedString.Parameters = _timerParams;
		}
		_timerParent.gameObject.UpdateActive(mTGALocalizedString != null);
		_timerText.SetText(mTGALocalizedString);
	}
}
