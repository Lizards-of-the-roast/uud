using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Core.Shared.Code.DebugTools;
using Unity.VisualScripting;
using UnityEngine;
using Wizards.Mtga;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.AutoPlay;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Replays;

public class DebugInfoIMGUIOnGui : MonoBehaviour
{
	public enum DebugTab
	{
		General,
		Hacks,
		Watcher,
		Connection,
		LocHacks,
		AudioWatcher,
		DuelScene,
		DraftBot,
		Analysis,
		Events,
		Quality,
		Autoplay,
		Replays,
		PlayerInbox,
		CampaignGraphMilestones,
		Achievements,
		PVPChallenges,
		Lobbies,
		Tournaments,
		ExternalChat,
		DebugChat,
		Gatherings,
		ChallengeService
	}

	public string _clipboardString = string.Empty;

	[NonSerialized]
	public int ButtonHeight;

	private GUIStyle _activeTabButtonStyle;

	private GUIStyle _horizontalSliderStyle;

	private GUIStyle _toggleStyle;

	private GUIStyle _sliderThumbStyle;

	private GUIStyle _redTextStyle;

	private GUIStyle _carouselTimeStyle;

	private int tabHeight;

	private const int tabWidth = 100;

	private const int tabTopspacing = 60;

	private const int tabBottomSpacing = 15;

	private readonly string[] tabs = Enum.GetNames(typeof(DebugTab));

	private List<IDebugGUIPage> _debugPages = new List<IDebugGUIPage>();

	private IDebugGUIPage _currentPage;

	private DraftBotGui _draftBotPageGUI;

	private IWatcherGUI _currentWatcherGUI;

	private GREWatcherGUI _greWatcherPageGUI;

	private FDWatcherGUI _fdWatcherPageGUI;

	private AssetBundleWatcherGUI _assetBundleWatcherPageGUI;

	private AutoplayPageGUI _autoplayPageGui;

	private AudioWatcher _audioWindow;

	private Vector2 _currentTabScrollPos = Vector2.zero;

	public bool DebugGUILocked { get; set; }

	public event Action<bool> StayOpenToggled;

	public void Init(PAPA papa)
	{
		ButtonHeight = (PlatformUtils.IsHandheld() ? 30 : 20);
		tabHeight = ButtonHeight;
		_audioWindow = new GameObject("AudioWatcher").AddComponent<AudioWatcher>();
		Transform obj = _audioWindow.transform;
		obj.ZeroOut();
		obj.SetParent(base.gameObject.transform);
		_debugPages.Add(new GeneralPageGUI());
		_debugPages.Add(new HacksPageGUI(papa));
		_debugPages.Add(new AnalysisPageGUI());
		_debugPages.Add(new DuelScenePageGUI());
		_debugPages.Add(new LocHackGui());
		_debugPages.Add(new ConnectionGUI());
		_debugPages.Add(new EventPageGUI());
		_debugPages.Add(new QualityPageGUI());
		_debugPages.Add(new AudioWatcherPageGUI(_audioWindow));
		_debugPages.Add(new ReplayGUI());
		_debugPages.Add(new PlayerInboxPageGUI());
		_debugPages.Add(new CampaignGraphMilestonesGui());
		_debugPages.Add(new AchievementsPageGUI());
		_debugPages.Add(new PVPChallengePageGUI());
		_debugPages.Add(new LobbyTablePageGUI());
		_debugPages.Add(new TournamentPageGUI());
		_debugPages.Add(new DebugChatWindowGUI(papa.ExternalChatManager));
		_debugPages.Add(new DebugGatheringWindowGUI(papa.ExternalChatManager));
		_debugPages.Add(new ChallengeServiceGUI());
		InitAutoPlay();
		_assetBundleWatcherPageGUI = new AssetBundleWatcherGUI(isEditorGUI: false);
		_debugPages.Add(_assetBundleWatcherPageGUI);
		_fdWatcherPageGUI = new FDWatcherGUI(papa.MatchManager, isEditorGUI: false);
		_debugPages.Add(_fdWatcherPageGUI);
		_draftBotPageGUI = new DraftBotGui();
		_debugPages.Add(_draftBotPageGUI);
		_greWatcherPageGUI = new GREWatcherGUI(papa.MatchManager, isEditorGUI: false, () => Pantry.Get<CardDatabase>());
		_debugPages.Add(_greWatcherPageGUI);
		foreach (IDebugGUIPage debugPage in _debugPages)
		{
			debugPage.Init(this);
		}
		_currentWatcherGUI = _fdWatcherPageGUI;
		_currentPage = _debugPages[0];
	}

