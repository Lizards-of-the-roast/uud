using UnityEngine;
using Wotc.Mtga.AutoPlay;

public class AutoplayPageGUI : IDebugGUIPage
{
	private readonly AutoPlayManager _autoPlayManager;

	private readonly AutoPlayHolder _autoPlayHolder;

	private DebugInfoIMGUIOnGui _GUI;

	private string[] autoplayFiles = AutoPlayManager.GetAutoplayFileNames();

	private bool _logToGui = true;

	private int _highlightLogFontSize = MDNPlayerPrefs.DebugAutoplayHighlightLogFontSize;

	private bool _canRunAutoplay;

	private static Vector2 _scrollPosition;

	private AutoPlayScriptMetadata _selectedAutoPlayMetadata;

	private string _selectedAutoplayFile;

	private Vector2 _fileScrollPosition = Vector2.zero;

	public DebugInfoIMGUIOnGui.DebugTab TabType => DebugInfoIMGUIOnGui.DebugTab.Autoplay;

	public string TabName => "Autoplay";

	public bool HiddenInTab => false;

	public AutoplayPageGUI(AutoPlayHolder autoPlayHolder)
	{
		_autoPlayHolder = autoPlayHolder;
		_autoPlayManager = _autoPlayHolder.AutoPlayManager;
	}

	public void Init(DebugInfoIMGUIOnGui gui)
	{
		_GUI = gui;
		_canRunAutoplay = AutoPlayManager.CanRunAutoPlay();
	}

	public void Destroy()
	{
	}

	public void OnQuit()
	{
	}

	public bool OnUpdate()
	{
		return true;
	}

	public void OnGUI()
	{
		if (_GUI.ShowDebugButton("Open Confluence", 500f))
		{
			Application.OpenURL("https://wizardsofthecoast.atlassian.net/wiki/display/MDN/Instructions+for+Running+MTGA+Autoplay");
		}
		if (_canRunAutoplay && _autoPlayManager != null)
		{
			DrawAutoplayGUI(_autoPlayManager);
		}
		else
		{
			_GUI.ShowLabel("Autoplay cannot be started. Please check files are set up.");
		}
	}

	private void DrawAutoplayGUI(AutoPlayManager autoPlayManager)
	{
		GUILayout.Label("Queued file: " + MDNPlayerPrefs.QueuedAutoplayFile, GUILayout.MaxWidth(520f));
		if (_GUI.ShowDebugButton("Clear Queued", 500f))
		{
			MDNPlayerPrefs.QueuedAutoplayFile = null;
		}
		if (_GUI.ShowDebugButton("Try Compile All", 500f))
		{
			string[] array = autoplayFiles;
			for (int i = 0; i < array.Length; i++)
			{
				AutoPlayScriptFactory.CreateAutoPlayScript(array[i], delegate
				{
				}, null, autoPlayManager);
			}
		}
		GUILayout.BeginHorizontal();
		if (autoPlayManager != null && !autoPlayManager.IsRunning)
		{
			GUILayout.BeginVertical(GUILayout.MaxWidth(520f));
			_logToGui = _GUI.ShowToggle(_logToGui, "Log To GUI");
			if (_logToGui)
			{
				int highlightLogFontSize = _highlightLogFontSize;
				_highlightLogFontSize = _GUI.ShowInputField("Highlight Log Font Size (1 is off)", _highlightLogFontSize, 250);
				if (highlightLogFontSize != _highlightLogFontSize)
				{
					MDNPlayerPrefs.DebugAutoplayHighlightLogFontSize = _highlightLogFontSize;
				}
			}
			_fileScrollPosition = _GUI.BeginScrollView(_fileScrollPosition);
			string[] array = autoplayFiles;
			foreach (string text in array)
			{
				if (_GUI.ShowDebugButton(text, 500f))
				{
					_selectedAutoplayFile = text;
					_selectedAutoPlayMetadata = AutoPlayScriptFactory.CreateAutoplayMetadata(text);
				}
			}
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
		}
		DrawActionPanel();
		GUILayout.BeginVertical(GUILayout.MaxWidth(720f));
		_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
		foreach (string item in autoPlayManager?.GuiLogs)
		{
			GUILayout.Label(item);
		}
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
	}

	private void DrawActionPanel()
	{
		if (_selectedAutoplayFile != null)
		{
			GUILayout.BeginVertical(GUILayout.MaxWidth(520f));
			GUILayout.Label("File: " + _selectedAutoplayFile);
			GUILayout.Label("Name: " + _selectedAutoPlayMetadata.Name);
			GUILayout.Label("Description: " + _selectedAutoPlayMetadata.Description);
			GUILayout.BeginHorizontal();
			if (_selectedAutoPlayMetadata.CanRunImmediate && _GUI.ShowDebugButton("Play Immediate", 500f))
			{
				_autoPlayHolder?.SetFontSize(_highlightLogFontSize);
				_autoPlayManager?.StartScript(_selectedAutoplayFile, _logToGui);
				_selectedAutoplayFile = null;
			}
			if (_selectedAutoPlayMetadata.CanRunOnRestart && _GUI.ShowDebugButton("Play on Restart", 500f))
			{
				MDNPlayerPrefs.QueuedAutoplayFile = _selectedAutoplayFile;
				_selectedAutoplayFile = null;
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		}
	}
}
