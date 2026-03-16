using System;
using Google.Protobuf.Collections;
using UnityEngine;
using Wizards.Arena.Enums.Match;
using Wizards.Arena.Models.Network;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Shared.Code.DebugTools;

public class ChallengeServiceGUI : IDebugGUIPage
{
	private DebugInfoIMGUIOnGui _GUI;

	private static GUIStyle _textInputStyleCache;

	private const int SpaceBetweenPromiseAreas = 15;

	private static GUIStyle _guiStyleWordWrapWhite;

	private Vector2 _currentScrollPos = Vector2.zero;

	private IChallengeServiceWrapper _challengeServiceWrapper;

	private IAccountClient _accountClient;

	private string _createChallengeName = string.Empty;

	private string _createChallengePass = string.Empty;

	private string _joinChallengeId = string.Empty;

	private string _joinChallengeName = string.Empty;

	private string _joinChallengePass = string.Empty;

	private string _inviteChallengeId = string.Empty;

	private string _inviteChallengePlayerId = string.Empty;

	private string _exitChallengeId = string.Empty;

	private string _messageChallengeId = string.Empty;

	private string _messageChallengeString = string.Empty;

	private string _closeChallengeId = string.Empty;

	private string _kickChallengeId = string.Empty;

	private string _kickPlayerId = string.Empty;

	private string _readyChallengeId = string.Empty;

	private string _readyAvatar = string.Empty;

	private string _readyPet = string.Empty;

	private string _readyPlayerId = string.Empty;

	private string _readySleeve = string.Empty;

	private string _readyTitle = string.Empty;

	private string _unreadyChallengeId = string.Empty;

	private string _unreadyPlayerId = string.Empty;

	private string _setSettingChallengeId = string.Empty;

	private string _setSettingChallengeName = string.Empty;

	private PlayFirst _setSettingPlayFirst = PlayFirst.Opponent;

	private int _setSettingPlayFirstInternal;

	private MatchWinCondition _setSettingWinCondition = MatchWinCondition.SingleElimination;

	private int _setSettingWinConditionInternal;

	private Vector2 _currentPlayFirstScroll = Vector2.zero;

	private Vector2 _currentWinConditionScroll = Vector2.zero;

	private string _issueChallengeId = string.Empty;

	private string _issuePlayerId = string.Empty;

	private string _issueChallengeName = string.Empty;

	private string _issueDeckId = string.Empty;

	public DebugInfoIMGUIOnGui.DebugTab TabType => DebugInfoIMGUIOnGui.DebugTab.ChallengeService;

	public string TabName => "ChallengeService";

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
		_challengeServiceWrapper = Pantry.Get<IChallengeServiceWrapper>();
		_accountClient = Pantry.Get<IAccountClient>();
		_challengeServiceWrapper.OnChallengeNotification += HandleChallengeNotification;
		ResetAllFields();
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

	private void HandleChallengeNotification(ChallengeNotification notify)
	{
	}

	private void ResetAllFields()
	{
		_createChallengeName = string.Empty;
		_createChallengePass = string.Empty;
		_joinChallengeId = string.Empty;
		_joinChallengeName = string.Empty;
		_joinChallengePass = string.Empty;
		_inviteChallengeId = string.Empty;
		_inviteChallengePlayerId = string.Empty;
		_exitChallengeId = string.Empty;
		_messageChallengeId = string.Empty;
		_messageChallengeString = string.Empty;
		_closeChallengeId = string.Empty;
		_kickChallengeId = string.Empty;
		_kickPlayerId = string.Empty;
		_readyChallengeId = string.Empty;
		_readyAvatar = string.Empty;
		_readyPet = string.Empty;
		_readyPlayerId = GetPlayerIdString();
		_readySleeve = string.Empty;
		_readyTitle = string.Empty;
		_unreadyChallengeId = string.Empty;
		_unreadyPlayerId = GetPlayerIdString();
	}

