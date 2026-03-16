using System.Collections.Generic;
using System.Linq;
using Core.Code.ClientFeatureToggle;
using Core.Code.Input;
using Core.Code.Promises;
using Core.Meta.Social;
using Core.Meta.Social.Tables;
using MTGA.KeyboardManager;
using MTGA.Social;
using SharedClientCore.SharedClientCore.Code.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.Models.Network;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.CustomInput;
using Wotc.Mtga.Extensions;

public class TablesCornerUI : MonoBehaviour, IKeyUpSubscriber, IKeySubscriber, IKeyDownSubscriber, IAcceptActionHandler, IBackActionHandler, IActionBlocker
{
	[SerializeField]
	private Animator _animatorCornerIcon;

	[SerializeField]
	private CustomButton _cornerIconButton;

	[SerializeField]
	private GameObject _tablesCreateJoinListParent;

	[SerializeField]
	private Button _createButton;

	[SerializeField]
	private Button _joinButton;

	[SerializeField]
	private TMP_InputField _createJoinTableNameInput;

	[SerializeField]
	private Transform _tablesListParent;

	[SerializeField]
	private TableListTile _tableTilePrefab;

	[SerializeField]
	private TMP_Text _tableListPlaceholderText;

	private readonly List<TableListTile> _currentTables = new List<TableListTile>();

	[SerializeField]
	private TablesPopupUI _tablesPopupPrefab;

	private ClientFeatureToggleDataProvider _toggleDataProvider;

	private TablesPopupUI _tablesPopupView;

	private ILobbyController _lobbyController;

	private IActionSystem _actions;

	private SocialUI _socialUI;

	private bool _disabled;

	private static readonly int HotHash = Animator.StringToHash("Hot");

	private static readonly int DisabledHash = Animator.StringToHash("Disabled");

	private const string TestLobbyPassword = "TestPassword";

	private TablesPopupUI TablesPopupView
	{
		get
		{
			SceneLoader sceneLoader = SceneLoader.GetSceneLoader();
			bool flag = sceneLoader != null;
			if (_tablesPopupView != null)
			{
				if (flag && _tablesPopupView.transform.parent != sceneLoader._popupsParent)
				{
					Object.Destroy(_tablesPopupView.gameObject);
					_tablesPopupView = sceneLoader.GetTablesPopup(_tablesPopupPrefab);
					_tablesPopupView.gameObject.UpdateActive(active: false);
				}
				return _tablesPopupView;
			}
			_tablesPopupView = (flag ? sceneLoader.GetTablesPopup(_tablesPopupPrefab) : Object.Instantiate(_tablesPopupPrefab, _socialUI.transform));
			_tablesPopupView.gameObject.UpdateActive(active: false);
			return _tablesPopupView;
		}
	}

	public bool Disabled
	{
		get
		{
			return _disabled;
		}
		set
		{
			if (_disabled != value)
			{
				_disabled = value;
				_cornerIconButton.enabled = !_disabled;
				if (_animatorCornerIcon != null && _animatorCornerIcon.isActiveAndEnabled)
				{
					_animatorCornerIcon.SetBool(DisabledHash, _disabled);
				}
			}
		}
	}

	private bool TableListVisible => _tablesCreateJoinListParent.gameObject.activeSelf;

	public bool SpecificTableViewVisible => TablesPopupView.IsShowing;

	public PriorityLevelEnum Priority => PriorityLevelEnum.Social;

	public void Init(IActionSystem actionSystem, SocialUI socialUI)
	{
		_toggleDataProvider = Pantry.Get<ClientFeatureToggleDataProvider>();
		_actions = actionSystem;
		_socialUI = socialUI;
		UpdateActive();
		_toggleDataProvider.RegisterForToggleUpdates(UpdateActive);
	}

	private void Awake()
	{
		_cornerIconButton.OnClick.AddListener(OnCornerButtonClicked);
		_createButton.onClick.AddListener(OnCreateTableButtonClicked);
		_joinButton.onClick.AddListener(OnJoinTableButtonClicked);
		_createJoinTableNameInput.onValueChanged.AddListener(OnCreateJoinInputValueChanged);
		_lobbyController = Pantry.Get<ILobbyController>();
		_lobbyController.LobbyUpdated += OnLobbyUpdated;
		_lobbyController.HistoryUpdated += OnHistoryUpdated;
		_lobbyController.NewLobbyInvite += OnNewLobbyInvite;
		_lobbyController.LobbyInviteRemoved += OnLobbyInviteRemoved;
		_lobbyController.LobbyClosed += OnLobbyClosed;
		_lobbyController.LobbyExited += OnLobbyExited;
		_lobbyController.LobbyKickPlayer += OnLobbyKicked;
		_tablesCreateJoinListParent.gameObject.UpdateActive(active: false);
		OnCreateJoinInputValueChanged(_createJoinTableNameInput.text);
	}

