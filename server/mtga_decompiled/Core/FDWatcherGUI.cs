using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Extensions;

public class FDWatcherGUI : IWatcherGUI, IDebugGUIPage
{
	private readonly MatchManager _matchManager;

	private readonly bool _isEditorGUI;

	private Vector2 _scrollPosition = Vector2.zero;

	private List<bool> _expandedMessages = new List<bool>();

	private bool _showOutgoing = true;

	private bool _showIncoming = true;

	private bool _showState = true;

	private bool _showUnmatched = true;

	private bool _showBinary = true;

	private bool _showText = true;

	private bool _newestFirst;

	private bool _excludeFilterTerms;

	private bool _filterFullText;

	private HashSet<HistoryEntry.MessageTag> _excludeTags = new HashSet<HistoryEntry.MessageTag>();

	private int _highlightId = -1;

	private HistoryEntry.MessageFormat _highlightType;

	private GUIStyle _guiStyle75PercentBlackBackground;

	private GUIStyle _guiStyleWordWrapWhite;

	private GUIStyle _guiStyleActiveTabButtonBk;

	public string _filterTextField = "";

	private bool _countHasUpdated = true;

	private List<HistoryEntry.MessageTag> _availableTags;

	private Dictionary<int, HistoryEntry> _incomingMessages;

	private Dictionary<int, HistoryEntry> _outgoingMessages;

	private IFDHistory _FDHistory;

	private List<HistoryEntry> _fdHistoryEntries;

	private GUIContent _clipboardButtonIcon;

	private string _connectionCategory = "FrontDoor";

	private EnvironmentDescription _cachedEnvironment;

	public DebugInfoIMGUIOnGui.DebugTab TabType => DebugInfoIMGUIOnGui.DebugTab.Watcher;

	public string TabName => "Watcher";

	public bool HiddenInTab => false;

	public FDWatcherGUI(MatchManager matchManager, bool isEditorGUI)
	{
		_matchManager = matchManager;
		_isEditorGUI = isEditorGUI;
		_newestFirst = InitNewestFirst();
		_clipboardButtonIcon = ContentForClipboardIcon();
	}

	public void Init(DebugInfoIMGUIOnGui gui)
	{
	}

	public void Destroy()
	{
	}

	public void OnQuit()
	{
	}

	public void Clear()
	{
		_expandedMessages.Clear();
		_FDHistory?.Clear();
		_highlightId = -1;
	}

	public bool OnUpdate()
	{
		if (Pantry.CurrentEnvironment != _cachedEnvironment)
		{
			_FDHistory = new FDAWSHistory(_matchManager);
			_cachedEnvironment = Pantry.CurrentEnvironment;
		}
		else
		{
			if (Pantry.CurrentEnvironment == null)
			{
				return false;
			}
			if (Pantry.CurrentEnvironment.HostPlatform == HostPlatform.Harness)
			{
				return false;
			}
		}
		bool result = _FDHistory.DoUpdate(_connectionCategory);
		_fdHistoryEntries = _FDHistory.HistoryEntries;
		return result;
	}

