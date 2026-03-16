using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using Wizards.Arena.Enums.Format;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Mtga.Platforms;
using Wizards.Unification.Models.Event;
using Wizards.Unification.Models.Events;
using Wotc.Mtga.Network.ServiceWrappers;

public class DraftBotGui : IDebugGUIPage
{
	private class DraftBotEntry
	{
		public DraftBot Bot;

		public bool AutoConnect;

		public bool AutoJoin;

		public bool AutoPick;

		public bool AutoReady;

		public void EnableAllAuto()
		{
			AutoConnect = true;
			AutoJoin = true;
			AutoPick = true;
			AutoReady = true;
		}

		public void DisableAllAuto()
		{
			AutoConnect = false;
			AutoJoin = false;
			AutoPick = false;
			AutoReady = false;
		}
	}

	private enum UiState
	{
		Idle,
		EventSelection,
		BotGrid
	}

	public class SavedBotConfiguration
	{
		public string Email;

		public string Password;

		public bool AutoConnect;

		public bool AutoJoin;

		public bool AutoPick;

		public bool AutoReady;
	}

	private UiState _uiState;

	private IFrontDoorConnectionServiceWrapper _lastFd;

	private List<EventInfoV3> _eventList = new List<EventInfoV3>();

	private EventInfoV3 _selectedEvent;

	private List<DraftBotEntry> _draftBots = new List<DraftBotEntry>();

	private bool _enableAuto;

	private Promise<ActiveEventsResponseV2> _getEventsPromise;

	private Vector2 _eventScrollPosition = Vector2.zero;

	private string _newBotEmail = "draftbot#@test.wizards.com";

	private string _newBotPassword = "Password1!";

	private string _newBotCount = "7";

	private string _newBotIndex = "1";

	private DebugInfoIMGUIOnGui _gui;

	private string _saveFilePath => Path.Combine(PlatformContext.GetStorageContext().LocalPersistedStoragePath, "draft_bots.txt");

	public DebugInfoIMGUIOnGui.DebugTab TabType => DebugInfoIMGUIOnGui.DebugTab.DraftBot;

	public string TabName => "Draft Bot";

	public bool HiddenInTab => false;

	public void Init(DebugInfoIMGUIOnGui gui)
	{
		_gui = gui;
	}

	public void Destroy()
	{
	}

	public void OnQuit()
	{
		DestroyAllBots();
	}

	public void DestroyAllBots()
	{
		foreach (DraftBotEntry draftBot in _draftBots)
		{
			draftBot.Bot.Destroy();
		}
	}

	public bool OnUpdate()
	{
		IEventsServiceWrapper eventsServiceWrapper = Pantry.Get<IEventsServiceWrapper>();
		IFrontDoorConnectionServiceWrapper frontDoorConnectionServiceWrapper = Pantry.Get<IFrontDoorConnectionServiceWrapper>();
		if (_lastFd != frontDoorConnectionServiceWrapper || _lastFd == null || !_lastFd.Connected)
		{
			_uiState = UiState.Idle;
			_eventList.Clear();
			_lastFd = frontDoorConnectionServiceWrapper;
			_getEventsPromise = null;
			_draftBots.Clear();
			_selectedEvent = null;
			_enableAuto = false;
		}
		if (_uiState == UiState.Idle && _lastFd != null && _lastFd.Connected)
		{
			if (_getEventsPromise == null)
			{
				_getEventsPromise = eventsServiceWrapper.GetEvents();
			}
			if (_getEventsPromise.IsDone)
			{
				if (_getEventsPromise.Successful)
				{
					_eventList = _getEventsPromise.Result.Events.Where((EventInfoV3 e) => e.FormatType == EFormatType.Draft).ToList();
					_uiState = UiState.EventSelection;
				}
				else
				{
					Debug.Log("DraftBot failed to get events: " + _getEventsPromise.Error.Message);
				}
				_getEventsPromise = null;
			}
		}
		if (_uiState == UiState.BotGrid)
		{
			foreach (DraftBotEntry draftBot in _draftBots)
			{
				DraftBot bot = draftBot.Bot;
				bot.Update();
				DraftBot.StatusType statusType = bot.GetStatusType();
				if (!bot.IsBusy() && _enableAuto)
				{
					if (statusType == DraftBot.StatusType.NotConnected && draftBot.AutoConnect)
					{
						bot.ConnectToFrontdoor();
					}
					if (statusType == DraftBot.StatusType.NotInEvent && draftBot.AutoJoin)
					{
						PAPA.StartGlobalCoroutine(bot.GetIntoEvent());
					}
					if (statusType == DraftBot.StatusType.NotReady && draftBot.AutoReady)
					{
						PAPA.StartGlobalCoroutine(bot.BecomeReady());
					}
					if (statusType == DraftBot.StatusType.Picking && draftBot.AutoPick)
					{
						PAPA.StartGlobalCoroutine(bot.PickFirstInList());
					}
				}
			}
		}
		return true;
	}

