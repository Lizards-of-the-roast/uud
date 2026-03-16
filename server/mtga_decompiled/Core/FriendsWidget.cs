using System;
using System.Collections.Generic;
using Core.Code.Input;
using Core.Code.Promises;
using Core.Meta.MainNavigation.Challenge;
using MTGA.Social;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.Promises;
using Wizards.Mtga.PrivateGame;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class FriendsWidget : MonoBehaviour
{
	private static readonly int Disabled = Animator.StringToHash("Disabled");

	[Header("UnifiedChallenge")]
	[SerializeField]
	private CustomButton CreateChallengeButton;

	[SerializeField]
	private Animator CreateChallengeAnimator;

	[SerializeField]
	private GameObject ChallengeWidget;

	public SocialEntityListHeader SectionIncomingChallengeRequest;

	public GameObject ActiveChallengeAnchor;

	[SerializeField]
	private CurrentChallengeTile _prefabCurrentChallengeTile;

	[SerializeField]
	private IncomingChallengeRequestTile _prefabChallengeRequestTile;

	[Header("Social Entities List Widget")]
	public CustomButton ButtonOpenFriendInvitePrompt;

	public GameObject AddFriendImage;

	public SocialEntityListHeader SectionOpponents;

	public SocialEntityListHeader SectionFriends;

	public SocialEntityListHeader SectionIncomingInvites;

	public SocialEntityListHeader SectionOutgoingInvites;

	public SocialEntityListHeader SectionBlocks;

	[Header("Presence Status")]
	public TMP_Text UsernameText;

	public TMP_Text StatusText;

	public CustomButton StatusButton;

	public GameObject StatusDropdownParent;

	public Button BusyStatusButton;

	public Button OfflineStatusButton;

	public Button OnlineStatusButton;

	[Header("Challenge Virtualized Scroll View")]
	public ScrollRect ChallengeListScroll;

	public float ChallengeWidgetSize = 10f;

	public float ChallengeHeaderSize = 10f;

	public float ChallengePaddingBottom = 10f;

	[Header("Friends Virtualized Scroll View")]
	public ScrollRect FriendsListScroll;

	public float WidgetSize = 10f;

	public float HeaderSize = 10f;

	public float PaddingBottom = 10f;

	[Header("Friend Tile Prefabs")]
	[SerializeField]
	private OpponentTile _prefabOpponentTile;

	[SerializeField]
	private FriendTile _prefabFriendTile;

	[SerializeField]
	private InviteIncomingTile _prefabInviteIncomingTile;

	[SerializeField]
	private InviteOutgoingTile _prefabInviteOutgoingTile;

	[SerializeField]
	private BlockTile _prefabBlockTile;

	private SocialUI _socialUI;

	private ISocialManager _socialManager;

	private PVPChallengeController _challengeController;

	private RectTransform _topSortingRoot;

	private readonly List<InviteIncomingTile> _activeInviteIncomingTiles = new List<InviteIncomingTile>();

	private readonly List<InviteOutgoingTile> _activeInviteOutgoingTiles = new List<InviteOutgoingTile>();

	private readonly List<OpponentTile> _activeOpponentTiles = new List<OpponentTile>();

	public readonly List<FriendTile> _activeFriendTiles = new List<FriendTile>();

	private readonly List<BlockTile> _activeBlockTiles = new List<BlockTile>();

	public readonly List<IncomingChallengeRequestTile> _activeChallengeRequestTiles = new List<IncomingChallengeRequestTile>();

	private CurrentChallengeTile _currentChallengeTile;

	private float _socialEntitiesListHeight;

	private float _challengeListHeight;

	private IActionSystem _actionSystem;

	private bool _bisInDuelScene;

	private bool AllowCreateOrJoin
	{
		get
		{
			if (_challengeController == null)
			{
				return false;
			}
			if (_challengeController.GetActiveCurrentChallengeData() == null)
			{
				return _challengeController.ChallengePermissionState != ChallengeDataProvider.ChallengePermissionState.Restricted_InMatch;
			}
			return false;
		}
	}

	private void OnDestroy()
	{
		if (_challengeController != null)
		{
			_challengeController.UnRegisterForChallengeChanges(HandleChallengeDataChanges);
			_challengeController.ChallengeEnabledChanged -= OnChallengeEnabledChanged;
			_challengeController = null;
		}
		CreateChallengeButton.OnClick.RemoveAllListeners();
	}

	public void Init(SocialUI socialUI, ISocialManager socialManager, PVPChallengeController challengeController, RectTransform topSortingRoot, IActionSystem actionSystem, bool bisInDuelScene)
	{
		_socialUI = socialUI;
		_socialManager = socialManager;
		_challengeController = challengeController;
		_topSortingRoot = topSortingRoot;
		_actionSystem = actionSystem;
		_bisInDuelScene = bisInDuelScene;
		UsernameText.text = _socialManager.LocalPlayer.DisplayName;
		SectionBlocks.IsOpen = false;
		OnChallengeEnabledChanged(_challengeController.IsChallengeEnabled());
		UpdateSocialEntityList();
		UpdateChallengeList();
		BindClickHandlers();
		FriendsListScroll.onValueChanged.AddListener(delegate
		{
			_socialUI.UpdateSocialEntityList();
		});
		BindSectionChangeHandlers();
		_challengeController.RegisterForChallengeChanges(HandleChallengeDataChanges);
		CreateChallengeButton.OnClick.AddListener(OnCreateChallengeClicked);
		UpdateChallengeButtonActive();
		_challengeController.ChallengeEnabledChanged += OnChallengeEnabledChanged;
	}

	private void HandleChallengeDataChanges(PVPChallengeData challenge)
	{
		UpdateChallengeButtonActive();
		UpdateSocialEntityList();
	}

	private void UpdateChallengeButtonActive()
	{
		bool allowCreateOrJoin = AllowCreateOrJoin;
		CreateChallengeAnimator.SetBool(Disabled, _bisInDuelScene || !allowCreateOrJoin);
		CreateChallengeButton.Interactable = !_bisInDuelScene && allowCreateOrJoin;
	}

	public void OnChallengeEnabledChanged(bool isChallengeEnabled)
	{
		ChallengeWidget.UpdateActive(isChallengeEnabled);
	}

	private void OnCreateChallengeClicked()
	{
		_challengeController.CreateAndCacheChallenge().ThenOnMainThread(delegate(Promise<PVPChallengeData> promise)
		{
			if (promise.Successful)
			{
				PVPChallengeData result = promise.Result;
				if (_challengeController.CanOpenChallenge(result.ChallengeId))
				{
					_socialUI.OpenPlayBlade(result.ChallengeId);
				}
			}
		});
	}

	private void OnOpenChallengeScreenButtonClicked(Guid challengeId)
	{
		_socialUI.OpenPlayBlade(challengeId);
	}

	private void BindClickHandlers()
	{
		ButtonOpenFriendInvitePrompt.OnClick.AddListener(_socialUI.LoadFriendInvitePanel);
		StatusButton.OnClick.AddListener(delegate
		{
			AudioManager.PlayAudio("sfx_ui_friends_click", _socialUI.AnimatorCornerIcon.gameObject);
			StatusDropdownParent.gameObject.SetActive(!StatusDropdownParent.gameObject.activeSelf);
		});
		BusyStatusButton.onClick.AddListener(delegate
		{
			AudioManager.PlayAudio("sfx_ui_friends_status_busy", _socialUI.AnimatorCornerIcon.gameObject);
			UpdateLocalUserPresenceStatus(PresenceStatus.Busy);
		});
		OnlineStatusButton.onClick.AddListener(delegate
		{
			AudioManager.PlayAudio("sfx_ui_friends_status_online", _socialUI.AnimatorCornerIcon.gameObject);
			UpdateLocalUserPresenceStatus(PresenceStatus.Available);
		});
		OfflineStatusButton.onClick.AddListener(delegate
		{
			AudioManager.PlayAudio("sfx_ui_friends_status_offline", _socialUI.AnimatorCornerIcon.gameObject);
			UpdateLocalUserPresenceStatus(PresenceStatus.Offline);
		});
	}

	private void UpdateLocalUserPresenceStatus(PresenceStatus newStatus)
	{
		PVPChallengeData activeCurrentChallengeData = _challengeController.GetActiveCurrentChallengeData();
		bool num = activeCurrentChallengeData?.ChallengeOwnerId.Equals(activeCurrentChallengeData.LocalPlayerId) ?? false;
		bool flag = activeCurrentChallengeData != null && activeCurrentChallengeData.Status == ChallengeStatus.InGame;
		if (num && !flag && newStatus == PresenceStatus.Offline)
		{
			string title = new MTGALocalizedString
			{
				Key = "DuelScene/ClientPrompt/Are_You_Sure_Title"
			};
			string message = new MTGALocalizedString
			{
				Key = "Social/Presence/OfflineConfirmationBody"
			};
			SystemMessageManager.ShowSystemMessage(title, message, showCancel: true, delegate
			{
				_socialManager.SetUserPresenceStatus(newStatus);
			});
		}
		else
		{
			_socialManager.SetUserPresenceStatus(newStatus);
		}
		StatusDropdownParent.gameObject.SetActive(value: false);
	}

	private void BindSectionChangeHandlers()
	{
		SocialEntityListHeader sectionOpponents = SectionOpponents;
		sectionOpponents.IsOpenChanged = (Action<bool>)Delegate.Combine(sectionOpponents.IsOpenChanged, new Action<bool>(_socialUI.HandleSectionChanged));
		SocialEntityListHeader sectionFriends = SectionFriends;
		sectionFriends.IsOpenChanged = (Action<bool>)Delegate.Combine(sectionFriends.IsOpenChanged, new Action<bool>(_socialUI.HandleSectionChanged));
		SocialEntityListHeader sectionIncomingInvites = SectionIncomingInvites;
		sectionIncomingInvites.IsOpenChanged = (Action<bool>)Delegate.Combine(sectionIncomingInvites.IsOpenChanged, new Action<bool>(_socialUI.HandleSectionChanged));
		SocialEntityListHeader sectionOutgoingInvites = SectionOutgoingInvites;
		sectionOutgoingInvites.IsOpenChanged = (Action<bool>)Delegate.Combine(sectionOutgoingInvites.IsOpenChanged, new Action<bool>(_socialUI.HandleSectionChanged));
		SocialEntityListHeader sectionBlocks = SectionBlocks;
		sectionBlocks.IsOpenChanged = (Action<bool>)Delegate.Combine(sectionBlocks.IsOpenChanged, new Action<bool>(_socialUI.HandleSectionChanged));
		SocialEntityListHeader sectionIncomingChallengeRequest = SectionIncomingChallengeRequest;
		sectionIncomingChallengeRequest.IsOpenChanged = (Action<bool>)Delegate.Combine(sectionIncomingChallengeRequest.IsOpenChanged, new Action<bool>(_socialUI.HandleSectionChanged));
	}

	public void UpdatePresenceStatus(PresenceStatus currentStatus)
	{
		bool active = currentStatus != PresenceStatus.Offline;
		if ((bool)ButtonOpenFriendInvitePrompt)
		{
			ButtonOpenFriendInvitePrompt.gameObject.SetActive(active);
		}
		if ((bool)AddFriendImage)
		{
			AddFriendImage.SetActive(active);
		}
		switch (currentStatus)
		{
		case PresenceStatus.Available:
			StatusText.text = Languages.ActiveLocProvider.GetLocalizedText("Social/Friends/ConnectionState/Online");
			if (BusyStatusButton != null)
			{
				BusyStatusButton.gameObject.UpdateActive(active: true);
			}
			break;
		case PresenceStatus.Busy:
			StatusText.text = Languages.ActiveLocProvider.GetLocalizedText("Social/Friends/ConnectionState/Busy");
			break;
		case PresenceStatus.Offline:
			StatusText.text = Languages.ActiveLocProvider.GetLocalizedText("Social/Friends/ConnectionState/Offline");
			if (BusyStatusButton != null)
			{
				BusyStatusButton.gameObject.UpdateActive(active: false);
			}
			break;
		case PresenceStatus.Away:
			break;
		}
	}

	public void UpdateChallengeList()
	{
		if (ChallengeListScroll.verticalScrollbar.size < 0.1f)
		{
			ChallengeListScroll.verticalScrollbar.size = 0.1f;
		}
		float num = ChallengePaddingBottom;
		if (_challengeController.GetActiveCurrentChallengeData() != null)
		{
			num += WidgetSize;
		}
		List<PVPChallengeData> incomingChallengeRequests = _challengeController.GetIncomingChallengeRequests();
		if (incomingChallengeRequests.Count > 0)
		{
			num += ChallengeHeaderSize;
			if (SectionIncomingChallengeRequest.IsOpen)
			{
				num += WidgetSize * (float)incomingChallengeRequests.Count;
			}
		}
		if (Mathf.Abs(_challengeListHeight - num) > Mathf.Epsilon)
		{
			_challengeListHeight = num;
			ChallengeListScroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _challengeListHeight);
		}
		Rect rect = ChallengeListScroll.viewport.rect;
		float num2 = 1f - ChallengeListScroll.verticalNormalizedPosition;
		Rect bounds = new Rect(0f, num2 * (_challengeListHeight - rect.height), rect.width, rect.height);
		float pos = 0f;
		UpdateActiveChallenge(bounds, ref pos);
		UpdateIncomingChallengesList(bounds, ref pos);
	}

	public void UpdateSocialEntityList()
	{
		if (FriendsListScroll.verticalScrollbar.size < 0.1f)
		{
			FriendsListScroll.verticalScrollbar.size = 0.1f;
		}
		float num = PaddingBottom;
		if (_socialManager.InvitesIncoming.Count > 0)
		{
			num += HeaderSize;
			if (SectionIncomingInvites.IsOpen)
			{
				num += WidgetSize * (float)_socialManager.InvitesIncoming.Count;
			}
		}
		if (_socialManager.Friends.Count > 0)
		{
			num += HeaderSize;
			if (SectionFriends.IsOpen)
			{
				num += WidgetSize * (float)_socialManager.Friends.Count;
			}
		}
		if (_socialManager.InvitesOutgoing.Count > 0)
		{
			num += HeaderSize;
			if (SectionOutgoingInvites.IsOpen)
			{
				num += WidgetSize * (float)_socialManager.InvitesOutgoing.Count;
			}
		}
		if (_socialManager.Blocks.Count > 0)
		{
			num += HeaderSize;
			if (SectionBlocks.IsOpen)
			{
				num += WidgetSize * (float)_socialManager.Blocks.Count;
			}
		}
		if (Mathf.Abs(_socialEntitiesListHeight - num) > Mathf.Epsilon)
		{
			_socialEntitiesListHeight = num;
			FriendsListScroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _socialEntitiesListHeight);
		}
		Rect rect = FriendsListScroll.viewport.rect;
		float num2 = 1f - FriendsListScroll.verticalNormalizedPosition;
		Rect bounds = new Rect(0f, num2 * (_socialEntitiesListHeight - rect.height), rect.width, rect.height);
		float pos = 0f;
		UpdateInvitesIncomingList(bounds, ref pos);
		UpdateFriendsList(bounds, ref pos);
		UpdateInvitesOutgoingList(bounds, ref pos);
		UpdateBlocksList(bounds, ref pos);
	}

	private void UpdateInvitesIncomingList(Rect bounds, ref float pos)
	{
		((RectTransform)SectionIncomingInvites.transform).anchoredPosition = new Vector2(0f, 0f - pos);
		SectionIncomingInvites.SetCount(_socialManager.InvitesIncoming.Count);
		if (_socialManager.InvitesIncoming.Count > 0)
		{
			pos += HeaderSize;
		}
		int a = Mathf.FloorToInt((bounds.yMin - pos) / WidgetSize);
		int a2 = Mathf.CeilToInt((bounds.yMax - pos) / WidgetSize);
		a = Mathf.Max(a, 0);
		a2 = Mathf.Min(a2, _socialManager.InvitesIncoming.Count);
		int i = 0;
		if (SectionIncomingInvites.IsOpen)
		{
			for (; a < a2; a++)
			{
				if (i >= _activeInviteIncomingTiles.Count)
				{
					CreateWidget_FriendInviteIncoming();
				}
				InviteIncomingTile inviteIncomingTile = _activeInviteIncomingTiles[i];
				((RectTransform)inviteIncomingTile.transform).anchoredPosition = new Vector2(0f, (float)(-a) * WidgetSize);
				inviteIncomingTile.Init(_socialManager.InvitesIncoming[a], _topSortingRoot, _actionSystem);
				i++;
			}
			pos += (float)_socialManager.InvitesIncoming.Count * WidgetSize;
		}
		for (; i < _activeInviteIncomingTiles.Count; i++)
		{
			_activeInviteIncomingTiles[i].gameObject.UpdateActive(active: false);
		}
	}

	private void UpdateFriendsList(Rect bounds, ref float pos)
	{
		((RectTransform)SectionFriends.transform).anchoredPosition = new Vector2(0f, 0f - pos);
		SectionFriends.SetCount(_socialManager.Friends.Count);
		if (_socialManager.Friends.Count > 0)
		{
			pos += HeaderSize;
		}
		int a = Mathf.FloorToInt((bounds.yMin - pos) / WidgetSize);
		int a2 = Mathf.CeilToInt((bounds.yMax - pos) / WidgetSize);
		a = Mathf.Max(a, 0);
		a2 = Mathf.Min(a2, _socialManager.Friends.Count);
		int i = 0;
		if (SectionFriends.IsOpen)
		{
			for (; a < a2; a++)
			{
				if (i >= _activeFriendTiles.Count)
				{
					CreateWidget_Friend();
				}
				FriendTile friendTile = _activeFriendTiles[i];
				((RectTransform)friendTile.transform).anchoredPosition = new Vector2(0f, (float)(-a) * WidgetSize);
				friendTile.Init(_socialManager.Friends[a], FriendTile.Context.SocialEntityList, _actionSystem, _challengeController.ChallengePermissionState, _topSortingRoot);
				friendTile.SetChallengeEnabled(value: true);
				i++;
			}
			pos += (float)_socialManager.Friends.Count * WidgetSize;
		}
		for (; i < _activeFriendTiles.Count; i++)
		{
			_activeFriendTiles[i].gameObject.UpdateActive(active: false);
		}
	}

	private void UpdateIncomingChallengesList(Rect bounds, ref float pos)
	{
		((RectTransform)SectionIncomingChallengeRequest.transform).anchoredPosition = new Vector2(0f, 0f - pos);
		List<PVPChallengeData> incomingChallengeRequests = _challengeController.GetIncomingChallengeRequests();
		SectionIncomingChallengeRequest.SetCount(incomingChallengeRequests.Count);
		if (incomingChallengeRequests.Count > 0)
		{
			pos += HeaderSize;
		}
		int a = Mathf.FloorToInt((bounds.yMin - pos) / ChallengeWidgetSize);
		int a2 = Mathf.CeilToInt((bounds.yMax - pos) / ChallengeWidgetSize);
		a = Mathf.Max(a, 0);
		a2 = Mathf.Min(a2, incomingChallengeRequests.Count);
		int i = 0;
		if (SectionIncomingChallengeRequest.IsOpen)
		{
			for (; a < a2; a++)
			{
				if (i >= _activeChallengeRequestTiles.Count)
				{
					CreateWidget_IncomingChallengeRequest();
				}
				IncomingChallengeRequestTile incomingChallengeRequestTile = _activeChallengeRequestTiles[i];
				((RectTransform)incomingChallengeRequestTile.transform).anchoredPosition = new Vector2(0f, (float)(-a) * ChallengeWidgetSize);
				incomingChallengeRequestTile.Init(incomingChallengeRequests[a], _topSortingRoot, !_bisInDuelScene && AllowCreateOrJoin);
				i++;
			}
			pos += (float)incomingChallengeRequests.Count * ChallengeWidgetSize;
		}
		for (; i < _activeChallengeRequestTiles.Count; i++)
		{
			_activeChallengeRequestTiles[i].gameObject.UpdateActive(active: false);
		}
	}

	private void UpdateActiveChallenge(Rect bounds, ref float pos)
	{
		((RectTransform)ActiveChallengeAnchor.transform).anchoredPosition = new Vector2(0f, 0f - pos);
		PVPChallengeData activeCurrentChallengeData = _challengeController.GetActiveCurrentChallengeData();
		if (activeCurrentChallengeData != null)
		{
			pos += ChallengeWidgetSize;
			if (!_currentChallengeTile)
			{
				CreateWidget_ActiveChallenge();
			}
			((RectTransform)_currentChallengeTile.transform).anchoredPosition = new Vector2(0f, pos);
			_currentChallengeTile.Init(activeCurrentChallengeData);
		}
		ActiveChallengeAnchor.UpdateActive(activeCurrentChallengeData != null);
	}

	private void UpdateInvitesOutgoingList(Rect bounds, ref float pos)
	{
		((RectTransform)SectionOutgoingInvites.transform).anchoredPosition = new Vector2(0f, 0f - pos);
		SectionOutgoingInvites.SetCount(_socialManager.InvitesOutgoing.Count);
		if (_socialManager.InvitesOutgoing.Count > 0)
		{
			pos += HeaderSize;
		}
		int a = Mathf.FloorToInt((bounds.yMin - pos) / WidgetSize);
		int a2 = Mathf.CeilToInt((bounds.yMax - pos) / WidgetSize);
		a = Mathf.Max(a, 0);
		a2 = Mathf.Min(a2, _socialManager.InvitesOutgoing.Count);
		int i = 0;
		if (SectionOutgoingInvites.IsOpen)
		{
			for (; a < a2; a++)
			{
				if (i >= _activeInviteOutgoingTiles.Count)
				{
					CreateWidget_FriendInviteOutgoing();
				}
				InviteOutgoingTile inviteOutgoingTile = _activeInviteOutgoingTiles[i];
				((RectTransform)inviteOutgoingTile.transform).anchoredPosition = new Vector2(0f, (float)(-a) * WidgetSize);
				inviteOutgoingTile.Init(_socialManager.InvitesOutgoing[a]);
				i++;
			}
			pos += (float)_socialManager.InvitesOutgoing.Count * WidgetSize;
		}
		for (; i < _activeInviteOutgoingTiles.Count; i++)
		{
			_activeInviteOutgoingTiles[i].gameObject.UpdateActive(active: false);
		}
	}

	private void UpdateBlocksList(Rect bounds, ref float pos)
	{
		((RectTransform)SectionBlocks.transform).anchoredPosition = new Vector2(0f, 0f - pos);
		SectionBlocks.SetCount(_socialManager.Blocks.Count);
		if (_socialManager.Blocks.Count > 0)
		{
			pos += HeaderSize;
		}
		int a = Mathf.FloorToInt((bounds.yMin - pos) / WidgetSize);
		int a2 = Mathf.CeilToInt((bounds.yMax - pos) / WidgetSize);
		a = Mathf.Max(a, 0);
		a2 = Mathf.Min(a2, _socialManager.Blocks.Count);
		int i = 0;
		if (SectionBlocks.IsOpen)
		{
			for (; a < a2; a++)
			{
				if (i >= _activeBlockTiles.Count)
				{
					CreateWidget_Block();
				}
				BlockTile blockTile = _activeBlockTiles[i];
				((RectTransform)blockTile.transform).anchoredPosition = new Vector2(0f, (float)(-a) * WidgetSize);
				blockTile.Init(_socialManager.Blocks[a]);
				i++;
			}
			pos += (float)_socialManager.Blocks.Count * WidgetSize;
		}
		for (; i < _activeBlockTiles.Count; i++)
		{
			_activeBlockTiles[i].gameObject.UpdateActive(active: false);
		}
	}

	private void CreateWidget_Friend()
	{
		FriendTile friendTile = UnityEngine.Object.Instantiate(_prefabFriendTile, SectionFriends.transform);
		_activeFriendTiles.Add(friendTile);
		friendTile.Callback_OpenChat = _socialUI.ShowChatWindow;
		friendTile.Callback_RemoveFriend = delegate(SocialEntity friend)
		{
			string title = new MTGALocalizedString
			{
				Key = "DuelScene/ClientPrompt/Are_You_Sure_Title"
			};
			string message = new MTGALocalizedString
			{
				Key = "Social/Friends/Prompt/RemoveFriend/Body",
				Parameters = new Dictionary<string, string> { { "displayName", friend.DisplayName } }
			};
			SystemMessageManager.ShowSystemMessage(title, message, showCancel: true, delegate
			{
				_socialManager.RemoveFriend(friend);
			});
		};
		friendTile.Callback_BlockFriend = delegate(SocialEntity friend)
		{
			string title = new MTGALocalizedString
			{
				Key = "DuelScene/ClientPrompt/Are_You_Sure_Title"
			};
			string message = new MTGALocalizedString
			{
				Key = "Social/Friends/Prompt/ConfirmBlock/Body",
				Parameters = new Dictionary<string, string> { { "displayName", friend.DisplayName } }
			};
			SystemMessageManager.ShowSystemMessage(title, message, showCancel: true, delegate
			{
				_socialManager.AddBlock(friend);
			});
		};
		_socialUI.SubscribeToFriendChallenge(friendTile);
	}

	private void CreateWidget_Block()
	{
		BlockTile blockTile = UnityEngine.Object.Instantiate(_prefabBlockTile, SectionBlocks.transform);
		_activeBlockTiles.Add(blockTile);
		blockTile.Callback_RemoveBlock = _socialManager.RemoveBlock;
	}

	private void CreateWidget_FriendInviteIncoming()
	{
		InviteIncomingTile inviteIncomingTile = UnityEngine.Object.Instantiate(_prefabInviteIncomingTile, SectionIncomingInvites.transform);
		_activeInviteIncomingTiles.Add(inviteIncomingTile);
		inviteIncomingTile.Callback_Accept = delegate(Invite inv)
		{
			_socialManager.AcceptFriendInviteIncoming(inv.PotentialFriend);
		};
		inviteIncomingTile.Callback_Reject = delegate(Invite inv)
		{
			_socialManager.DeclineFriendInviteIncoming(inv);
		};
		inviteIncomingTile.Callback_Block = delegate(Invite inv)
		{
			MTGALocalizedString obj = new MTGALocalizedString
			{
				Key = "DuelScene/ClientPrompt/Are_You_Sure_Title"
			};
			SystemMessageManager.ShowSystemMessage(message: new MTGALocalizedString
			{
				Key = "Social/Friends/Prompt/ConfirmBlock/Body",
				Parameters = new Dictionary<string, string> { 
				{
					"displayName",
					inv.PotentialFriend.DisplayName
				} }
			}, title: obj, showCancel: true, onOK: delegate
			{
				_socialManager.AddBlock(inv.PotentialFriend);
			});
		};
	}

	private void CreateWidget_FriendInviteOutgoing()
	{
		InviteOutgoingTile inviteOutgoingTile = UnityEngine.Object.Instantiate(_prefabInviteOutgoingTile, SectionOutgoingInvites.transform);
		_activeInviteOutgoingTiles.Add(inviteOutgoingTile);
		inviteOutgoingTile.Callback_Reject = delegate(Invite inv)
		{
			_socialManager.RevokeFriendInviteOutgoing(inv);
		};
	}

	private void CreateWidget_ActiveChallenge()
	{
		CurrentChallengeTile currentChallengeTile = UnityEngine.Object.Instantiate(_prefabCurrentChallengeTile, ActiveChallengeAnchor.transform);
		_currentChallengeTile = currentChallengeTile;
		_currentChallengeTile.OnOpenChallengeScreen = OnOpenChallengeScreenButtonClicked;
	}

	private void CreateWidget_IncomingChallengeRequest()
	{
		IncomingChallengeRequestTile incomingChallengeRequestTile = UnityEngine.Object.Instantiate(_prefabChallengeRequestTile, SectionIncomingChallengeRequest.transform);
		_activeChallengeRequestTiles.Add(incomingChallengeRequestTile);
		incomingChallengeRequestTile.Callback_Accept = delegate(Guid challengeId)
		{
			_challengeController.AcceptChallengeInvite(challengeId).ThenOnMainThreadIfSuccess(delegate(PVPChallengeData challenge)
			{
				_socialUI.OpenPlayBlade(challenge.ChallengeId);
			});
		};
		incomingChallengeRequestTile.Callback_Reject = delegate(Guid challengeId)
		{
			_challengeController.RejectChallengeInvite(challengeId);
		};
		incomingChallengeRequestTile.Callback_AddFriend = delegate(string challengeSenderFullName)
		{
			_socialManager.SubmitFriendInviteOutgoing(challengeSenderFullName);
		};
		incomingChallengeRequestTile.Callback_Block = delegate(string challengSenderFullName)
		{
			_socialManager.BlockByDisplayName(challengSenderFullName);
		};
	}
}
