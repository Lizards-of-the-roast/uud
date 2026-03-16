using System;
using System.Linq;
using AssetLookupTree;
using Assets.Core.Shared.Code;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Mtga.Format;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.PlayBlade;
using Wizards.Unification.Models.PlayBlade;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class HomePageBillboard : MonoBehaviour
{
	public TextMeshProUGUI Title;

	public TextMeshProUGUI TimerText;

	public TextMeshProUGUI Description;

	public Localize locTitle;

	public Localize locDescription;

	public Transform TimerRow;

	public Image Image;

	public GameObject RankContainer;

	public Image RankImage;

	public Image Stopwatch;

	public Image Lock;

	public Color PreviewColor;

	public Color LockColor;

	public Color CloseColor;

	[SerializeField]
	private GameObject _calloutBadge;

	private EventContext _event;

	private EBillboardType _billboardType;

	private EventTimerState _lastState;

	private TimeSpan _lastTimeLeft;

	private readonly AssetLoader.AssetTracker<Sprite> _imageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("HomeBillboardImageSprite");

	private readonly AssetLoader.AssetTracker<Sprite> _rankImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("PlayBladeEventTitleRankSprite");

	private IPlayBladeSelectionProvider _playBladeSelectionProvider;

	private PlayBladeConfigDataProvider _playBladeConfigProvider;

	private PlayBladeV3 _playBlade;

	private bool _initialized;

	public string EventId => _event.PlayerEvent.EventInfo.EventId;

	private void Initialize()
	{
		_playBladeSelectionProvider = Pantry.Get<IPlayBladeSelectionProvider>();
		_playBladeConfigProvider = Pantry.Get<PlayBladeConfigDataProvider>();
		_initialized = true;
	}

	private void Awake()
	{
		if (!_initialized)
		{
			Initialize();
		}
	}

	private void Update()
	{
		if (_event == null)
		{
			return;
		}
		EventTimerState timerState = _event.PlayerEvent.GetTimerState();
		if (timerState == EventTimerState.Preview)
		{
			if (_lastState != timerState)
			{
				TimerRow.gameObject.UpdateActive(active: true);
				Stopwatch.gameObject.UpdateActive(active: false);
				Lock.gameObject.UpdateActive(active: true);
				Lock.color = PreviewColor;
				TimerText.color = PreviewColor;
				TimerText.gameObject.UpdateActive(active: true);
			}
			TimeSpan timeSpan = _event.PlayerEvent.EventInfo.StartTime - ServerGameTime.GameTime;
			if (_lastTimeLeft.Minutes != timeSpan.Minutes)
			{
				_lastTimeLeft = timeSpan;
				string item = timeSpan.To_HH_MM();
				string text = string.Empty;
				if (Languages.ActiveLocProvider != null)
				{
					text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/HomePage/Billboards/EventStartTimer", ("timeLeft", item));
				}
				TimerText.text = text;
			}
		}
		if (timerState == EventTimerState.Unjoined && _lastState != timerState)
		{
			TimerRow.gameObject.UpdateActive(active: false);
		}
		if (timerState == EventTimerState.Unjoined_LockingSoon)
		{
			if (_lastState != timerState)
			{
				TimerRow.gameObject.UpdateActive(active: true);
				Stopwatch.gameObject.UpdateActive(active: true);
				Lock.gameObject.UpdateActive(active: false);
				Stopwatch.color = LockColor;
				TimerText.color = LockColor;
				TimerText.gameObject.UpdateActive(active: true);
			}
			TimeSpan timeSpan2 = _event.PlayerEvent.EventInfo.LockedTime - ServerGameTime.GameTime;
			if (_lastTimeLeft.Minutes != timeSpan2.Minutes)
			{
				_lastTimeLeft = timeSpan2;
				string item2 = timeSpan2.To_HH_MM();
				string localizedText = Languages.ActiveLocProvider.GetLocalizedText("MainNav/HomePage/Billboards/SignUpEndTimer", ("timeLeft", item2));
				TimerText.text = localizedText;
			}
		}
		if (timerState == EventTimerState.Joined && _lastState != timerState)
		{
			TimerRow.gameObject.UpdateActive(active: false);
		}
		if (timerState == EventTimerState.Joined_ClosingSoon)
		{
			if (_lastState != timerState)
			{
				TimerRow.gameObject.UpdateActive(active: true);
				Stopwatch.gameObject.UpdateActive(active: true);
				Lock.gameObject.UpdateActive(active: false);
				Stopwatch.color = CloseColor;
				TimerText.color = CloseColor;
				TimerText.gameObject.UpdateActive(active: true);
			}
			TimeSpan timeSpan3 = _event.PlayerEvent.EventInfo.ClosedTime - ServerGameTime.GameTime;
			if (_lastTimeLeft.Minutes != timeSpan3.Minutes)
			{
				_lastTimeLeft = timeSpan3;
				string item3 = timeSpan3.To_HH_MM();
				string localizedText2 = Languages.ActiveLocProvider.GetLocalizedText("MainNav/HomePage/Billboards/EventEndTimer", ("timeLeft", item3));
				TimerText.text = localizedText2;
			}
		}
		_lastState = timerState;
	}

	private void SetNpeBadging(BillboardData billboardData)
	{
		if (!(_calloutBadge == null) && billboardData != null && billboardData.BillboardType != EBillboardType.Filter)
		{
			IEventInfo obj = billboardData.BillboardEvent?.PlayerEvent?.EventInfo;
			_calloutBadge.SetActive(obj?.ShouldBadgeOnEventItem() ?? false);
		}
	}

	public void SetEvent(AssetLookupSystem assetLookupSystem, BillboardData billboardData, PlayBladeV3 playBlade, CombinedRankInfo combinedRankInfo)
	{
		if (!_initialized)
		{
			Initialize();
		}
		SetNpeBadging(billboardData);
		_event = billboardData.BillboardEvent;
		_billboardType = billboardData.BillboardType;
		_playBlade = playBlade;
		IEventUXInfo eventUXInfo = _event?.PlayerEvent?.EventUXInfo;
		string text = _billboardType switch
		{
			EBillboardType.Event => eventUXInfo?.EventComponentData?.TitleRankText?.LocKey, 
			EBillboardType.Filter => billboardData?.BillboardDynamicFilterTag?.LocTitle, 
			_ => "", 
		};
		if (string.IsNullOrEmpty(text))
		{
			SimpleLog.LogPreProdError("[Billboard] Failed to find Desc in EventComponentData for " + (eventUXInfo?.PublicEventName ?? "null") + "\n falling back to eventName prepend ");
			text = eventUXInfo?.TitleLocKey;
		}
		locTitle.SetText(text);
		bool flag = _billboardType == EBillboardType.Event && _event.PlayerEvent.EventInfo.IsRanked;
		RankContainer.SetActive(flag);
		if (flag)
		{
			bool isLimited = FormatUtilities.IsLimited(_event.PlayerEvent.EventInfo.FormatType);
			string rankImagePath = RankIconUtils.GetRankImagePath(assetLookupSystem, combinedRankInfo, isLimited);
			AssetLoaderUtils.TrySetSprite(RankImage, _rankImageSpriteTracker, rankImagePath);
		}
		GetComponent<CustomButton>().OnClick.RemoveAllListeners();
		GetComponent<CustomButton>().OnClick.AddListener(delegate
		{
			if (_billboardType == EBillboardType.Event)
			{
				if (_event.PlayerEvent.EventUXInfo.HasEventPage)
				{
					SceneLoader.GetSceneLoader().GoToEventScreen(_event, reloadIfAlreadyLoaded: false, SceneLoader.NavMethod.Banner);
				}
				else
				{
					PlayBladeQueueEntry playBladeQueueEntry = _playBladeConfigProvider.GetPlayBladeConfig().FirstOrDefault((PlayBladeQueueEntry c) => c.EventNameBO1 == _event.PlayerEvent.EventInfo.InternalEventName || c.EventNameBO3 == _event.PlayerEvent.EventInfo.InternalEventName);
					if (playBladeQueueEntry == null)
					{
						SimpleLog.LogError("Could not find PlayBlade queue for event: " + _event.PlayerEvent.EventInfo.InternalEventName);
					}
					else
					{
						BladeSelectionData selection = _playBladeSelectionProvider.GetSelection();
						selection.bladeType = Wizards.Mtga.PlayBlade.BladeType.FindMatch;
						selection.findMatch.DeckId = Guid.Empty;
						selection.findMatch.UseBO3 = false;
						selection.findMatch.QueueType = playBladeQueueEntry.QueueType;
						selection.findMatch.QueueId = playBladeQueueEntry.Id;
						selection.findMatch.QueueIdForQueueType[selection.findMatch.QueueType] = selection.findMatch.QueueId;
						_playBladeSelectionProvider.SetSelection(selection);
						_playBlade.Show();
					}
				}
			}
			else if (_billboardType == EBillboardType.Filter)
			{
				_playBlade.ShowEventsTabAndFilter(billboardData.BillboardDynamicFilterTag.TagId);
			}
		});
		GetComponent<CustomButton>().OnMouseover.RemoveAllListeners();
		GetComponent<CustomButton>().OnMouseover.AddListener(delegate
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, base.gameObject);
		});
		AssetLoaderUtils.TrySetSprite(Image, _imageSpriteTracker, ClientEventDefinitionList.GetBillboardImagePath(assetLookupSystem, _event));
		if (Image.sprite == null)
		{
			Debug.LogWarningFormat("No event billboard image for event \"{0}\"", _event.PlayerEvent.EventUXInfo.PublicEventName);
		}
		bool flag2 = _event == null || _event.PlayerEvent.CourseData.CurrentModule == PlayerEventModule.Join;
		Animator component = GetComponent<Animator>();
		if (component.isActiveAndEnabled)
		{
			component.SetBool("Attract", !flag2);
		}
	}

	public void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(Image, _imageSpriteTracker);
		AssetLoaderUtils.CleanupImage(RankImage, _rankImageSpriteTracker);
	}
}
