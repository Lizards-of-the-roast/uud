using System;
using System.Collections.Generic;
using Core.Code.ClientFeatureToggle;
using Core.Code.Harnesses.OfflineHarnessServices;
using Core.Code.PlayerInbox;
using Core.Code.Promises;
using Core.Shared.Code.Network;
using Core.Shared.Code.WrapperFactories;
using Newtonsoft.Json;
using UnityEngine;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Unification.Models.Player;
using Wotc.Mtga.Wrapper.Mailbox;

public class PlayerInboxPageGUI : IDebugGUIPage
{
	private DebugInfoIMGUIOnGui _GUI;

	private static GUIStyle _textInputStyleCache;

	private static GUIStyle _guiStyleWordWrapWhite;

	private string _letterId = "";

	private string _letterString = "";

	private string _mockLetterToAdd = "";

	private bool _mockEnabledAtInit;

	private bool _addMockDataToLetters = true;

	private bool _allowMockPlayerInboxService;

	private bool _checkedForMockPlayerInboxServiceAllowed;

	private bool _playerInboxFeatureToggle;

	public DebugInfoIMGUIOnGui.DebugTab TabType => DebugInfoIMGUIOnGui.DebugTab.PlayerInbox;

	public string TabName => "Player Inbox";

	public bool HiddenInTab => false;

	private static GUIStyle _textInputStyle
	{
		get
		{
			if (_textInputStyleCache == null)
			{
				_textInputStyleCache = new GUIStyle(GUI.skin.GetStyle("TextField"));
			}
			return _textInputStyleCache;
		}
	}

	public void Init(DebugInfoIMGUIOnGui gui)
	{
		_GUI = gui;
		_mockEnabledAtInit = MDNPlayerPrefs.DEBUG_MockPlayerInboxService;
	}

	public void Destroy()
	{
	}

	public void OnQuit()
	{
	}

	public bool OnUpdate()
	{
		ClientFeatureToggleDataProvider clientFeatureToggleDataProvider = Pantry.Get<ClientFeatureToggleDataProvider>();
		if (clientFeatureToggleDataProvider != null)
		{
			_playerInboxFeatureToggle = clientFeatureToggleDataProvider.GetToggleValueById("ClientPlayerInbox");
		}
		if (!_checkedForMockPlayerInboxServiceAllowed)
		{
			_allowMockPlayerInboxService = PlayerInboxServiceWrapperFactory.AllowPlayerInboxMockService();
			_checkedForMockPlayerInboxServiceAllowed = true;
		}
		return true;
	}

	public void OnGUI()
	{
		if (!_playerInboxFeatureToggle)
		{
			GUILayout.Label("Client feature toggle for Player Inbox is disabled");
			return;
		}
		GUILayout.Label("----------- Player Inbox Service Mock -----------");
		if (!_allowMockPlayerInboxService && MDNPlayerPrefs.DEBUG_MockPlayerInboxService)
		{
			MDNPlayerPrefs.DEBUG_MockPlayerInboxService = false;
		}
		bool dEBUG_MockPlayerInboxService = MDNPlayerPrefs.DEBUG_MockPlayerInboxService;
		dEBUG_MockPlayerInboxService = _GUI.ShowToggle(dEBUG_MockPlayerInboxService, "Mock Player Inbox Service (Client restart required)");
		if (MDNPlayerPrefs.DEBUG_MockPlayerInboxService != dEBUG_MockPlayerInboxService)
		{
			if (_allowMockPlayerInboxService)
			{
				MDNPlayerPrefs.DEBUG_MockPlayerInboxService = dEBUG_MockPlayerInboxService;
			}
			else
			{
				Debug.LogError("To enable player inbox mock service, run in editor or use an account with debug role access");
			}
		}
		if (MDNPlayerPrefs.DEBUG_MockPlayerInboxService && _mockEnabledAtInit)
		{
			HarnessPlayerInboxServiceWrapper harnessPlayerInboxServiceWrapper = Pantry.Get<IPlayerInboxServiceWrapper>() as HarnessPlayerInboxServiceWrapper;
			GUILayout.BeginHorizontal();
			if (_GUI.ShowDebugButton("Reset Inbox Mock Letters", 200f))
			{
				harnessPlayerInboxServiceWrapper.ResetInbox();
			}
			if (_GUI.ShowDebugButton("Clear Inbox Letters", 200f))
			{
				harnessPlayerInboxServiceWrapper.ClearInbox();
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if (_GUI.ShowDebugButton("Add Mock Letter", 200f))
			{
				harnessPlayerInboxServiceWrapper.AddLetterFromJson(_mockLetterToAdd, _addMockDataToLetters);
			}
			_addMockDataToLetters = _GUI.ShowToggle(_addMockDataToLetters, "As Mock Letter", GUILayout.Width(110f));
			_mockLetterToAdd = GUILayout.TextField(_mockLetterToAdd, _textInputStyle);
			GUILayout.EndHorizontal();
		}
		GUILayout.Label("----------- Player Inbox Service Requests -----------");
		GUILayout.BeginHorizontal();
		if (_GUI.ShowDebugButton("Get Letters", 200f))
		{
			Pantry.Get<PlayerInboxDataProvider>().GetPlayerInbox().IfSuccess(delegate(Promise<List<Client_Letter>> promise)
			{
				UpdateLetterString(promise.Result);
			});
		}
		if (_GUI.ShowDebugButton("Get Letters(Force Service Req)", 200f))
		{
			Pantry.Get<PlayerInboxDataProvider>().GetPlayerInbox(forceServiceRequest: true).IfSuccess(delegate(Promise<List<Client_Letter>> promise)
			{
				UpdateLetterString(promise.Result);
			});
		}
		if (_GUI.ShowDebugButton("Get Letters(Error)", 200f))
		{
			Pantry.Get<PlayerInboxDataProvider>().BroadcastLetterDataChange(PlayerInboxDataProvider.LetterDataChange.Error, null);
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if (_GUI.ShowDebugButton("Mark Letter Read", 200f))
		{
			Pantry.Get<PlayerInboxDataProvider>().MarkLetterRead(string.IsNullOrEmpty(_letterId) ? Guid.Empty : new Guid(_letterId));
		}
		if (_GUI.ShowDebugButton("Claim Letter Attachment", 200f))
		{
			Pantry.Get<PlayerInboxDataProvider>().ClaimLetterAttachment(string.IsNullOrEmpty(_letterId) ? Guid.Empty : new Guid(_letterId)).ThenOnMainThread(delegate(Promise<Client_LetterAttachmentClaimed> promise)
			{
				if (promise.Successful)
				{
					DisplayClaimedRewards(promise.Result.inventoryInfo.Changes);
				}
				else
				{
					ContentControllerPlayerInbox.DisplayClaimedRewardError(promise.Error.Message);
				}
			});
		}
		_letterId = GUILayout.TextField(_letterId, _textInputStyle);
		GUILayout.EndHorizontal();
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
		GUILayout.TextArea(_letterString, _guiStyleWordWrapWhite);
	}

	private void UpdateLetterString(List<Client_Letter> letters)
	{
		_letterString = JsonConvert.SerializeObject(letters, Formatting.Indented);
	}

	private void DisplayClaimedRewards(List<InventoryChange> changes)
	{
		SceneLoader.GetSceneLoader().GetPlayerInboxContentController().DisplayClaimedRewards(changes);
	}
}