	private void OnDestroy()
	{
		_cornerIconButton.OnClick.RemoveListener(OnCornerButtonClicked);
		_createButton.onClick.RemoveListener(OnCreateTableButtonClicked);
		_joinButton.onClick.RemoveListener(OnJoinTableButtonClicked);
		_createJoinTableNameInput.onValueChanged.RemoveListener(OnCreateJoinInputValueChanged);
		_lobbyController.LobbyUpdated -= OnLobbyUpdated;
		_lobbyController.HistoryUpdated -= OnHistoryUpdated;
		_lobbyController.NewLobbyInvite -= OnNewLobbyInvite;
		_lobbyController.LobbyInviteRemoved -= OnLobbyInviteRemoved;
		_lobbyController.LobbyClosed -= OnLobbyClosed;
		_lobbyController.LobbyExited -= OnLobbyExited;
		_lobbyController.LobbyKickPlayer -= OnLobbyKicked;
		_toggleDataProvider.UnRegisterForToggleUpdates(UpdateActive);
	}

	private void OnEnable()
	{
		_animatorCornerIcon.SetBool(DisabledHash, Disabled);
		UpdateActive();
	}

	private void Update()
	{
		if (((!PlatformUtils.IsHandheld() && CustomInputModule.PointerIsHeldDown()) || CustomInputModule.IsRightClick()) && _tablesCreateJoinListParent.gameObject.activeInHierarchy && !((RectTransform)_tablesCreateJoinListParent.transform).GetMouseOver() && !((RectTransform)_cornerIconButton.transform).GetMouseOver())
		{
			Minimize();
		}
	}

	private void UpdateActive()
	{
		if (_toggleDataProvider != null)
		{
			base.gameObject.SetActive(_toggleDataProvider.GetToggleValueById("Lobby"));
		}
	}

	public void UpdateCornerIconActive(bool active)
	{
		_cornerIconButton.gameObject.SetActive(active);
	}