	public void InitAutoPlay()
	{
		if (_autoplayPageGui == null)
		{
			AutoPlayHolder autoPlayHolder = UnityEngine.Object.FindObjectOfType<AutoPlayHolder>();
			if (autoPlayHolder != null)
			{
				_autoplayPageGui = new AutoplayPageGUI(autoPlayHolder);
				_autoplayPageGui.Init(this);
				_debugPages.Add(_autoplayPageGui);
			}
			else
			{
				Debug.Log("Could not find autoplay holder, autoplay disabled");
			}
		}
	}

	private void OnDestroy()
	{
		foreach (IDebugGUIPage debugPage in _debugPages)
		{
			debugPage.Destroy();
		}
		if ((bool)_audioWindow && (bool)_audioWindow.gameObject)
		{
			UnityEngine.Object.Destroy(_audioWindow.gameObject);
			_audioWindow = null;
		}
	}

	private void OnApplicationQuit()
	{
		foreach (IDebugGUIPage debugPage in _debugPages)
		{
			debugPage.OnQuit();
		}
		UnityEngine.Object.Destroy(this);
	}

	private void Update()
	{
		_currentPage.OnUpdate();
		if (_currentPage.TabType == DebugTab.Watcher && _currentWatcherGUI is IDebugGUIPage debugGUIPage)
		{
			debugGUIPage.OnUpdate();
		}
	}

