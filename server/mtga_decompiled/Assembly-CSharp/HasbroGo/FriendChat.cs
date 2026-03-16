using System.Collections.Generic;
using HasbroGo.Helpers;
using HasbroGo.Logging;
using HasbroGo.Social.Events;
using HasbroGo.Social.Models;
using TMPro;
using UnityEngine;

namespace HasbroGo;

public class FriendChat : MonoBehaviour
{
	[SerializeField]
	private TMP_InputField inputField;

	[SerializeField]
	private GameObject chatBoxContent;

	[SerializeField]
	private GameObject messagePrefab;

	[SerializeField]
	private TextMeshProUGUI displayName;

	private List<GameObject> chatMessages = new List<GameObject>();

	private Queue<DirectMessageReceivedEventArgs> messages = new Queue<DirectMessageReceivedEventArgs>();

	private readonly string logCategory = "FriendChat";

	public Friend Friend { get; private set; }

	public void InitChat(Friend friend)
	{
		Friend = friend;
		displayName.text = ((friend.DisplayName.ToString() != string.Empty) ? friend.DisplayName.ToString() : friend.AccountId);
		QueueAllMessages(friend.AccountId);
	}

	private void OnEnable()
	{
		inputField.onSubmit.AddListener(SendDirectMessage);
	}

	private void OnDisable()
	{
		inputField.onSubmit.RemoveListener(SendDirectMessage);
	}

	private void LateUpdate()
	{
		lock (messages)
		{
			DirectMessageReceivedEventArgs result;
			while (messages.TryDequeue(out result))
			{
				AddMessageToChatContents(result.Message.SenderAccountId, result.Message.Payload);
			}
		}
	}

	public void QueueMessage(DirectMessageReceivedEventArgs e)
	{
		messages.Enqueue(e);
	}

	private void AddMessageToChatContents(string accountId, string message)
	{
		if (string.IsNullOrEmpty(message))
		{
			LogHelper.Logger.Log(logCategory, LogLevel.Error, "Tried to add empty message");
			return;
		}
		GameObject gameObject = Object.Instantiate(messagePrefab, chatBoxContent.transform);
		gameObject.GetComponent<TextMeshProUGUI>().text = message;
		chatMessages.Add(gameObject);
		if (accountId == Friend.AccountId)
		{
			gameObject.GetComponent<TextMeshProUGUI>().color = Color.red;
		}
	}

	private void ClearChatMessages()
	{
		foreach (GameObject chatMessage in chatMessages)
		{
			Object.Destroy(chatMessage);
		}
		chatMessages.Clear();
	}

	private void QueueAllMessages(string accountId)
	{
		ClearChatMessages();
		foreach (DirectMessageReceivedEventArgs accountIdChatMessage in SocialManager.Instance.GetAccountIdChatMessages(accountId))
		{
			QueueMessage(accountIdChatMessage);
		}
	}

	private void SendDirectMessage(string message)
	{
		if (!string.IsNullOrEmpty(message))
		{
			AddMessageToChatContents(HasbroGoSDKManager.Instance.HasbroGoSdk.AuthManager.GetAccountId(), inputField.text);
			SocialManager.Instance.SendDirectChatMessage(Friend, inputField.text);
			inputField.text = string.Empty;
		}
	}
}