	private string GetPlayerIdString()
	{
		return _accountClient?.AccountInformation?.AccountID ?? string.Empty;
	}

	public void OnGUI()
	{
		_currentScrollPos = GUILayout.BeginScrollView(_currentScrollPos, false, true);
		GUILayout.Space(15f);
		CreateChallengeGUI();
		GUILayout.Space(15f);
		JoinChallengeGUI();
		GUILayout.Space(15f);
		InviteChallengeGUI();
		GUILayout.Space(15f);
		ExitChallengeGUI();
		GUILayout.Space(15f);
		MessageChallengeGUI();
		GUILayout.Space(15f);
		CloseChallengeGUI();
		GUILayout.Space(15f);
		KickChallengeGUI();
		GUILayout.Space(15f);
		ReadyChallengeGUI();
		GUILayout.Space(15f);
		SetSettingsChallengeGUI();
		GUILayout.Space(15f);
		IssueChallengeGUI();
		GUILayout.Space(45f);
		GUILayout.EndScrollView();
	}

	private void CreateChallengeGUI(float maxWidth = 500f)
	{
		using (new GUILayout.HorizontalScope("box", GUILayout.MaxWidth(maxWidth)))
		{
			GUILayout.BeginVertical();
			GUILayout.Label("Create Challenge:");
			GUILayout.BeginHorizontal();
			GUILayout.Label("ChallengeName:");
			_createChallengeName = GUILayout.TextField(_createChallengeName, _textInputStyle);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("ChallengePassword:");
			_createChallengePass = GUILayout.TextField(_createChallengePass, _textInputStyle);
			GUILayout.EndHorizontal();
			if (GUILayout.Button("Create"))
			{
				_challengeServiceWrapper.ChallengeCreate(_createChallengeName, _createChallengePass).Then(delegate
				{
				});
			}
			GUILayout.EndVertical();
		}
	}

	private void JoinChallengeGUI(float maxWidth = 500f)
	{
		using (new GUILayout.HorizontalScope("box", GUILayout.MaxWidth(maxWidth)))
		{
			GUILayout.BeginVertical();
			GUILayout.Label("Join Challenge:");
			GUILayout.BeginHorizontal();
			GUILayout.Label("ChallengeId:");
			_joinChallengeId = GUILayout.TextField(_joinChallengeId, _textInputStyle);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("ChallengeName:");
			_joinChallengeName = GUILayout.TextField(_joinChallengeName, _textInputStyle);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("ChallengePassword:");
			_joinChallengePass = GUILayout.TextField(_joinChallengePass, _textInputStyle);
			GUILayout.EndHorizontal();
			if (GUILayout.Button("Join"))
			{
				_challengeServiceWrapper.ChallengeJoin(_joinChallengeId).Then(delegate
				{
				});
			}
			GUILayout.EndVertical();
		}
	}

	private void InviteChallengeGUI(float maxWidth = 500f)
	{
		using (new GUILayout.HorizontalScope("box", GUILayout.MaxWidth(maxWidth)))
		{
			GUILayout.BeginVertical();
			GUILayout.Label("Invite Challenge:");
			GUILayout.BeginHorizontal();
			GUILayout.Label("ChallengeId:");
			_inviteChallengeId = GUILayout.TextField(_inviteChallengeId, _textInputStyle);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("PlayerId:");
			_inviteChallengePlayerId = GUILayout.TextField(_inviteChallengePlayerId, _textInputStyle);
			GUILayout.EndHorizontal();
			if (GUILayout.Button("Invite"))
			{
				_challengeServiceWrapper.ChallengeInvite(_inviteChallengeId, _inviteChallengePlayerId).Then(delegate
				{
				});
			}
			GUILayout.EndVertical();
		}
	}