	private bool DrawToggle(ref bool val, string text, int width)
	{
		bool flag = GUILayout.Toggle(val, text, GUILayout.Width(width));
		bool result = flag != val;
		val = flag;
		return result;
	}

	public void OnGUI()
	{
		if (_uiState == UiState.Idle)
		{
			GUILayout.Label("Connect to Front Door with your client");
		}
		if (_uiState == UiState.EventSelection)
		{
			_eventScrollPosition = _gui.BeginScrollView(_eventScrollPosition);
			foreach (EventInfoV3 @event in _eventList)
			{
				if (!_gui.ShowDebugButton(@event.InternalEventName, 500f))
				{
					continue;
				}
				_selectedEvent = @event;
				_uiState = UiState.BotGrid;
				_draftBots.Clear();
				foreach (SavedBotConfiguration item in LoadBotCredentials())
				{
					AddBot(item.Email, item.Password, item.AutoConnect, item.AutoJoin, item.AutoReady, item.AutoPick);
				}
			}
			GUILayout.EndScrollView();
		}
		if (_uiState != UiState.BotGrid)
		{
			return;
		}
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("<<", GUILayout.Width(100f)))
		{
			_uiState = UiState.EventSelection;
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Email:");
		_newBotEmail = GUILayout.TextField(_newBotEmail);
		GUILayout.Space(25f);
		GUILayout.Label("Password:");
		_newBotPassword = GUILayout.TextField(_newBotPassword);
		if (GUILayout.Button("Add Bot"))
		{
			AddBot(_newBotEmail, _newBotPassword, autoConnect: true, autoJoin: true, autoReady: true, autoPick: true);
			SaveBotCredentials();
		}
		GUILayout.Label("Number of Bots to Add:");
		_newBotCount = GUILayout.TextField(_newBotCount);
		GUILayout.Label("Starting Index:");
		_newBotIndex = GUILayout.TextField(_newBotIndex);
		if (GUILayout.Button("Add Bots") && int.TryParse(_newBotCount, out var result) && int.TryParse(_newBotIndex, out var result2))
		{
			for (int i = result2; i < result + result2; i++)
			{
				string email = _newBotEmail.Replace("#", i.ToString());
				AddBot(email, _newBotPassword, autoConnect: true, autoJoin: true, autoReady: true, autoPick: true);
			}
			SaveBotCredentials();
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal(GUILayout.Width(700f));
		_enableAuto = _gui.ShowToggle(_enableAuto, "Enable Automatic Behavior");
		if (GUILayout.Button("Remove All"))
		{
			_draftBots.Clear();
			SaveBotCredentials();
		}
		if (GUILayout.Button("Drop all"))
		{
			foreach (DraftBotEntry draftBot in _draftBots)
			{
				PAPA.StartGlobalCoroutine(draftBot.Bot.Drop());
			}
		}
		if (GUILayout.Button("Sort by seat"))
		{
			_draftBots.Sort((DraftBotEntry be1, DraftBotEntry be2) => be1.Bot.GetSeatIndex().CompareTo(be2.Bot.GetSeatIndex()));
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Space(25f);
		GUILayout.Label("Auto\nConn", GUILayout.Width(35f));
		GUILayout.Label("Auto\nJoin", GUILayout.Width(35f));
		GUILayout.Label("Auto\nRdy", GUILayout.Width(35f));
		GUILayout.Label("Auto\nPick", GUILayout.Width(35f));
		if (GUILayout.Button("All", GUILayout.Width(50f)))
		{
			foreach (DraftBotEntry draftBot2 in _draftBots)
			{
				draftBot2.EnableAllAuto();
			}
			SaveBotCredentials();
		}
		if (GUILayout.Button("None", GUILayout.Width(50f)))
		{
			foreach (DraftBotEntry draftBot3 in _draftBots)
			{
				draftBot3.DisableAllAuto();
			}
			SaveBotCredentials();
		}
		GUILayout.EndHorizontal();
		DraftBotEntry draftBotEntry = null;
		foreach (DraftBotEntry draftBot4 in _draftBots)
		{
			DraftBot bot = draftBot4.Bot;
			GUILayout.BeginHorizontal();
			GUI.color = Color.red;
			if (GUILayout.Button("X", GUILayout.Width(25f)))
			{
				draftBotEntry = draftBot4;
			}
			GUI.color = Color.white;
			if (DrawToggle(ref draftBot4.AutoConnect, "C", 35))
			{
				SaveBotCredentials();
			}
			if (DrawToggle(ref draftBot4.AutoJoin, "J", 35))
			{
				SaveBotCredentials();
			}
			if (DrawToggle(ref draftBot4.AutoReady, "R", 35))
			{
				SaveBotCredentials();
			}
			if (DrawToggle(ref draftBot4.AutoPick, "P", 35))
			{
				SaveBotCredentials();
			}
			if (GUILayout.Button("All", GUILayout.Width(50f)))
			{
				draftBot4.EnableAllAuto();
				SaveBotCredentials();
			}
			if (GUILayout.Button("None", GUILayout.Width(50f)))
			{
				draftBot4.DisableAllAuto();
				SaveBotCredentials();
			}
			if (GUILayout.Button("D", GUILayout.Width(25f)))
			{
				PAPA.StartGlobalCoroutine(bot.Drop());
			}
			GUILayout.Label($"{bot.GetSeatIndex()} - {bot.GetEmail()}");
			DraftBot.StatusType statusType = bot.GetStatusType();
			GUILayout.Label(bot.GetStatusString() ?? "");
			if (bot.IsBusy())
			{
				GUILayout.Label("Waiting for reply...");
			}
			else
			{
				switch (statusType)
				{
				case DraftBot.StatusType.NotConnected:
					if (GUILayout.Button("Connect"))
					{
						bot.ConnectToFrontdoor();
					}
					break;
				case DraftBot.StatusType.NotInEvent:
					if (GUILayout.Button("Join Event"))
					{
						PAPA.StartGlobalCoroutine(bot.GetIntoEvent());
					}
					break;
				case DraftBot.StatusType.WaitingForPod:
					if (GUILayout.Button("Cancel Pod"))
					{
						PAPA.StartGlobalCoroutine(bot.CancelPod());
					}
					break;
				case DraftBot.StatusType.NotReady:
					if (GUILayout.Button("I'm Ready!"))
					{
						PAPA.StartGlobalCoroutine(bot.BecomeReady());
					}
					break;
				case DraftBot.StatusType.Picking:
					if (GUILayout.Button("Pick First Card"))
					{
						PAPA.StartGlobalCoroutine(bot.PickFirstInList());
					}
					break;
				case DraftBot.StatusType.CompleteDraft:
					if (GUILayout.Button("Complete Draft"))
					{
						PAPA.StartGlobalCoroutine(bot.CompleteDraft());
					}
					break;
				case DraftBot.StatusType.ConfirmCardPool:
					if (GUILayout.Button("Confirm Card Pool"))
					{
						PAPA.StartGlobalCoroutine(bot.ConfirmCardPool());
					}
					break;
				case DraftBot.StatusType.Done:
					if (GUILayout.Button("Drop from Event"))
					{
						PAPA.StartGlobalCoroutine(bot.Drop());
					}
					break;
				}
			}
			GUILayout.EndHorizontal();
		}
		if (draftBotEntry != null)
		{
			_draftBots.Remove(draftBotEntry);
			SaveBotCredentials();
		}
	}

	private void AddBot(string email, string password, bool autoConnect, bool autoJoin, bool autoReady, bool autoPick)
	{
		DraftBot bot = new DraftBot(email, password, Pantry.CurrentEnvironment, _selectedEvent, null);
		DraftBotEntry draftBotEntry = new DraftBotEntry();
		draftBotEntry.Bot = bot;
		draftBotEntry.AutoConnect = autoConnect;
		draftBotEntry.AutoJoin = autoJoin;
		draftBotEntry.AutoReady = autoReady;
		draftBotEntry.AutoPick = autoPick;
		_draftBots.Add(draftBotEntry);
	}

	private List<SavedBotConfiguration> LoadBotCredentials()
	{
		if (File.Exists(_saveFilePath))
		{
			return JsonConvert.DeserializeObject<List<SavedBotConfiguration>>(File.ReadAllText(_saveFilePath));
		}
		return new List<SavedBotConfiguration>();
	}

	private void SaveBotCredentials()
	{
		string contents = JsonConvert.SerializeObject(_draftBots.Select((DraftBotEntry x) => new SavedBotConfiguration
		{
			Email = x.Bot.GetEmail(),
			Password = x.Bot.GetPassword(),
			AutoConnect = x.AutoConnect,
			AutoJoin = x.AutoJoin,
			AutoReady = x.AutoReady,
			AutoPick = x.AutoPick
		}).ToList());
		File.WriteAllText(_saveFilePath, contents);
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
}
