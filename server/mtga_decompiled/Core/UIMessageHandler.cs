using System;
using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

public class UIMessageHandler : IDisposable
{
	private Action<UIMessage> _sendUIMessage;

	private readonly Dictionary<uint, TimerPlus> _outgoingHoverThrottleTimer = new Dictionary<uint, TimerPlus>();

	public event Action<string> EmoteRecievedCallback;

	public event Action<string> EmoteSentCallback;

	public event Action<uint> CardHoverChanged;

	public event Action<string, string> GenericEventReceived;

	public void UpdateSendUIMessageCallback(Action<UIMessage> sendUIMessage)
	{
		_sendUIMessage = sendUIMessage;
	}

	public void Dispose()
	{
		_sendUIMessage = null;
		this.EmoteRecievedCallback = null;
		this.EmoteSentCallback = null;
		this.GenericEventReceived = null;
		this.CardHoverChanged = null;
	}

	public void OnUIMessage(UIMessage msg)
	{
		if (msg.OnHover != null)
		{
			Handle_OnHover(msg.OnHover);
		}
		_ = msg.OnSelect;
		_ = msg.OnShuffle;
		if (msg.OnChat != null)
		{
			Handle_OnChat(msg.OnChat);
		}
		if (msg.OnGenericEvent != null)
		{
			Handle_OnGenericEvent(msg.OnGenericEvent);
		}
	}

	private void Handle_OnChat(OnChat chatMsg)
	{
		this.EmoteRecievedCallback?.Invoke(chatMsg.Text);
	}

	public bool TrySendEmote(string emote)
	{
		SendEmoteMessage(emote);
		return true;
	}

	private void SendEmoteMessage(string emote)
	{
		UIMessage obj = new UIMessage
		{
			OnChat = new OnChat
			{
				Text = emote
			}
		};
		_sendUIMessage?.Invoke(obj);
		this.EmoteSentCallback?.Invoke(emote);
	}

	private void Handle_OnHover(OnHover hoverMsg)
	{
		this.CardHoverChanged?.Invoke(hoverMsg.ObjectId);
	}

	public void TrySendHoverMessage(uint targetCard)
	{
		bool flag = false;
		if (!_outgoingHoverThrottleTimer.TryGetValue(targetCard, out var value))
		{
			TimerPlus timerPlus = new TimerPlus();
			timerPlus.Interval = 185.0;
			timerPlus.Start();
			timerPlus.AutoReset = false;
			_outgoingHoverThrottleTimer.Add(targetCard, timerPlus);
			flag = true;
		}
		else if (value.TimeLeft <= 0.009999999776482582)
		{
			flag = true;
			value.Stop();
			value.Start();
		}
		if (flag)
		{
			SendHoverMessage(targetCard);
		}
	}

	private void SendHoverMessage(uint targetCard)
	{
		UIMessage obj = new UIMessage
		{
			OnHover = new OnHover
			{
				ObjectId = targetCard
			}
		};
		_sendUIMessage?.Invoke(obj);
	}

	private void Handle_OnGenericEvent(OnGenericEvent genericEvent)
	{
		this.GenericEventReceived?.Invoke(genericEvent.Category, genericEvent.Payload);
	}

	public bool TrySendGenericEvent(string category, string payload)
	{
		SendGenericEvent(category, payload);
		return true;
	}

	private void SendGenericEvent(string category, string payload)
	{
		UIMessage obj = new UIMessage
		{
			OnGenericEvent = new OnGenericEvent
			{
				Category = category,
				Payload = payload
			}
		};
		_sendUIMessage?.Invoke(obj);
	}
}