	private void SetupGUIStyles()
	{
		if (_activeTabButtonStyle == null)
		{
			_activeTabButtonStyle = new GUIStyle(GUI.skin.button)
			{
				fontStyle = FontStyle.BoldAndItalic
			};
			_activeTabButtonStyle.normal.textColor = Color.green;
			_activeTabButtonStyle.active.textColor = Color.green;
			_activeTabButtonStyle.hover.textColor = Color.green;
			_activeTabButtonStyle.focused.textColor = Color.green;
		}
		if (_horizontalSliderStyle == null)
		{
			_horizontalSliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
		}
		if (_sliderThumbStyle == null)
		{
			_sliderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb)
			{
				fixedWidth = 30f,
				fixedHeight = 30f
			};
		}
		if (_redTextStyle == null)
		{
			_redTextStyle = new GUIStyle();
			_redTextStyle.normal.textColor = Color.red;
			_redTextStyle.active.textColor = Color.red;
			_redTextStyle.hover.textColor = Color.red;
			_redTextStyle.focused.textColor = Color.red;
		}
		if (_carouselTimeStyle == null)
		{
			_carouselTimeStyle = new GUIStyle(GUI.skin.GetStyle("TextField"));
		}
		if (_toggleStyle == null)
		{
			_toggleStyle = new GUIStyle(GUI.skin.toggle)
			{
				border = new RectOffset(0, 0, 0, 0),
				overflow = new RectOffset(0, 0, 0, 0),
				imagePosition = ImagePosition.ImageOnly,
				padding = new RectOffset(50, 0, 50, 0)
			};
		}
	}

	private void OnGUI()
	{
		SetupGUIStyles();
		GUI.matrix = PlatformContext.GetIMGUIScale();
		GUILayout.Space(60f);
		int num = DrawTabs();
		GUILayout.Space(15f);
		_currentTabScrollPos = GUILayout.BeginScrollView(_currentTabScrollPos, GUILayout.Width((float)Screen.width / GUI.matrix.lossyScale.x), GUILayout.Height((float)Screen.height / GUI.matrix.lossyScale.y - 75f - (float)Mathf.CeilToInt((float)Enum.GetNames(typeof(DebugTab)).Length / (float)num) * 30f));
		if (_currentPage.TabType == DebugTab.Watcher)
		{
			GUILayout.BeginHorizontal();
			DrawWatcherTypeButton("FD Watcher", _fdWatcherPageGUI);
			DrawWatcherTypeButton(_greWatcherPageGUI.TabName, _greWatcherPageGUI);
			DrawWatcherTypeButton(_assetBundleWatcherPageGUI.TabName, _assetBundleWatcherPageGUI);
			GUILayout.EndHorizontal();
			if (_currentWatcherGUI is IDebugGUIPage debugGUIPage)
			{
				debugGUIPage.OnGUI();
			}
		}
		else
		{
			_currentPage.OnGUI();
		}
		GUILayout.EndScrollView();
		GUI.matrix = Matrix4x4.identity;
	}

	private int DrawTabs()
	{
		int num = Mathf.FloorToInt(((float)Screen.width / GUI.matrix.lossyScale.x - 100f) / 100f);
		GUILayout.BeginHorizontal();
		int i;
		for (i = 0; i < tabs.Length; i++)
		{
			if (i > 0 && i % num == 0)
			{
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
			}
			ShowTabButton(_debugPages.Find((IDebugGUIPage x) => x.TabType == (DebugTab)Enum.Parse(typeof(DebugTab), tabs[i]) && !x.HiddenInTab));
		}
		if (GUILayout.Button("Help", GUILayout.Width(100f), GUILayout.Height(tabHeight)))
		{
			Application.OpenURL("https://wizardsofthecoast.atlassian.net/wiki/spaces/MDN/pages/291538337/Arena+In-game+debug+tools");
		}
		bool debugGUILocked = DebugGUILocked;
		DebugGUILocked = ShowToggle(DebugGUILocked, "Stay Open", GUILayout.Width(100f));
		if (debugGUILocked != DebugGUILocked)
		{
			this.StayOpenToggled?.Invoke(DebugGUILocked);
		}
		GUILayout.EndHorizontal();
		return num;
	}

	private void ShowTabButton(IDebugGUIPage newPage)
	{
		if (newPage != null)
		{
			if (_currentPage == newPage)
			{
				GUILayout.Button(newPage.TabName, _activeTabButtonStyle, GUILayout.Width(100f), GUILayout.Height(tabHeight));
			}
			else if (GUILayout.Button(newPage.TabName, GUILayout.Width(100f), GUILayout.Height(tabHeight)))
			{
				_currentPage = newPage;
			}
		}
	}

	private void DrawWatcherTypeButton(string label, IWatcherGUI watcher)
	{
		if (GUILayout.Button(label, (_currentWatcherGUI == watcher) ? _activeTabButtonStyle : GUI.skin.button, GUILayout.Width(125f)))
		{
			_currentWatcherGUI = watcher;
		}
	}

	public bool ShowDebugButton(string label, float maxWidth = 500f, params GUILayoutOption[] options)
	{
		return GUILayout.Button(label, options.Append(GUILayout.MaxWidth(maxWidth)).Append(GUILayout.Height(ButtonHeight)).ToArray());
	}

	public void ShowLabelInfo(string label, string content)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(label, GUILayout.Width(100f));
		GUILayout.TextField(content ?? "(null)", GUILayout.MaxWidth(500f));
		if (GUILayout.Button("Copy", GUILayout.Width(100f)))
		{
			GUIUtility.systemCopyBuffer = content;
		}
		GUILayout.EndHorizontal();
		_clipboardString = _clipboardString + label + " " + content + Environment.NewLine;
	}

	public void ShowLabel(string label, float width)
	{
		GUILayout.Label(label, GUILayout.Width(width));
	}

	public void ShowLabel(string label, params GUILayoutOption[] options)
	{
		GUILayout.Label(label, options);
	}

	public float ShowHorizontalPercentSlider(float value)
	{
		GUILayout.BeginHorizontal();
		value = GUILayout.HorizontalSlider(value, 0f, 1f, _horizontalSliderStyle, _sliderThumbStyle, GUILayout.MaxWidth(500f));
		GUILayout.Label((value * 100f).ToString());
		GUILayout.EndHorizontal();
		return value;
	}

	public bool ShowToggle(bool value, string label, params GUILayoutOption[] options)
	{
		return GUILayout.Toggle(value, label, options.Append(GUILayout.Height(ButtonHeight)).ToArray());
	}

	public bool ShowToggleWithStyle(bool value, string label, params GUILayoutOption[] options)
	{
		return GUILayout.Toggle(value, label, _toggleStyle, options);
	}

	public int ShowToolbar(int value, string[] options)
	{
		return GUILayout.Toolbar(value, options, GUILayout.Height(ButtonHeight));
	}

	public void ShowBox(string text, params GUILayoutOption[] options)
	{
		GUILayout.Box(text, options);
	}

	public string ShowTextField(string text)
	{
		return GUILayout.TextField(text);
	}

	public string ShowCarouselTextField(string text)
	{
		return GUILayout.TextField(text, _carouselTimeStyle);
	}

	public T ShowInputField<T>(string label, T origValue, int labelWidth = 100)
	{
		GUILayout.BeginHorizontal();
		ShowLabel(label, labelWidth);
		string text = ShowTextField(origValue.ToString());
		T result = origValue;
		TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
		if (converter != null && converter.IsValid(text))
		{
			result = (T)converter.ConvertFromString(text);
		}
		GUILayout.EndHorizontal();
		return result;
	}

	public Vector2 BeginScrollView(Vector2 position, params GUILayoutOption[] options)
	{
		GUI.skin.verticalScrollbar.fixedWidth = ButtonHeight;
		GUI.skin.verticalScrollbarThumb.fixedWidth = ButtonHeight;
		return GUILayout.BeginScrollView(position, options);
	}

	public void SetCarouselTextStyleColour(bool isGreen)
	{
		if (isGreen)
		{
			_carouselTimeStyle.normal.textColor = Color.green;
			_carouselTimeStyle.hover.textColor = Color.green;
			_carouselTimeStyle.active.textColor = Color.green;
			_carouselTimeStyle.focused.textColor = Color.green;
		}
		else
		{
			_carouselTimeStyle.normal.textColor = Color.red;
			_carouselTimeStyle.hover.textColor = Color.red;
			_carouselTimeStyle.active.textColor = Color.red;
			_carouselTimeStyle.focused.textColor = Color.red;
		}
	}

	public int SelectionListUtility(string selectionListName, int selectedIndex, string[] options, ref Vector2 scrollPosition, params GUILayoutOption[] guiLayoutOptions)
	{
		int result = selectedIndex;
		GUILayout.BeginVertical(GUI.skin.box, guiLayoutOptions);
		GUILayout.Label("---------" + selectionListName + "---------");
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
		for (int i = 0; i < options.Length; i++)
		{
			if (GUILayout.Button(((i == selectedIndex) ? ">" : "") + options[i]))
			{
				result = i;
				break;
			}
		}
		GUILayout.EndScrollView();
		GUILayout.Label("---------------------------");
		GUILayout.EndVertical();
		return result;
	}

	public T SelectEnumUtility<T>(string selectionListName, T selectedEnumeration, ref Vector2 scrollPosition, params GUILayoutOption[] guiLayoutOptions) where T : Enum
	{
		string[] names = Enum.GetNames(typeof(T));
		int selectedIndex = Array.IndexOf(names, Enum.GetName(typeof(T), selectedEnumeration));
		GUILayoutOption[] array = new GUILayoutOption[2]
		{
			GUILayout.MinHeight((float)(names.Length + 3) * GUI.skin.button.lineHeight + GUI.skin.label.lineHeight * 2f),
			GUILayout.ExpandHeight(expand: true)
		};
		if (guiLayoutOptions.Length != 0)
		{
			array.AddRange(guiLayoutOptions);
		}
		selectedIndex = SelectionListUtility(selectionListName, selectedIndex, names, ref scrollPosition, array);
		return (T)Enum.GetValues(typeof(T)).GetValue(selectedIndex);
	}
}
