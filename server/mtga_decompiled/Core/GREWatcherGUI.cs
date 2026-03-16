using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf.Collections;
using GreClient.History;
using GreClient.Test;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pooling;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

public class GREWatcherGUI : IWatcherGUI, IDebugGUIPage
{
	public class GREWatcherMessage
	{
		public GreHistoryEntry HistoryEntry;

		public GREToClientMessage ToClient;

		public ClientToGREMessage ToGRE;

		public string RawText;

		public string FullText;

		public uint MsgId;

		public string TypeString;

		public bool Expanded;

		public string CustomText = "unset";

		public MessageDirection Direction => HistoryEntry.Direction;

		public GREWatcherMessage(GreHistoryEntry entry)
		{
			HistoryEntry = entry;
			ToClient = HistoryEntry.ActualMessage as GREToClientMessage;
			ToGRE = HistoryEntry.ActualMessage as ClientToGREMessage;
			RawText = entry.ActualMessage.ToString();
			if (entry.FullGameStateMessage != null)
			{
				FullText = entry.FullGameStateMessage.ToString();
			}
			else
			{
				FullText = RawText;
			}
			if (ToClient != null)
			{
				MsgId = ToClient.MsgId;
				TypeString = ToClient.Type.ToString();
			}
			else if (ToGRE != null)
			{
				MsgId = ToGRE.RespId;
				TypeString = ToGRE.Type.ToString();
			}
		}
	}

	private readonly MatchManager _matchManager;

	private readonly bool _isEditorGUI;

	private readonly Func<CardDatabase> _getCardDatabase;

	private readonly IObjectPool _pool = new ObjectPool();

	private Vector2 m_scrollPosition = Vector2.zero;

	private int m_lastNumMessages;

	private List<GREWatcherMessage> m_messages = new List<GREWatcherMessage>();

	private int m_lastHistorySize;

	private string _searchText = string.Empty;

	private Font _font;

	private GUIStyle _style = new GUIStyle();

	private GUIStyle _highlightStyle = new GUIStyle();

	public DebugInfoIMGUIOnGui.DebugTab TabType => DebugInfoIMGUIOnGui.DebugTab.Watcher;

	public string TabName => "GRE Watcher";

	public bool HiddenInTab => true;

	public GREWatcherGUI(MatchManager matchManager, bool isEditorGUI, Func<CardDatabase> getCardDatabase)
	{
		_matchManager = matchManager;
		_isEditorGUI = isEditorGUI;
		_getCardDatabase = getCardDatabase;
		LoadFont();
	}

	public void Init(DebugInfoIMGUIOnGui gui)
	{
	}

	public void Destroy()
	{
		Shutdown();
	}

	public void OnQuit()
	{
	}

	private void LoadFont()
	{
		_font = Resources.Load("VeraMono") as Font;
	}

	public void Shutdown()
	{
		Resources.UnloadAsset(_font);
	}

	public void Reset()
	{
		Shutdown();
		LoadFont();
		m_messages.ForEach(delegate(GREWatcherMessage m)
		{
			m.Expanded = false;
		});
		m_messages.Clear();
		m_lastHistorySize = 0;
	}

