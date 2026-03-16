using System.Collections.Generic;
using Core.Code.Promises;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.Models.Network;
using Wizards.Mtga;

namespace Core.Meta.Social.Tables;

public class TablesPopupChatWindow : MonoBehaviour
{
	[SerializeField]
	private Transform _chatBubblesContainer;

	[SerializeField]
	private TMP_InputField _chatInputField;

	[SerializeField]
	private Button _sendChatButton;

	[SerializeField]
	private TablesPopupChatBubble _prefabMessageIncoming;

	[SerializeField]
	private TablesPopupChatBubble _prefabMessageOutgoing;

	private string _currentLobbyId;

	private readonly List<LobbyPlayer> _knownPlayers = new List<LobbyPlayer>();

	private readonly List<TablesPopupChatBubble> _currentChatItems = new List<TablesPopupChatBubble>();

	private IAccountClient AccountClient => Pantry.Get<IAccountClient>();

	private ILobbyController LobbyController => Pantry.Get<ILobbyController>();

	public void Awake()
	{
		_sendChatButton.onClick.AddListener(SendChatMessage);
		_chatInputField.onSubmit.AddListener(SendChatMessage);
		LobbyController.HistoryUpdated += OnLobbyHistoryUpdated;
	}

	public void OnDestroy()
	{
		_sendChatButton.onClick.RemoveListener(SendChatMessage);
		_chatInputField.onSubmit.RemoveListener(SendChatMessage);
		LobbyController.HistoryUpdated -= OnLobbyHistoryUpdated;
	}

	public void SendChatMessage()
	{
		SendChatMessage(_chatInputField.text);
	}

	public void SendChatMessage(string _)
	{
		if (!_chatInputField.wasCanceled && !string.IsNullOrWhiteSpace(_chatInputField.text))
		{
			int num = Mathf.Max(0, _chatInputField.caretPosition);
			string text = _chatInputField.text;
			if (!string.IsNullOrEmpty(text) && num < text.Length && text[num] == '\n')
			{
				_chatInputField.text = text.Remove(num);
			}
			if (num != text.Length)
			{
				_chatInputField.text = text.Replace("\n", "");
			}
			LobbyController.SendLobbyMessage(_currentLobbyId, _chatInputField.text);
			_chatInputField.text = string.Empty;
			_chatInputField.caretPosition = 0;
			_chatInputField.ActivateInputField();
		}
	}

	public void UpdateLobbyInfo(string currentLobbyId, List<LobbyPlayer> players)
	{
		_currentLobbyId = currentLobbyId;
		_knownPlayers.Clear();
		_knownPlayers.AddRange(players);
	}

	public void OnLobbyHistoryUpdated(string lobbyId, List<LobbyMessage> history)
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			OnLobbyHistoryUpdatedMainThread(lobbyId, history);
		});
	}

	public void OnLobbyHistoryUpdatedMainThread(string lobbyId, List<LobbyMessage> history)
	{
		if (!(_currentLobbyId != lobbyId))
		{
			UpdateChatHistory(history);
		}
	}

	public void UpdateChatHistory(List<LobbyMessage> history)
	{
		foreach (TablesPopupChatBubble currentChatItem in _currentChatItems)
		{
			Object.Destroy(currentChatItem.gameObject);
		}
		_currentChatItems.Clear();
		IAccountClient accountClient = AccountClient;
		foreach (LobbyMessage chatItem in history)
		{
			TablesPopupChatBubble original = ((accountClient.AccountInformation.PersonaID == chatItem.PlayerId) ? _prefabMessageOutgoing : _prefabMessageIncoming);
			LobbyPlayer playerInfo = _knownPlayers.Find((LobbyPlayer p) => p.PlayerId == chatItem.PlayerId);
			TablesPopupChatBubble tablesPopupChatBubble = Object.Instantiate(original, _chatBubblesContainer);
			tablesPopupChatBubble.SetTextAndBubbles(playerInfo, chatItem.Message);
			_currentChatItems.Add(tablesPopupChatBubble);
		}
	}
}
