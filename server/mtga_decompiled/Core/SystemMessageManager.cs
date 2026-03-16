using System;
using System.Collections.Generic;
using System.Linq;
using Core.Code.Input;
using Core.Meta.MainNavigation.SystemMessage;
using MTGA.KeyboardManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wizards.Mtga.Platforms;
using Wizards.Unification.Models.Player;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;

public class SystemMessageManager : ISystemMessageManager
{
	public enum SystemMessagePriority
	{
		FatalError,
		Other
	}

	public class SystemMessageButtonData
	{
		public string Text = "Button";

		public Action Callback;

		public bool IsConfirm;

		public bool HideOnClick = true;

		public bool IsCancel;

		public bool IsDisabled;

		public bool IsExternalLink;

		public string AlertText = "";
	}

	public class SystemMessageHandle
	{
		public List<SystemMessageButtonData> Buttons;

		public string Title;

		public string Message;

		public string Details;

		public string LogOverride;

		public SystemMessagePriority Priority;

		public readonly DateTime TimeStamp;

		public SystemMessageHandle(List<SystemMessageButtonData> buttons, string title, string message, string details, SystemMessagePriority priority, string logOverride)
		{
			Buttons = buttons;
			Title = title;
			Message = message;
			Details = details;
			Priority = priority;
			TimeStamp = DateTime.Now;
			LogOverride = logOverride;
		}
	}

	private SystemMessageHandle _currentMessage;

	private List<SystemMessageHandle> _messages = new List<SystemMessageHandle>();

	private readonly KeyboardManager _keyboardManager;

	private readonly IActionSystem _actionSystem;

	private readonly IBILogger _biLogger;

	private SystemMessageView _view;

	private GameObject _root;

	private IFrontDoorConnectionServiceWrapper _fdc;

	private static SystemMessageManager _instance;

	public bool ShowingMessage => _messages.Count > 0;

	public static SystemMessageManager Instance => _instance;

	public static SystemMessageManager Initialize(KeyboardManager keyboardManager, IActionSystem actionSystem, IBILogger biLogger)
	{
		if (_instance == null)
		{
			_instance = new SystemMessageManager(keyboardManager, actionSystem, biLogger);
		}
		return _instance;
	}