	private void ExitChallengeGUI(float maxWidth = 500f)
	{
		using (new GUILayout.HorizontalScope("box", GUILayout.MaxWidth(maxWidth)))
		{
			GUILayout.BeginVertical();
			GUILayout.Label("Exit Challenge:");
			GUILayout.BeginHorizontal();
			GUILayout.Label("ChallengeId:");
			_exitChallengeId = GUILayout.TextField(_exitChallengeId, _textInputStyle);
			GUILayout.EndHorizontal();
			if (GUILayout.Button("Exit"))
			{
				_challengeServiceWrapper.ChallengeExit(_exitChallengeId).Then(delegate
				{
				});
			}
			GUILayout.EndVertical();
		}
	}

	private void MessageChallengeGUI(float maxWidth = 500f)
	{
		using (new GUILayout.HorizontalScope("box", GUILayout.MaxWidth(500f)))
		{
			GUILayout.BeginVertical();
			GUILayout.Label("Message Challenge:");
			GUILayout.BeginHorizontal();
			GUILayout.Label("ChallengeId:");
			_messageChallengeId = GUILayout.TextField(_messageChallengeId, _textInputStyle);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Message:");
			_messageChallengeString = GUILayout.TextField(_messageChallengeString, _textInputStyle);
			GUILayout.EndHorizontal();
			if (GUILayout.Button("Send"))
			{
				_challengeServiceWrapper.ChallengeSendMessage(_messageChallengeId, _messageChallengeString).Then(delegate
				{
				});
			}
			GUILayout.EndVertical();
		}
	}

	private void CloseChallengeGUI(float maxWidth = 500f)
	{
		using (new GUILayout.HorizontalScope("box", GUILayout.MaxWidth(maxWidth)))
		{
			GUILayout.BeginVertical();
			GUILayout.Label("Close Challenge:");
			GUILayout.BeginHorizontal();
			GUILayout.Label("ChallengeId:");
			_closeChallengeId = GUILayout.TextField(_closeChallengeId, _textInputStyle);
			GUILayout.EndHorizontal();
			if (GUILayout.Button("Send"))
			{
				_challengeServiceWrapper.ChallengeClose(_closeChallengeId).Then(delegate
				{
				});
			}
			GUILayout.EndVertical();
		}
	}

	private void KickChallengeGUI(float maxWidth = 500f)
	{
		using (new GUILayout.HorizontalScope("box", GUILayout.MaxWidth(maxWidth)))
		{
			GUILayout.BeginVertical();
			GUILayout.Label("Kick Challenge:");
			GUILayout.BeginHorizontal();
			GUILayout.Label("ChallengeId:");
			_kickChallengeId = GUILayout.TextField(_kickChallengeId, _textInputStyle);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("PlayerId:");
			_kickPlayerId = GUILayout.TextField(_kickPlayerId, _textInputStyle);
			GUILayout.EndHorizontal();
			if (GUILayout.Button("Send"))
			{
				_challengeServiceWrapper.ChallengeKick(_kickChallengeId, _kickPlayerId).Then(delegate
				{
				});
			}
			GUILayout.EndVertical();
		}
	}