	public void OnGUI()
	{
		if (_guiStyle75PercentBlackBackground == null)
		{
			_guiStyle75PercentBlackBackground = new GUIStyle(GUI.skin.box);
			_guiStyle75PercentBlackBackground.normal.background = CreateSolidColorTexture(2, 2, new Color(0f, 0f, 0f, 0.75f));
		}
		if (_guiStyleWordWrapWhite == null)
		{
			_guiStyleWordWrapWhite = new GUIStyle
			{
				wordWrap = true,
				normal = new GUIStyleState
				{
					textColor = Color.white
				}
			};
		}
		if (_guiStyleActiveTabButtonBk == null)
		{
			_guiStyleActiveTabButtonBk = new GUIStyle(GUI.skin.button);
			_guiStyleActiveTabButtonBk.fontStyle = FontStyle.BoldAndItalic;
			_guiStyleActiveTabButtonBk.normal.textColor = Color.green;
			_guiStyleActiveTabButtonBk.active.textColor = Color.green;
			_guiStyleActiveTabButtonBk.hover.textColor = Color.green;
			_guiStyleActiveTabButtonBk.focused.textColor = Color.green;
		}
		if (GUILayout.Button($"Record Server History (Requires Reboot): {MDNPlayerPrefs.RecordServerMessageHistory}", GUILayout.MaxWidth(383f)))
		{
			MDNPlayerPrefs.RecordServerMessageHistory = !MDNPlayerPrefs.RecordServerMessageHistory;
		}
		GUILayout.BeginHorizontal();
		CreateWatcherSelectionTab("FrontDoor");
		CreateWatcherSelectionTab("MatchDoor");
		if (_FDHistory == null)
		{
			return;
		}
		if (_FDHistory.HasPreviousMatchHistoryConnection())
		{
			CreateWatcherSelectionTab("MatchDoor(Prev)");
		}
		GUILayout.EndHorizontal();
		if (!_FDHistory.HasConnection())
		{
			return;
		}
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Collapse All"))
		{
			_expandedMessages.Clear();
		}
		if (GUILayout.Button("Expand All"))
		{
			_expandedMessages.Clear();
			while (_expandedMessages.Count < _fdHistoryEntries.Count)
			{
				_expandedMessages.Add(item: true);
			}
		}
		GUILayout.EndHorizontal();
		if (GUILayout.Button("Clear"))
		{
			_expandedMessages.Clear();
			_FDHistory.ClearHistory();
		}
		bool flag = false;
		List<string> list = new List<string>();
		if (GUILayout.Button("Copy To Clipboard (as filtered)"))
		{
			flag = true;
		}
		if (_countHasUpdated)
		{
			while (_expandedMessages.Count < _fdHistoryEntries.Count)
			{
				_expandedMessages.Add(item: false);
			}
			_availableTags = (from v in _fdHistoryEntries.SelectMany((HistoryEntry v) => v.Tags).Distinct()
				orderby v.ToString()
				select v).ToList();
			_incomingMessages = _fdHistoryEntries.Where((HistoryEntry v) => v.Direction == HistoryEntry.MessageDirection.Incoming).DistinctBy((HistoryEntry v) => v.MessageId).ToDictionary((HistoryEntry v) => v.MessageId);
			_outgoingMessages = _fdHistoryEntries.Where((HistoryEntry v) => v.Direction == HistoryEntry.MessageDirection.Outgoing).DistinctBy((HistoryEntry v) => v.MessageId).ToDictionary((HistoryEntry v) => v.MessageId);
		}
		GUILayout.Label("Filter terms:");
		_filterTextField = GUILayout.TextField(_filterTextField);
		GUILayout.BeginHorizontal(_guiStyle75PercentBlackBackground);
		GUILayout.BeginVertical();
		_showIncoming = GUILayout.Toggle(_showIncoming, "Incoming");
		_showOutgoing = GUILayout.Toggle(_showOutgoing, "Outgoing");
		_showUnmatched = GUILayout.Toggle(_showUnmatched, "Unmatched");
		_showState = GUILayout.Toggle(_showState, "State/Misc");
		_newestFirst = DrawNewestFirst(_newestFirst);
		GUILayout.EndVertical();
		GUILayout.BeginVertical();
		_showText = GUILayout.Toggle(_showText, "Text");
		_showBinary = GUILayout.Toggle(_showBinary, "Binary");
		GUILayout.EndVertical();
		GUILayout.BeginVertical();
		_excludeFilterTerms = GUILayout.Toggle(_excludeFilterTerms, "Exclude filter terms");
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
		int num;
		int num2;
		int num3;
		if (_newestFirst)
		{
			num = _fdHistoryEntries.Count - 1;
			num2 = -1;
			num3 = -1;
		}
		else
		{
			num = 0;
			num2 = _fdHistoryEntries.Count;
			num3 = 1;
		}
		for (int num4 = num; num4 != num2; num4 += num3)
		{
			HistoryEntry historyEntry = _fdHistoryEntries[num4];
			string[] source = _filterTextField.Split(new string[1] { " " }, StringSplitOptions.None);
			source = source.Select((string str) => str.ToLower()).ToArray();
			bool flag2 = historyEntry.Direction == HistoryEntry.MessageDirection.Incoming;
			bool flag3 = historyEntry.Direction == HistoryEntry.MessageDirection.Outgoing;
			bool flag4 = historyEntry.Direction == HistoryEntry.MessageDirection.State;
			bool flag5 = historyEntry.Format == HistoryEntry.MessageFormat.Text;
			bool flag6 = (flag2 && _showIncoming) || (flag3 && _showOutgoing) || (flag4 && _showState);
			bool flag7 = (flag5 && _showText) || (!flag5 && _showBinary);
			bool flag8 = false;
			bool flag9 = false;
			bool flag10 = false;
			Dictionary<int, HistoryEntry> dictionary = (flag2 ? _outgoingMessages : _incomingMessages);
			HistoryEntry historyEntry2 = null;
			if (historyEntry.MessageId == 0)
			{
				flag8 = true;
			}
			else if (historyEntry.MessageId > 0 && dictionary.ContainsKey(historyEntry.MessageId))
			{
				historyEntry2 = dictionary[historyEntry.MessageId];
				dictionary.Remove(historyEntry.MessageId);
			}
			else
			{
				flag6 = _showUnmatched;
				flag9 = true;
			}
			bool flag11 = false;
			string text = (_filterFullText ? historyEntry.FullText : historyEntry.Name);
			if (text != null)
			{
				flag11 = ((!_excludeFilterTerms) ? source.All(text.ToLower().Contains) : (!source.All(text.ToLower().Contains)));
			}
			if (!(flag6 && flag7 && flag11))
			{
				continue;
			}
			bool flag12 = false;
			foreach (HistoryEntry.MessageTag tag in historyEntry.Tags)
			{
				if (_excludeTags.Contains(tag))
				{
					flag12 = true;
					break;
				}
				if (tag == HistoryEntry.MessageTag.Error)
				{
					flag10 = true;
				}
			}
			if (flag12)
			{
				continue;
			}
			GUILayout.BeginHorizontal();
			if (_expandedMessages[num4])
			{
				if (GUILayout.Button("-", GUILayout.Width(20f)))
				{
					_expandedMessages[num4] = false;
				}
			}
			else if (GUILayout.Button("+", GUILayout.Width(20f)))
			{
				_expandedMessages[num4] = true;
			}
			bool flag13 = historyEntry.MessageId == _highlightId && historyEntry.Format == _highlightType;
			Color color = GUI.color;
			GUI.color = (flag4 ? new Color(0.8f, 0.8f, 0.8f) : (flag2 ? new Color(0f, 0.5f, 1f) : new Color(0f, 1f, 0f)));
			if (flag9)
			{
				GUI.color = Color.Lerp(GUI.color, Color.red, 0.7f);
			}
			if (flag8)
			{
				GUI.color = Color.Lerp(GUI.color, Color.gray, 0.7f);
			}
			if (flag10)
			{
				GUI.color = Color.Lerp(GUI.color, Color.red, 0.7f);
			}
			string text2 = (flag4 ? "..." : ((!flag9) ? ((!flag8) ? ((historyEntry.Direction == HistoryEntry.MessageDirection.Incoming) ? "<==" : "==>") : ((historyEntry.Direction == HistoryEntry.MessageDirection.Incoming) ? "<---" : "--->")) : ((historyEntry.Direction == HistoryEntry.MessageDirection.Incoming) ? "<=/=" : "=/=>")));
			string text3 = "--";
			if (historyEntry.Direction == HistoryEntry.MessageDirection.Incoming && historyEntry2 != null)
			{
				text3 = $"{(int)(historyEntry.TimeReceived - historyEntry2.TimeReceived).TotalMilliseconds}ms";
			}
			if (GUILayout.Button(text2, GUILayout.Width(flag13 ? 80 : 40)))
			{
				_highlightId = historyEntry.MessageId;
				_highlightType = historyEntry.Format;
			}
			GUILayout.Label(text3, GUILayout.Width(50f));
			GUILayout.Label(BytesToHumanReadableString(historyEntry.MessageSize), GUILayout.Width(60f));
			GUI.color = color;
			DrawClipboardIcon(_clipboardButtonIcon, historyEntry.FullText);
			GUILayout.BeginVertical(_guiStyle75PercentBlackBackground);
			int num5 = historyEntry.MessageId;
			if (num5 < 0)
			{
				num5 = -num5;
			}
			string text4 = $"[{num5}] {historyEntry.Name}";
			GUILayout.Label(text4);
			if (_expandedMessages[num4])
			{
				GUILayout.TextArea((historyEntry.FullText.Length > 5000) ? (historyEntry.FullText.Substring(0, 5000) + "\n[TRUNCATED]") : historyEntry.FullText, _guiStyleWordWrapWhite);
				if (GUI.changed)
				{
					GUIUtility.keyboardControl = 0;
				}
			}
			if (flag)
			{
				list.Add(text2 + "  " + text4);
				list.Add(historyEntry.FullText);
			}
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}
		GUILayout.EndScrollView();
		if (flag)
		{
			GUIUtility.systemCopyBuffer = string.Join(Environment.NewLine, list.ToArray());
		}
	}