	public bool OnUpdate()
	{
		try
		{
			MessageHistory messageHistory = _matchManager.MessageHistory;
			if (messageHistory == null)
			{
				return false;
			}
			List<GreHistoryEntry> history = messageHistory.History;
			for (int i = m_lastHistorySize; i < history.Count; i++)
			{
				m_messages.Add(new GREWatcherMessage(history[i]));
			}
			m_lastHistorySize = history.Count;
			if (m_messages.Count != m_lastNumMessages)
			{
				m_lastNumMessages = m_messages.Count;
				return true;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("GRE Watcher blew up: " + ex);
		}
		return false;
	}

	public void OnGUI()
	{
		try
		{
			GUILayout.BeginVertical(GUI.skin.box);
			_style = new GUIStyle(GUI.skin.label);
			_style.font = _font;
			_style.fontSize = 12;
			_highlightStyle = new GUIStyle(GUI.skin.label);
			_highlightStyle.normal.textColor = UnityEngine.Color.green;
			_highlightStyle.font = _font;
			_highlightStyle.fontSize = 12;
			_highlightStyle.fontStyle = FontStyle.Bold;
			_ = GUI.skin.font;
			if (GUILayout.Button("Collapse All"))
			{
				m_messages.ForEach(delegate(GREWatcherMessage m)
				{
					m.Expanded = false;
				});
			}
			if (GUILayout.Button("Clear"))
			{
				m_messages.Clear();
			}
			GUILayout.Space(5f);
			GUIStyle textField = GUI.skin.textField;
			GUILayout.Label("Text Filter", textField);
			GUILayout.BeginHorizontal();
			GUIStyle style = GUI.skin.textField;
			if (_isEditorGUI)
			{
				style = GUI.skin.FindStyle("ToolbarSeachTextField");
			}
			_searchText = GUILayout.TextField(_searchText, style);
			GUIStyle style2 = GUI.skin.button;
			string text = "Cancel";
			if (_isEditorGUI)
			{
				text = string.Empty;
				style2 = GUI.skin.FindStyle("ToolbarSeachCancelButton");
			}
			if (GUILayout.Button(text, style2))
			{
				_searchText = string.Empty;
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(5f);
			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical();
			bool flag = DrawBooleanPlayerPrefToggle("GREWatcher_showState", "Show Parsed GameState");
			bool flag2 = DrawBooleanPlayerPrefToggle("GREWatcher_showAnnotations", "Annotations");
			bool flag3 = DrawBooleanPlayerPrefToggle("GREWatcher__showIdChanges", "ID Changes");
			GUILayout.EndVertical();
			GUILayout.BeginVertical();
			bool flag4 = DrawBooleanPlayerPrefToggle("GREWatcher_showIncoming", "Incoming");
			bool flag5 = DrawBooleanPlayerPrefToggle("GREWatcher_showOutgoing", "Outgoing");
			GUILayout.EndVertical();
			GUILayout.BeginVertical();
			bool flag6 = DrawBooleanPlayerPrefToggle("GREWatcher_showRawText", "Show Raw Text");
			bool flag7 = DrawBooleanPlayerPrefToggle("GREWatcher_showFullText", "Show Full Text");
			GUILayout.EndVertical();
			GUILayout.BeginVertical();
			bool flag8 = DrawBooleanPlayerPrefToggle("GREWatcher_showGameStateMessages", "Show Game State Messages");
			bool flag9 = DrawBooleanPlayerPrefToggle("GREWatcher_showOtherMessages", "Show Other Messages");
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			GUILayout.Space(10f);
			m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition);
			for (int num = 0; num < m_messages.Count; num++)
			{
				GREWatcherMessage gREWatcherMessage = m_messages[num];
				if ((gREWatcherMessage.Direction == MessageDirection.Incoming && !flag4) || (gREWatcherMessage.Direction == MessageDirection.Outgoing && !flag5) || (gREWatcherMessage.HistoryEntry.GameState == null && !flag9) || (gREWatcherMessage.HistoryEntry.GameState != null && !flag8))
				{
					continue;
				}
				GUILayout.BeginHorizontal();
				if (gREWatcherMessage.Expanded)
				{
					if (GUILayout.Button("-", GUILayout.Width(20f)))
					{
						gREWatcherMessage.Expanded = false;
					}
				}
				else if (GUILayout.Button("+", GUILayout.Width(20f)))
				{
					gREWatcherMessage.Expanded = true;
					if (gREWatcherMessage.HistoryEntry.GameState != null)
					{
						CardDatabase cardDatabase = _getCardDatabase?.Invoke();
						if (cardDatabase != null)
						{
							gREWatcherMessage.CustomText = GreTestUtils.PrintGameState(gREWatcherMessage.HistoryEntry.GameState, cardDatabase);
						}
					}
				}
				string text3;
				if (gREWatcherMessage.HistoryEntry.GameState != null)
				{
					RepeatedField<AnnotationInfo> annotations = (gREWatcherMessage.HistoryEntry.ActualMessage as GREToClientMessage).GameStateMessage.Annotations;
					int num2 = annotations.Count((AnnotationInfo x) => x.Type.First() == AnnotationType.ObjectIdChanged);
					string text2 = ((gREWatcherMessage.HistoryEntry.GameState.CurrentStep == Step.None) ? gREWatcherMessage.HistoryEntry.GameState.CurrentPhase.ToString() : gREWatcherMessage.HistoryEntry.GameState.CurrentStep.ToString());
					text3 = string.Format("{0}{1,4} {2,-20} {3,3}.{4, -16} (a:{5} i:{6})", (gREWatcherMessage.Direction == MessageDirection.Incoming) ? "<--" : "-->", gREWatcherMessage.MsgId.ToString(), gREWatcherMessage.TypeString.Replace("GREMessageType_", "").Replace("ClientMessageType_", ""), gREWatcherMessage.HistoryEntry.GameState.GameWideTurn, text2, annotations.Count.ToString(), num2.ToString());
				}
				else
				{
					text3 = string.Format("{0}{1,4} {2,-20}", (gREWatcherMessage.Direction == MessageDirection.Incoming) ? "<--" : "-->", gREWatcherMessage.MsgId.ToString(), gREWatcherMessage.TypeString.Replace("GREMessageType_", "").Replace("ClientMessageType_", ""));
				}
				bool flag10 = !string.IsNullOrEmpty(_searchText) && gREWatcherMessage.RawText.ToLower().Contains(_searchText.ToLower());
				GUILayout.Label(text3, flag10 ? _highlightStyle : _style);
				GUILayout.EndHorizontal();
				if (!gREWatcherMessage.Expanded)
				{
					continue;
				}
				if (gREWatcherMessage.HistoryEntry.GameState == null || flag)
				{
					GUILayout.Label(gREWatcherMessage.CustomText, _style);
				}
				if (gREWatcherMessage.HistoryEntry.GameState != null)
				{
					RepeatedField<AnnotationInfo> annotations2 = (gREWatcherMessage.HistoryEntry.ActualMessage as GREToClientMessage).GameStateMessage.Annotations;
					if (flag2)
					{
						foreach (AnnotationInfo item in annotations2)
						{
							GUILayout.Label(AnnotationToString(item));
						}
						RepeatedField<AnnotationInfo> persistentAnnotations = gREWatcherMessage.HistoryEntry.FullGameStateMessage.PersistentAnnotations;
						if (persistentAnnotations.Count > 0)
						{
							GUILayout.Space(2f);
							GUILayout.Label("Persistent Annotations:");
							GUILayout.BeginVertical(GUI.skin.box);
							foreach (AnnotationInfo item2 in persistentAnnotations)
							{
								GUILayout.Label(AnnotationToString(item2));
							}
							GUILayout.EndVertical();
						}
						if (flag3)
						{
							foreach (AnnotationInfo item3 in annotations2)
							{
								if (item3.Type.First() == AnnotationType.ObjectIdChanged)
								{
									uint num3 = (uint)item3.Details.First((KeyValuePairInfo kvpi) => kvpi.Key.Equals("new_id")).ValueInt32[0];
									GUILayout.Label("   " + (uint)item3.Details.First((KeyValuePairInfo kvpi) => kvpi.Key.Equals("orig_id")).ValueInt32[0] + " -> " + num3);
								}
							}
						}
					}
				}
				if (flag6)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label("Raw Text:", GUILayout.Width(70f));
					string text4 = JObject.Parse(gREWatcherMessage.RawText).ToString(Formatting.Indented);
					if (GUILayout.Button("Copy", GUILayout.Width(45f)))
					{
						TextEditor textEditor = new TextEditor();
						textEditor.text = text4;
						textEditor.SelectAll();
						textEditor.Copy();
						GUILayout.Label("Copied!");
					}
					GUILayout.EndHorizontal();
					GUILayout.TextArea(text4);
					if (GUI.changed)
					{
						GUIUtility.keyboardControl = 0;
					}
				}
				if (flag7)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label("Full Text:", GUILayout.Width(70f));
					string text5 = JObject.Parse(gREWatcherMessage.FullText).ToString(Formatting.Indented);
					if (GUILayout.Button("Copy", GUILayout.Width(45f)))
					{
						TextEditor textEditor2 = new TextEditor();
						textEditor2.text = text5;
						textEditor2.SelectAll();
						textEditor2.Copy();
						GUILayout.Label("Copied!");
					}
					GUILayout.EndHorizontal();
					GUILayout.TextArea(text5);
					if (GUI.changed)
					{
						GUIUtility.keyboardControl = 0;
					}
				}
			}
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
		}
		catch (Exception ex)
		{
			Debug.LogError("GRE Watcher blew up: " + ex);
		}
		static bool DrawBooleanPlayerPrefToggle(string playerPrefName, string label)
		{
			bool num4 = PlayerPrefsExt.GetInt(playerPrefName, 1) == 1;
			bool flag11 = GUILayout.Toggle(num4, label);
			if (num4 != flag11)
			{
				PlayerPrefsExt.SetInt(playerPrefName, flag11 ? 1 : 0);
			}
			return flag11;
		}
	}

	private string AnnotationToString(AnnotationInfo a)
	{
		StringBuilder stringBuilder = _pool.PopObject<StringBuilder>();
		stringBuilder.Append(string.Join(",", a.Type.Select((AnnotationType x) => x.ToString()).ToArray()));
		stringBuilder.Append(" ").Append(a.AffectorId).Append(" affects ");
		stringBuilder.Append(string.Join(",", a.AffectedIds.Select((uint x) => x.ToString()).ToArray()));
		stringBuilder.Append(" (ID ").Append(a.Id.ToString()).Append(")");
		foreach (KeyValuePairInfo detail in a.Details)
		{
			stringBuilder.Append("\n   ").Append(detail.Key).Append(" = ")
				.Append(AnnotationValueToString(detail));
		}
		string result = stringBuilder.ToString();
		stringBuilder.Clear();
		_pool.PushObject(stringBuilder);
		return result;
	}

	private string AnnotationValueToString(KeyValuePairInfo detail)
	{
		return string.Join(",", KvpiStrings(detail));
	}

	private IEnumerable<string> KvpiStrings(KeyValuePairInfo detail)
	{
		return detail.Type switch
		{
			KeyValuePairValueType.Bool => detail.ValueBool.Select((bool x) => x.ToString()), 
			KeyValuePairValueType.Int32 => detail.ValueInt32.Select((int x) => x.ToString()), 
			KeyValuePairValueType.Int64 => detail.ValueInt64.Select((long x) => x.ToString()), 
			KeyValuePairValueType.String => detail.ValueString, 
			KeyValuePairValueType.Uint32 => detail.ValueUint32.Select((uint x) => x.ToString()), 
			KeyValuePairValueType.Uint64 => detail.ValueUint64.Select((ulong x) => x.ToString()), 
			KeyValuePairValueType.Float => detail.ValueUint32.Select((uint x) => x.ToString()), 
			KeyValuePairValueType.Double => detail.ValueUint64.Select((ulong x) => x.ToString()), 
			_ => Array.Empty<string>(), 
		};
	}
}
