using System;
using System.Collections.Generic;
using System.Linq;
using Core.MainNavigation.RewardTrack;
using Core.Meta.MainNavigation;
using Core.Meta.MainNavigation.RewardTrack;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class ProgressionTracksContentController : NavContentController
{
	[Serializable]
	private class PageControlView
	{
		public Button PagePrevButton;

		public Button PageNextButton;

		public Localize PageOfText;

		public Button PageNavDotPrefab;

		public Transform PageNavDotContainer;

		[NonSerialized]
		public List<Button> PageNavDotInstances;
	}

	[SerializeField]
	private Transform _buttons;

	[SerializeField]
	private CustomButton _backButton;

	[SerializeField]
	private Transform _tabGroup;

	[SerializeField]
	private NotificationPopup _hangerPrefab;

	[SerializeField]
	private Transform _hangerContainer;

	private NotificationPopup _hanger;

	[SerializeField]
	private Transform _trackContainer;

	[SerializeField]
	private CustomButton _clickShield;

	[SerializeField]
	private List<RewardTrackData> _rewardTrackData;

	[SerializeField]
	private RewardTrackView _rewardTrackPrefab;

	[Space]
	[SerializeField]
	private PageControlView _pageControls = new PageControlView();

	[SerializeField]
	private SwipePageTurnModule _pageSwipeControls;

	[SerializeField]
	private float _scrollSensitivity = 0.25f;

	[SerializeField]
	private bool _reverseScroll;

	[Header("Expired BattlePass")]
	[SerializeField]
	private Transform _completeCenter;

	[SerializeField]
	private Localize _completeTitle;

	private static readonly int Completed = Animator.StringToHash("Completed");

	private static readonly int Highlight = Animator.StringToHash("Highlight");

	private Animator _animator;

	private RewardTrackView _activeView;

	private List<RewardTrackView> _trackViews = new List<RewardTrackView>();

	private Dictionary<string, Tab> _trackTabs = new Dictionary<string, Tab>();

	private ProgressionTrackPageContext _context;

	private float _lastScroll;

	private NavContentType _sceneBeforeTabSelectForBI;

	private DateTime _tabSelectionTimeForBI;

	private IBILogger _logger;

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	private CardMaterialBuilder _cardMaterialBuilder;

	private RewardTrackScreenWrapperCompassGuide _compassGuide;

	public override NavContentType NavContentType => NavContentType.RewardTrack;

	private static SetMasteryDataProvider MasteryPassProvider => Pantry.Get<SetMasteryDataProvider>();

	public override void Activate(bool active)
	{
		if (active)
		{
			MasteryPassProvider.OnCurrentBpRewardTierUpdate += OnCurrentBpRewardTierUpdate;
			MasteryPassProvider.OnCurrentBpProgressUpdate += OnCurrentBpProgressUpdate;
		}
		else
		{
			MasteryPassProvider.OnCurrentBpRewardTierUpdate -= OnCurrentBpRewardTierUpdate;
			MasteryPassProvider.OnCurrentBpProgressUpdate -= OnCurrentBpProgressUpdate;
		}
		if (active)
		{
			string trackName = ((!MasteryPassProvider.HasTrackExpired(_context?.TrackName)) ? _context.TrackName : MasteryPassProvider.CurrentBpName);
			OpenTrackView(trackName);
			UpdateTabPips();
			_tabSelectionTimeForBI = DateTime.UtcNow;
		}
		else
		{
			OnTrackDoneBeingViewed(_sceneBeforeTabSelectForBI, (_activeView != null) ? _activeView.TrackName : null);
		}
	}

	protected override void Start()
	{
		base.Start();
		if (_compassGuide != null)
		{
			SetContext(_compassGuide.TrackPageContext);
		}
		_animator = GetComponent<Animator>();
		_backButton.OnClick.AddListener(OnBack);
		_pageControls.PagePrevButton.onClick.AddListener(OnPagePrev);
		_pageControls.PageNextButton.onClick.AddListener(OnPageNext);
		if ((bool)_pageSwipeControls)
		{
			if (_reverseScroll)
			{
				_pageSwipeControls.onSwipeLeft.AddListener(OnPageNext);
				_pageSwipeControls.onSwipeRight.AddListener(OnPagePrev);
			}
			else
			{
				_pageSwipeControls.onSwipeLeft.AddListener(OnPagePrev);
				_pageSwipeControls.onSwipeRight.AddListener(OnPageNext);
			}
		}
	}

	private void OnDestroy()
	{
		MasteryPassProvider.OnCurrentBpRewardTierUpdate -= OnCurrentBpRewardTierUpdate;
		MasteryPassProvider.OnCurrentBpProgressUpdate -= OnCurrentBpProgressUpdate;
		_backButton.OnClick.RemoveAllListeners();
		_pageControls.PagePrevButton.onClick.RemoveAllListeners();
		_pageControls.PageNextButton.onClick.RemoveAllListeners();
		if ((bool)_pageSwipeControls)
		{
			_pageSwipeControls.onSwipeLeft.RemoveAllListeners();
			_pageSwipeControls.onSwipeRight.RemoveAllListeners();
		}
		for (int i = 0; i < _pageControls.PageNavDotContainer.childCount; i++)
		{
			Button component = _pageControls.PageNavDotContainer.GetChild(i).GetComponent<Button>();
			if ((bool)component)
			{
				component.onClick.RemoveAllListeners();
			}
		}
	}

	public void Init(IBILogger logger, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, CardMaterialBuilder cardMaterialBuilder)
	{
		_logger = logger;
		_cardDatabase = cardDatabase;
		_cardViewBuilder = cardViewBuilder;
		_cardMaterialBuilder = cardMaterialBuilder;
		_compassGuide = Pantry.Get<WrapperCompass>().GetGuide<RewardTrackScreenWrapperCompassGuide>();
		List<string> list = new List<string>();
		list.Add(MasteryPassProvider.CurrentBpName);
		if (MasteryPassProvider.GetOrbCount(MasteryPassProvider.PreviousBpName) > 0)
		{
			list.Add(MasteryPassProvider.PreviousBpName);
		}
		if (list.Count == 1)
		{
			list.Clear();
		}
		_trackTabs.Clear();
		int num = 0;
		foreach (string trackName in list)
		{
			if (_rewardTrackData.All((RewardTrackData p) => p.TrackName != trackName))
			{
				Debug.LogError("Could not find track asset definition for " + trackName);
				continue;
			}
			Transform child = _tabGroup.GetChild(num++);
			Tab component = child.GetComponent<Tab>();
			if ((object)component == null)
			{
				Debug.LogError("Misaligned Tab asset for Progression Track " + trackName);
				child.gameObject.UpdateActive(active: false);
				continue;
			}
			component.gameObject.UpdateActive(active: true);
			component.SetLabel(MasteryPassProvider.GetTrackTitle(trackName));
			component.Clicked += OnTabClicked;
			bool pipVisible = MasteryPassProvider.GetOrbCount(trackName) > 0;
			component.SetPipVisible(pipVisible);
			_trackTabs.Add(trackName, component);
		}
		for (; num < _tabGroup.childCount; num++)
		{
			_tabGroup.GetChild(num).gameObject.UpdateActive(active: false);
		}
	}

	public void SetContext(ProgressionTrackPageContext context)
	{
		_context = context;
		_sceneBeforeTabSelectForBI = context.PreviousSceneForBI;
	}

	private void OnTabClicked(Tab clickedTab)
	{
		var (text2, _) = (KeyValuePair<string, Tab>)(ref _trackTabs.FirstOrDefault((KeyValuePair<string, Tab> t) => t.Value == clickedTab));
		if (_activeView != null && text2 != _activeView.TrackName)
		{
			OpenTrackView(text2);
		}
	}

	private void OpenTrackView(string trackName)
	{
		if (_activeView != null)
		{
			_activeView.PageChange -= UpdatePageControls;
			OnTrackDoneBeingViewed(_sceneBeforeTabSelectForBI, _activeView.TrackName);
		}
		_sceneBeforeTabSelectForBI = NavContentType.RewardTrack;
		_activeView = _trackViews.Find((RewardTrackView v) => v.TrackName == trackName);
		if (_activeView == null)
		{
			RewardTrackData rewardTrackData = _rewardTrackData.Find((RewardTrackData v) => v.TrackName == trackName);
			if (rewardTrackData == null)
			{
				Debug.LogError("Cannot find track asset data matching " + trackName + ".");
				return;
			}
			if (_rewardTrackPrefab == null)
			{
				Debug.LogError("Cannot find RewardTrackPrefab.");
				return;
			}
			_activeView = UnityEngine.Object.Instantiate(_rewardTrackPrefab, _trackContainer);
			_activeView.InjectTrackData(rewardTrackData);
			_activeView.SetPreviousTrack(MasteryPassProvider.PreviousBpName);
			if (_hanger == null)
			{
				_hanger = UnityEngine.Object.Instantiate(_hangerPrefab, _hangerContainer);
				_hanger.Init(_cardDatabase, _cardViewBuilder);
				_hanger.gameObject.SetActive(value: false);
				_hanger.SetTailActive(active: false);
			}
			_activeView.Hanger = _hanger;
			if (_clickShield != null)
			{
				_activeView.ClickShield = _clickShield;
			}
			_activeView.InitializeRewardTrackView(_cardDatabase, _cardViewBuilder, _cardMaterialBuilder);
			_trackViews.Add(_activeView);
		}
		foreach (KeyValuePair<string, Tab> trackTab in _trackTabs)
		{
			trackTab.Deconstruct(out var key, out var value);
			string text = key;
			Tab tab = value;
			bool tabActiveVisuals = text == trackName;
			tab.SetTabActiveVisuals(tabActiveVisuals);
		}
		foreach (RewardTrackView trackView in _trackViews)
		{
			Wotc.Mtga.Extensions.GameObjectExtensions.UpdateActive(active: trackView.TrackName == trackName, go: trackView.gameObject);
		}
		_activeView.PageChange += UpdatePageControls;
		_activeView.UpdateCurrentPage(resetToDefault: true);
		_activeView.SetPanelIntro(_context.PlayIntro);
		bool flag = MasteryPassProvider.HasTrackExpired(_activeView.TrackName);
		bool flag2 = !MasteryPassProvider.IsEnabled(_activeView.TrackName);
		_buttons.gameObject.UpdateActive(!flag && !flag2);
		_completeCenter.gameObject.UpdateActive(flag);
		if (flag)
		{
			_completeTitle.SetText("MainNav/BattlePass/SetExpired_Title", new Dictionary<string, string> { { "setName", _activeView.TrackLabel } });
		}
		else
		{
			UpdatePageControls();
		}
	}

	private void UpdateTabPips()
	{
		if (_trackTabs == null)
		{
			return;
		}
		foreach (KeyValuePair<string, Tab> trackTab in _trackTabs)
		{
			trackTab.Deconstruct(out var key, out var value);
			string trackName = key;
			Tab tab = value;
			bool pipVisible = MasteryPassProvider.GetOrbCount(trackName) > 0;
			tab.SetPipVisible(pipVisible);
		}
	}

	private void OnCurrentBpRewardTierUpdate(ClientRewardTierUpdate update)
	{
		_activeView.UpdateCurrentPage(resetToDefault: true);
		_activeView.UpdateLevel();
		_activeView.UpdatePurchaseOptions();
		UpdateTabPips();
	}

	private void OnCurrentBpProgressUpdate(Queue<LevelChange> levels)
	{
		_activeView.UpdateCurrentPage(resetToDefault: true);
		_activeView.UpdateLevel();
		_activeView.UpdatePurchaseOptions();
		UpdateTabPips();
	}

	private void OnBack()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_back, base.gameObject);
		GoToBackTarget();
	}

	private void GoToBackTarget()
	{
		SceneLoader sceneLoader = SceneLoader.GetSceneLoader();
		switch (_context.BackTarget)
		{
		case NavContentType.None:
		case NavContentType.Home:
			sceneLoader.GoToLanding(new HomePageContext());
			break;
		case NavContentType.Profile:
			sceneLoader.GoToProfileScreen(SceneChangeInitiator.User, "Back from Mastery Tracks");
			break;
		case NavContentType.RewardTree:
			sceneLoader.GoToRewardTreeScene(new RewardTreePageContext(_context.TrackName, null, null, NavContentType.RewardTrack));
			break;
		default:
			sceneLoader.GoToLanding(new HomePageContext());
			break;
		}
	}

	private void OnPagePrev()
	{
		_activeView.CurrentPage--;
	}

	private void OnPageNext()
	{
		_activeView.CurrentPage++;
	}

	private void Update()
	{
		float axis = Input.GetAxis("Mouse ScrollWheel");
		if (Mathf.Abs(axis) > 0.01f && Time.time - _lastScroll > _scrollSensitivity)
		{
			_lastScroll = Time.time;
			_activeView.CurrentPage += ((axis < 0f) ? 1 : (-1));
		}
	}

	private void UpdatePageControls()
	{
		_pageControls.PagePrevButton.interactable = _activeView.CurrentPage > 0;
		_pageControls.PageNextButton.interactable = _activeView.CurrentPage < _activeView.PagesCount - 1;
		_pageControls.PageOfText.SetText("MainNav/BattlePass/PageOf", new Dictionary<string, string>
		{
			{
				"current",
				(_activeView.CurrentPage + 1).ToString()
			},
			{
				"total",
				_activeView.PagesCount.ToString()
			}
		});
		int i;
		for (i = 0; i < _activeView.PagesCount; i++)
		{
			if (_pageControls.PageNavDotContainer.childCount <= i)
			{
				Button button = UnityEngine.Object.Instantiate(_pageControls.PageNavDotPrefab, _pageControls.PageNavDotContainer);
				int pageIndex = i;
				button.onClick.AddListener(delegate
				{
					OnNavDotClicked(pageIndex);
				});
			}
			Animator component = _pageControls.PageNavDotContainer.GetChild(i).GetComponent<Animator>();
			component.gameObject.UpdateActive(active: true);
			component.SetBool(Highlight, i == _activeView.CurrentPage);
		}
		for (; i < _pageControls.PageNavDotContainer.childCount; i++)
		{
			_pageControls.PageNavDotContainer.GetChild(i).gameObject.UpdateActive(active: false);
		}
	}

	private void OnNavDotClicked(int pageIndex)
	{
		_activeView.CurrentPage = pageIndex;
	}

	private void OnTrackDoneBeingViewed(NavContentType fromScene, string trackName)
	{
		DateTime utcNow = DateTime.UtcNow;
		TimeSpan duration = utcNow - _tabSelectionTimeForBI;
		_tabSelectionTimeForBI = utcNow;
		_logger.Send(ClientBusinessEventType.ProgressionTrackViewed, new ProgressionTrackViewed
		{
			FromSceneName = fromScene.ToString(),
			TrackName = trackName,
			Duration = duration,
			EventTime = utcNow
		});
	}
}
