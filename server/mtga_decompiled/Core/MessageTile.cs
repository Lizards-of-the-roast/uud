using System;
using MTGA.Social;
using UnityEngine;
using UnityEngine.EventSystems;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class MessageTile : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private Localize _title;

	[SerializeField]
	private Localize _body;

	[SerializeField]
	private CustomButton _acceptChallengeButton;

	[SerializeField]
	private CustomButton _acceptInviteButton;

	[SerializeField]
	private CustomButton _acceptTournamentButton;

	private Animator _animator;

	private bool _asNotification;

	private static readonly int ChatWindowModeHash = Animator.StringToHash("ChatWindowMode");

	private static readonly int ReadHash = Animator.StringToHash("Read");

	private static readonly int FriendOfflineHash = Animator.StringToHash("FriendOffline");

	private static readonly int ChallengeHash = Animator.StringToHash("Challenge");

	private static readonly int OutroHash = Animator.StringToHash("Outro");

	public SocialMessage Message { get; private set; }

	public event Action<SocialMessage> NotificationClicked;

	public event Action<SocialMessage> AcceptClicked;

	public event Action<MessageTile> Disabled;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
	}

	private void OnDisable()
	{
		this.Disabled?.Invoke(this);
	}

	public void Init(SocialMessage message, bool asNotification = false)
	{
		Message = message;
		_asNotification = asNotification;
		_animator.SetBool(ChatWindowModeHash, !_asNotification);
		_animator.SetBool(ReadHash, message.Seen == MessageSeen.InChat);
		_animator.SetBool(FriendOfflineHash, !message.Friend.IsOnline);
		_animator.SetBool(ChallengeHash, message.Type == MessageType.Challenge);
		UpdateNotification();
		_body.SetText(message.TextBody);
		_title.gameObject.UpdateActive(!string.IsNullOrEmpty(message.TextTitle));
		_title.SetText(message.TextTitle);
		if (asNotification && PlatformUtils.IsHandheld())
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public void UpdateNotification()
	{
		bool active = Message.Type == MessageType.Invite && Message.Direction == Direction.Incoming && !Message.Canceled;
		bool active2 = Message.Type == MessageType.Challenge && Message.Direction != Direction.None && !Message.Canceled;
		bool active3 = (Message.Type == MessageType.TournamentIsReady || Message.Type == MessageType.TournamentRoundIsReady) && !Message.Canceled;
		_acceptInviteButton?.gameObject.UpdateActive(active);
		_acceptChallengeButton?.gameObject.UpdateActive(active2);
		_acceptTournamentButton?.gameObject.UpdateActive(active3);
	}

	public bool IsNotificationTile()
	{
		return !_animator.GetBool(ChatWindowModeHash);
	}

	public bool IsChatTile()
	{
		return _animator.GetBool(ChatWindowModeHash);
	}

	public void Cleanup()
	{
		Message = null;
		this.NotificationClicked = null;
		this.Disabled = null;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (_asNotification)
		{
			this.NotificationClicked?.Invoke(Message);
		}
	}

	public void Unity_OnAcceptClick()
	{
		this.AcceptClicked?.Invoke(Message);
	}
}
