using System.Collections.Generic;
using UnityEngine;

namespace MTGA.Social;

public class Conversation
{
	public readonly List<SocialMessage> MessageHistory = new List<SocialMessage>();

	public bool HasUnseenMessages;

	public bool HasUnseenChallenge;

	public string ConversationId { get; }

	public string RelationshipId { get; internal set; }

	public SocialEntity Friend { get; }

	public int ChatMessageCount { get; private set; }

	public string MessageDraft { get; private set; }

	public int ActivityTimestamp { get; private set; }

	public Conversation(SocialEntity friend)
	{
		Friend = friend;
	}

	public void AddSocialMessage(SocialMessage message)
	{
		MessageHistory.Add(message);
		if (message.Type == MessageType.Chat)
		{
			ChatMessageCount++;
			Friend.HasChatHistory = ChatMessageCount > 0 || !string.IsNullOrEmpty(MessageDraft);
			ActivityTimestamp = Time.frameCount;
		}
		else if (message.Type == MessageType.Challenge && message.Direction == Direction.Incoming)
		{
			ActivityTimestamp = Time.frameCount;
		}
	}

	public void RemoveSocialMessage(SocialMessage message)
	{
		MessageHistory.Remove(message);
		if (message.Type == MessageType.Chat)
		{
			ChatMessageCount--;
			Friend.HasChatHistory = ChatMessageCount > 0 || !string.IsNullOrEmpty(MessageDraft);
		}
	}

	public void SaveMessageDraft(string message)
	{
		MessageDraft = message;
		if (!string.IsNullOrEmpty(message))
		{
			ActivityTimestamp = Time.frameCount;
		}
		Friend.HasChatHistory = ChatMessageCount > 0 || !string.IsNullOrEmpty(MessageDraft);
	}
}