	public void OnHistoryUpdated(string lobbyId, List<LobbyMessage> lobbyHistory)
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			OnHistoryUpdatedMainThread(lobbyId, lobbyHistory);
		});
	}

	public void OnHistoryUpdatedMainThread(string lobbyId, List<LobbyMessage> lobbyHistory)
	{
		LobbyMessage lobbyMessage = lobbyHistory.Last();
		if (!TableListVisible && !SpecificTableViewVisible)
		{
			_animatorCornerIcon.SetBool(HotHash, value: true);
			_lobbyController.GetLobbies().TryGetValue(lobbyId, out var value);
			if (value != null && !lobbyMessage.Message.StartsWith("\u009b\u0080\u0099\u0091\u0092"))
			{
				_lobbyController.ForwardNotificationAlert(new SocialMessage(value, lobbyMessage));
			}
		}
	}

	public void OnLobbyUpdated(Client_Lobby lobby)
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			OnLobbyUpdatedMainThread(lobby);
		});
	}

	public void OnLobbyUpdatedMainThread(Client_Lobby lobby)
	{
		UpdateTableListTiles();
	}

	public void OnLobbyClosed(string lobbyId)
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			OnLobbyUpdatedMainThread(lobbyId);
		});
	}

	public void OnLobbyKicked(Client_LobbyKick lobbyKick)
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			OnLobbyKickedMainThread(lobbyKick);
		});
	}

	public void OnLobbyKickedMainThread(Client_LobbyKick lobbyKick)
	{
		UpdateTableListTiles();
	}

	public void OnLobbyUpdatedMainThread(string lobbyId)
	{
		UpdateTableListTiles();
	}

	public void OnNewLobbyInvite(Client_LobbyInvite lobbyInvite)
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			OnNewLobbyInviteMainThread(lobbyInvite);
		});
	}

	public void OnNewLobbyInviteMainThread(Client_LobbyInvite lobbyInvite)
	{
		UpdateTableListTiles();
		if (!TableListVisible)
		{
			_animatorCornerIcon.SetBool(HotHash, value: true);
			_lobbyController.ForwardNotificationAlert(new SocialMessage(lobbyInvite));
		}
	}

	public void OnLobbyInviteRemoved(string lobbyId)
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			OnLobbyInviteRemovedMainThread(lobbyId);
		});
	}

	public void OnLobbyInviteRemovedMainThread(string lobby)
	{
		UpdateTableListTiles();
	}

	public void OnLobbyExited(string lobbyId)
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			OnLobbyExitedMainThread(lobbyId);
		});
	}

	public void OnLobbyExitedMainThread(string lobbyId)
	{
		UpdateTableListTiles();
	}

	private void OnCornerButtonClicked()
	{
		AudioManager.PlayAudio((!TableListVisible) ? "sfx_ui_friends_open" : "sfx_ui_friends_close", _animatorCornerIcon.gameObject);
		if (!TableListVisible && !SpecificTableViewVisible)
		{
			ShowTablesList();
			SceneLoader sceneLoader = SceneLoader.GetSceneLoader();
			if (sceneLoader.CurrentContentType == NavContentType.Home && sceneLoader.CurrentNavContent is HomePageContentController homePageContentController)
			{
				homePageContentController.ObjectivesPanel?.CloseAllObjectivePopups();
			}
		}
		else
		{
			Minimize();
		}
	}

	private void OnCreateTableButtonClicked()
	{
		if (!_lobbyController.GetLobbies().ContainsKey(_createJoinTableNameInput.text))
		{
			_lobbyController.CreateLobby(_createJoinTableNameInput.text, "TestPassword").ThenOnMainThread(delegate(Promise<Client_Lobby> p)
			{
				UpdateTableListTiles();
				OpenSpecificTableView(p);
			});
		}
	}

	private void OnJoinTableButtonClicked()
	{
		if (_lobbyController.GetLobbies().TryGetValue(_createJoinTableNameInput.text, out var value))
		{
			OpenSpecificTableView(value);
			return;
		}
		_lobbyController.JoinLobby("", _createJoinTableNameInput.text).ThenOnMainThread(delegate(Promise<Client_Lobby> p)
		{
			UpdateTableListTiles();
			OpenSpecificTableView(p);
		});
	}

	public void OnCreateJoinInputValueChanged(string latestValue)
	{
		bool interactable = !string.IsNullOrWhiteSpace(latestValue);
		_joinButton.interactable = interactable;
	}

	public void ShowTablesList()
	{
		_animatorCornerIcon.SetBool(HotHash, value: false);
		_tablesCreateJoinListParent.gameObject.UpdateActive(active: true);
		UpdateTableListTiles();
	}

	public void UpdateTableListTiles()
	{
		foreach (TableListTile currentTable in _currentTables)
		{
			Object.Destroy(currentTable.gameObject);
		}
		_currentTables.Clear();
		string key;
		foreach (KeyValuePair<string, Client_Lobby> lobby in _lobbyController.GetLobbies())
		{
			lobby.Deconstruct(out key, out var value);
			string lobbyId = key;
			Client_Lobby lobbyData = value;
			TableListTile tableListTile = Object.Instantiate(_tableTilePrefab, _tablesListParent);
			tableListTile.Init(this, lobbyId);
			tableListTile.SetLobbyData(lobbyData);
			_currentTables.Add(tableListTile);
		}
		foreach (KeyValuePair<string, Client_LobbyInvite> lobbyInvite in _lobbyController.GetLobbyInvites())
		{
			lobbyInvite.Deconstruct(out key, out var value2);
			string lobbyId2 = key;
			Client_LobbyInvite lobbyInviteData = value2;
			TableListTile tableListTile2 = Object.Instantiate(_tableTilePrefab, _tablesListParent);
			tableListTile2.Init(this, lobbyId2);
			tableListTile2.SetLobbyInviteData(lobbyInviteData);
			_currentTables.Add(tableListTile2);
		}
		_tableListPlaceholderText.gameObject.UpdateActive(_currentTables.Count == 0);
	}

	public void OpenSpecificTableView(Promise<Client_Lobby> lobbyPromise)
	{
		if (lobbyPromise.Successful)
		{
			_animatorCornerIcon.SetBool(HotHash, value: false);
			OpenSpecificTableView(lobbyPromise.Result);
		}
		else
		{
			SimpleLog.LogError($"Lobby promise {lobbyPromise} failed");
		}
	}

	public void OpenSpecificTableView(Client_Lobby lobby)
	{
		_animatorCornerIcon.SetBool(HotHash, value: false);
		_tablesCreateJoinListParent.gameObject.UpdateActive(active: false);
		TablesPopupView.SetTableInfo(this, lobby);
		TablesPopupView.Activate(activate: true);
	}

	public void JoinTable(string lobbyId)
	{
		_lobbyController.JoinLobby(lobbyId);
	}

	public void Minimize()
	{
		CloseTablesList();
		CloseSpecificTableView();
		IActionSystem actions = _actions;
		if (actions != null && actions.IsCurrentFocus(this))
		{
			_actions.PopFocus(this);
		}
	}

	public void CloseTablesList()
	{
		if (TableListVisible)
		{
			_tablesCreateJoinListParent.gameObject.UpdateActive(active: false);
		}
	}

	public void CloseSpecificTableView()
	{
		if (SpecificTableViewVisible)
		{
			TablesPopupView.Activate(activate: false);
		}
	}

	public bool HandleKeyDown(KeyCode curr, Modifiers mods)
	{
		if (Disabled)
		{
			return false;
		}
		if (curr == KeyCode.Escape && SpecificTableViewVisible)
		{
			CloseSpecificTableView();
			return true;
		}
		return SpecificTableViewVisible;
	}

	public bool HandleKeyUp(KeyCode curr, Modifiers mods)
	{
		if (Disabled)
		{
			return false;
		}
		if (curr == KeyCode.Escape && SpecificTableViewVisible)
		{
			return true;
		}
		return SpecificTableViewVisible;
	}

	public void OnAccept()
	{
	}

	public void OnBack(ActionContext context)
	{
		if (SpecificTableViewVisible)
		{
			CloseSpecificTableView();
		}
	}
}