	private void OnSystemMessage(SystemMessage[] systemMessages)
	{
		bool result = default(bool);
		foreach (SystemMessage systemMessage in systemMessages)
		{
			string message = (systemMessage.MessageLocalized ? Languages.ActiveLocProvider.GetLocalizedText(systemMessage.Message) : systemMessage.Message);
			string title = (string.IsNullOrWhiteSpace(systemMessage.Title) ? Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_System_Message_Title") : (systemMessage.TitleLocalized ? Languages.ActiveLocProvider.GetLocalizedText(systemMessage.Title) : systemMessage.Title));
			if (systemMessage.Parameters != null && systemMessage.Parameters.TryGetValue("ShouldCloseGame", out var value) && bool.TryParse(value, out result) && result)
			{
				ShowSystemMessage(title, message, showCancel: false, delegate
				{
					SceneLoader.ApplicationQuit();
				});
			}
			else
			{
				ShowSystemMessage(title, message);
			}
		}
	}

	private SystemMessageManager(KeyboardManager keyboardManager, IActionSystem actionSystem, IBILogger biLogger)
	{
		_keyboardManager = keyboardManager;
		_actionSystem = actionSystem;
		_biLogger = biLogger;
	}

	public void SetFDConnectionWrapper(IFrontDoorConnectionServiceWrapper fdc)
	{
		if (_fdc != null)
		{
			_fdc.UnRegisterSystemMessageEvent(OnSystemMessage);
		}
		_fdc = fdc;
		_fdc.RegisterSystemMessageEvent(OnSystemMessage);
	}

	public static void ShowSystemMessage(string title, string message, bool showCancel = false, Action onOK = null, Action onCancel = null, int fontSize = -1, string logOverride = null)
	{
		if (showCancel)
		{
			Instance.ShowOkCancel(title, message, onOK, onCancel, null, SystemMessagePriority.Other, logOverride);
		}
		else
		{
			Instance.ShowOk(title, message, onOK, null, SystemMessagePriority.Other, logOverride);
		}
	}

	public void Shutdown()
	{
		if (_fdc != null)
		{
			_fdc.UnRegisterSystemMessageEvent(OnSystemMessage);
		}
		_instance = null;
		_messages.Clear();
		_currentMessage = null;
		if (_view != null)
		{
			UnityEngine.Object.Destroy(_root);
			_root = null;
			_view = null;
		}
	}

	private void DismissCurrentMessage()
	{
		if (_currentMessage != null)
		{
			Close(_currentMessage);
		}
	}

	public SystemMessageHandle ShowMessage(string title, string text, List<SystemMessageButtonData> buttons, string details = null, SystemMessagePriority priority = SystemMessagePriority.Other, string logOverride = null)
	{
		SystemMessageHandle systemMessageHandle = new SystemMessageHandle(buttons, title, text, details, priority, logOverride);
		_messages.Add(systemMessageHandle);
		_messages = _messages.OrderBy((SystemMessageHandle x) => x.Priority).ToList();
		if (_messages[0] == systemMessageHandle)
		{
			PresentView(systemMessageHandle);
		}
		return systemMessageHandle;
	}

	public void ClearMessageQueue()
	{
		_messages.Clear();
	}

	public void Close(SystemMessageHandle msg)
	{
		string currentSceneName = ((!(SceneLoader.GetSceneLoader() != null)) ? SceneManager.GetActiveScene().name : SceneLoader.GetSceneLoader().GetCurrentSceneName());
		BI_MessageDialogueViewed(currentSceneName, msg.Title, msg.LogOverride ?? msg.Message, msg.TimeStamp);
		bool num = msg == _messages[0];
		_messages.Remove(msg);
		if (!num)
		{
			return;
		}
		if (_messages.Count > 0)
		{
			PresentView(_messages[0]);
			return;
		}
		_currentMessage = null;
		if (_view != null && _view.IsOpen)
		{
			_view.Hide();
		}
	}

	private void PresentView(SystemMessageHandle message)
	{
		_currentMessage = message;
		if (_view == null)
		{
			SpawnSystemMessageView();
			_view = _root.GetComponentInChildren<SystemMessageView>();
			_view.Initialize(_keyboardManager, _actionSystem, DismissCurrentMessage);
		}
		_view.CreateButtons(_currentMessage.Buttons);
		_view.SetTitle(_currentMessage.Title);
		_view.SetMessage(_currentMessage.Message);
		_view.SetDetails(_currentMessage.Details);
		_view.Show();
	}

	private void SpawnSystemMessageView()
	{
		string path = "SystemMessageView_Desktop_16x9";
		if (PlatformUtils.GetCurrentDeviceType() == DeviceType.Handheld)
		{
			path = ((!((double)PlatformUtils.GetCurrentAspectRatio() < 1.5)) ? "SystemMessageView_Handheld_16x9" : "SystemMessageView_Handheld_4x3");
		}
		GameObject original = Resources.Load<GameObject>(path);
		_root = UnityEngine.Object.Instantiate(original);
		UnityEngine.Object.DontDestroyOnLoad(_root);
	}

	private void BI_MessageDialogueViewed(string currentSceneName, string title, string message, DateTime timeStamp)
	{
		DateTime now = DateTime.Now;
		TimeSpan duration = now - timeStamp;
		MessageDialogueViewed payload = new MessageDialogueViewed
		{
			EventTime = now,
			CurrentSceneName = currentSceneName,
			Title = title,
			Message = message,
			Duration = duration
		};
		_biLogger.Send(ClientBusinessEventType.MessageDialogueViewed, payload);
	}

	public SystemMessageHandle ShowOk(string title, string text, Action onOk = null, string details = null, SystemMessagePriority priority = SystemMessagePriority.Other, string logOverride = null)
	{
		return ShowMessage(title, text, Languages.ActiveLocProvider.GetLocalizedText("MainNav/Popups/Modal/ModalOptions_OK"), onOk, details, priority);
	}

	public SystemMessageHandle ShowOkCancel(string title, string text, Action onOk, Action onCancel, string details = null, SystemMessagePriority priority = SystemMessagePriority.Other, string logOverride = null)
	{
		return ShowMessage(title, text, Languages.ActiveLocProvider.GetLocalizedText("MainNav/Popups/Modal/ModalOptions_Cancel"), onCancel, Languages.ActiveLocProvider.GetLocalizedText("MainNav/Popups/Modal/ModalOptions_OK"), onOk, details, priority);
	}

	public SystemMessageHandle ShowMessage(string title, string text, string button1Text, Action button1Action, string details = null, SystemMessagePriority priority = SystemMessagePriority.Other, string logOverride = null)
	{
		List<SystemMessageButtonData> buttons = new List<SystemMessageButtonData>
		{
			new SystemMessageButtonData
			{
				Text = button1Text,
				Callback = button1Action,
				IsConfirm = true
			}
		};
		return ShowMessage(title, text, buttons, details, priority);
	}

	public SystemMessageHandle ShowMessage(string title, string text, string button1Text, Action button1Action, string button2Text, Action button2Action, string details = null, SystemMessagePriority priority = SystemMessagePriority.Other, string logOverride = null)
	{
		List<SystemMessageButtonData> buttons = new List<SystemMessageButtonData>
		{
			new SystemMessageButtonData
			{
				Text = button1Text,
				Callback = button1Action
			},
			new SystemMessageButtonData
			{
				Text = button2Text,
				Callback = button2Action,
				IsConfirm = true
			}
		};
		return ShowMessage(title, text, buttons, details, priority);
	}

	public SystemMessageHandle ShowMessage(string title, string text, string button1Text, Action button1Action, string button2Text, Action button2Action, string button3Text, Action button3Action, string details = null, SystemMessagePriority priority = SystemMessagePriority.Other)
	{
		List<SystemMessageButtonData> buttons = new List<SystemMessageButtonData>
		{
			new SystemMessageButtonData
			{
				Text = button1Text,
				Callback = button1Action
			},
			new SystemMessageButtonData
			{
				Text = button2Text,
				Callback = button2Action
			},
			new SystemMessageButtonData
			{
				Text = button3Text,
				Callback = button3Action,
				IsConfirm = true
			}
		};
		return ShowMessage(title, text, buttons, details, priority);
	}

	public SystemMessageHandle ShowMessage(string title, string text, string button1Text, string button1AlertText, bool button1Disabled, Action button1Action, string button2Text, string button2AlertText, bool button2Disabled, Action button2Action, string button3Text, string button3AlertText, bool button3Disabled, Action button3Action, string details = null, SystemMessagePriority priority = SystemMessagePriority.Other)
	{
		List<SystemMessageButtonData> buttons = new List<SystemMessageButtonData>
		{
			new SystemMessageButtonData
			{
				Text = button1Text,
				Callback = button1Action,
				AlertText = button1AlertText,
				IsDisabled = button1Disabled
			},
			new SystemMessageButtonData
			{
				Text = button2Text,
				Callback = button2Action,
				AlertText = button2AlertText,
				IsDisabled = button2Disabled
			},
			new SystemMessageButtonData
			{
				Text = button3Text,
				Callback = button3Action,
				IsConfirm = true,
				AlertText = button3AlertText,
				IsDisabled = button3Disabled
			}
		};
		return ShowMessage(title, text, buttons, details, priority);
	}
}