	private void ReadyChallengeGUI(float maxWidth = 500f)
	{
		using (new GUILayout.HorizontalScope("box", GUILayout.MaxWidth(maxWidth)))
		{
			GUILayout.BeginVertical();
			GUILayout.Label("Ready Challenge:");
			GUILayout.BeginHorizontal();
			GUILayout.Label("PlayerId:");
			_readyChallengeId = GUILayout.TextField(_readyChallengeId, _textInputStyle);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("ChallengeId:");
			_readyPlayerId = GUILayout.TextField(_readyPlayerId, _textInputStyle);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Avatar:");
			_readyAvatar = GUILayout.TextField(_readyAvatar, _textInputStyle);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Pet:");
			_readyPet = GUILayout.TextField(_readyPet, _textInputStyle);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Sleeve:");
			_readySleeve = GUILayout.TextField(_readySleeve, _textInputStyle);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Title:");
			_readyTitle = GUILayout.TextField(_readyTitle, _textInputStyle);
			GUILayout.EndHorizontal();
			if (_GUI.ShowDebugButton("Send", 500f))
			{
				new RepeatedField<string>();
				_challengeServiceWrapper.ChallengeReady(_readyChallengeId, Guid.NewGuid()).Then(delegate
				{
				});
			}
			GUILayout.EndVertical();
		}
		GUILayout.Space(15f);
		using (new GUILayout.HorizontalScope("box", GUILayout.MaxWidth(500f)))
		{
			GUILayout.BeginVertical();
			GUILayout.Label("Unready Challenge:");
			_unreadyChallengeId = _GUI.ShowInputField("ChallengeId", _unreadyChallengeId);
			_unreadyPlayerId = _GUI.ShowInputField("PlayerId", _unreadyPlayerId);
			if (_GUI.ShowDebugButton("Send", 500f))
			{
				_challengeServiceWrapper.ChallengeUnready(_unreadyChallengeId).Then(delegate
				{
				});
			}
			GUILayout.EndVertical();
		}
	}

	private void SetSettingsChallengeGUI(float maxWidth = 500f)
	{
		using (new GUILayout.HorizontalScope("box", GUILayout.MaxWidth(maxWidth)))
		{
			GUILayout.BeginVertical();
			GUILayout.Label("Ready Challenge:");
			GUILayout.BeginHorizontal();
			GUILayout.Label("ChallengeId:");
			_setSettingChallengeId = GUILayout.TextField(_setSettingChallengeId, _textInputStyle);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("ChallengeName:");
			_setSettingChallengeName = GUILayout.TextField(_setSettingChallengeName, _textInputStyle);
			GUILayout.EndHorizontal();
			PlayFirst setSettingPlayFirst = _setSettingPlayFirst;
			_setSettingPlayFirst = _GUI.SelectEnumUtility("PlayFirst:", _setSettingPlayFirst, ref _currentPlayFirstScroll);
			if (_setSettingPlayFirst != setSettingPlayFirst)
			{
				_setSettingPlayFirstInternal = (int)_setSettingPlayFirst;
			}
			MatchWinCondition setSettingWinCondition = _setSettingWinCondition;
			_setSettingWinCondition = _GUI.SelectEnumUtility("WinCondition:", _setSettingWinCondition, ref _currentWinConditionScroll);
			if (_setSettingWinCondition != setSettingWinCondition)
			{
				_setSettingWinConditionInternal = (int)_setSettingWinCondition;
			}
			if (_GUI.ShowDebugButton("Send", 500f))
			{
				_challengeServiceWrapper.ChallengeSetSettings(_setSettingChallengeId, _setSettingChallengeName, _setSettingPlayFirst, _setSettingWinCondition).Then(delegate
				{
				});
			}
			GUILayout.EndVertical();
		}
	}

	private void IssueChallengeGUI(float maxWidth = 500f)
	{
		using (new GUILayout.HorizontalScope("box", GUILayout.MaxWidth(maxWidth)))
		{
			GUILayout.BeginVertical();
			GUILayout.Label("Ready Challenge:");
			GUILayout.BeginHorizontal();
			GUILayout.Label("ChallengeId:");
			_issueChallengeId = GUILayout.TextField(_issueChallengeId, _textInputStyle);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("PlayerId:");
			_issuePlayerId = GUILayout.TextField(_issuePlayerId, _textInputStyle);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("ChallengeName:");
			_issueChallengeName = GUILayout.TextField(_issueChallengeName, _textInputStyle);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("DeckId:");
			_issueDeckId = GUILayout.TextField(_issueDeckId, _textInputStyle);
			GUILayout.EndHorizontal();
			if (_GUI.ShowDebugButton("Send", 500f))
			{
				_challengeServiceWrapper.ChallengeIssue(_issueChallengeId, _issueChallengeName, _issueDeckId).Then(delegate
				{
				});
			}
			GUILayout.EndVertical();
		}
	}
}