	private void CreateWatcherSelectionTab(string connectionCategory)
	{
		if (_connectionCategory == connectionCategory)
		{
			GUILayout.Button(connectionCategory, _guiStyleActiveTabButtonBk, GUILayout.Width(125f));
		}
		else if (GUILayout.Button(connectionCategory, GUILayout.Width(125f)))
		{
			_connectionCategory = connectionCategory;
		}
	}

	public static Texture2D CreateSolidColorTexture(int width, int height, Color col)
	{
		Color[] array = new Color[width * height];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = col;
		}
		Texture2D texture2D = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false);
		texture2D.SetPixels(array);
		texture2D.Apply();
		return texture2D;
	}

	public static GUIContent ContentForClipboardIcon()
	{
		return new GUIContent("Clip")
		{
			tooltip = "Copy To Clipboard"
		};
	}

	private static void DrawClipboardIcon(GUIContent guiContent, string textToCopy)
	{
		if (GUILayout.Button(guiContent, GUILayout.Width(40f)))
		{
			GUIUtility.systemCopyBuffer = textToCopy;
		}
	}

	private static bool InitNewestFirst()
	{
		if (PlayerPrefs.GetInt("FDWatcherNewestFirst", 0) == 0)
		{
			return false;
		}
		return true;
	}

	private static bool DrawNewestFirst(bool newestFirst)
	{
		bool flag = GUILayout.Toggle(newestFirst, "Newest First");
		if (flag != newestFirst)
		{
			newestFirst = flag;
			PlayerPrefs.SetInt("FDWatcherNewestFirst", newestFirst ? 1 : 0);
		}
		return flag;
	}

	private static string BytesToHumanReadableString(long byteCount)
	{
		string[] array = new string[7] { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
		if (byteCount == 0L)
		{
			return "0" + array[0];
		}
		long num = Math.Abs(byteCount);
		int num2 = Convert.ToInt32(Math.Floor(Math.Log(num, 1024.0)));
		double num3 = Math.Round((double)num / Math.Pow(1024.0, num2), 1);
		return (double)Math.Sign(byteCount) * num3 + array[num2];
	}
}
