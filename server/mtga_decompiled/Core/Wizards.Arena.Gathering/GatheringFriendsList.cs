using System;
using System.Collections.Generic;
using AK.Wwise;
using MTGA.Social;
using UnityEngine;
using Wizards.GeneralUtilities.AdvancedButton;
using Wizards.GeneralUtilities.ObjectCommunication;
using Wizards.Mtga;
using Wotc.Mtga.Extensions;

namespace Wizards.Arena.Gathering;

public class GatheringFriendsList : MonoBehaviour
{
	[SerializeField]
	private AdvancedButton _addFriendButton;

	[Header("Friends List Section Headers")]
	[SerializeField]
	private SocialEntityListHeader _sectionFriends;

	[SerializeField]
	private SocialEntityListHeader _sectionIncomingInvites;

	[SerializeField]
	private SocialEntityListHeader _sectionOutgoingInvites;

	[SerializeField]
	private SocialEntityListHeader _sectionBlocks;

	[Header("Prefab & Parent References")]
	[SerializeField]
	private GameObject _prefabFriendTile;

	[SerializeField]
	private GameObject _prefabInviteIncomingTile;

	[SerializeField]
	private GameObject _prefabInviteOutgoingTile;

	[SerializeField]
	private GameObject _prefabBlockTile;

	[SerializeField]
	private GameObject _addFriendInvitePanelPrefab;

	[SerializeField]
	private BeaconIdentifier _addFriendInvitePanelParent;

	[Header("Audio References")]
	[SerializeField]
	private AK.Wwise.Event _userStatusButtonClickAudioEvent;

	[SerializeField]
	private AK.Wwise.Event _userStatusButtonRolloverAudioEvent;

	[SerializeField]
	private AK.Wwise.Event _addFriendButtonClickedAudioEvent;

	[SerializeField]
	private AK.Wwise.Event _addFriendButtonRolloverAudioEvent;

	private ISocialManager _socialManager;

	private readonly List<GatheringFriendTile> _friendItems = new List<GatheringFriendTile>();

	private readonly List<GatheringInviteIncomingTile> _incomingItems = new List<GatheringInviteIncomingTile>();

	private readonly List<GatheringInviteOutgoingTile> _outgoingItems = new List<GatheringInviteOutgoingTile>();

	private readonly List<GatheringBlockTile> _blockedItems = new List<GatheringBlockTile>();

	private void Awake()
	{
		_socialManager = Pantry.Get<ISocialManager>();
		_sectionFriends.IsOpen = false;
		_sectionIncomingInvites.IsOpen = false;
		_sectionOutgoingInvites.IsOpen = false;
		_sectionBlocks.IsOpen = false;
	}

	private void Start()
	{
		BindListeners();
		UpdateSocialEntityList();
	}

	private void StatusDropdownMouseEnter()
	{
		AudioManager.PlayAudio(_userStatusButtonRolloverAudioEvent.Name, base.gameObject);
	}

	private void OnFriendsUpdated()
	{
		UpdateFriends(_sectionFriends.IsOpen);
	}

	private void OnIncomingUpdated()
	{
		UpdateIncomingInvites(_sectionIncomingInvites.IsOpen);
	}

	private void OnOutgoingUpdated()
	{
		UpdateOutgoingInvites(_sectionOutgoingInvites.IsOpen);
	}

	private void OnBlockedUpdated()
	{
		UpdateBlocks(_sectionBlocks.IsOpen);
	}

	private void UpdateSocialEntityList()
	{
		UpdateFriends(_sectionFriends.IsOpen);
		UpdateIncomingInvites(_sectionIncomingInvites.IsOpen);
		UpdateOutgoingInvites(_sectionOutgoingInvites.IsOpen);
		UpdateBlocks(_sectionBlocks.IsOpen);
	}

	private void UpdateFriends(bool isOpen)
	{
		_friendItems.Clear();
		_sectionFriends.ContentParent.transform.DestroyChildren();
		_sectionFriends.SetCount(_socialManager.Friends.Count);
		if (!isOpen)
		{
			return;
		}
		foreach (SocialEntity friend in _socialManager.Friends)
		{
			GatheringFriendTile component = UnityEngine.Object.Instantiate(_prefabFriendTile, _sectionFriends.ContentParent.transform).GetComponent<GatheringFriendTile>();
			component.Init(friend);
			_friendItems.Add(component);
		}
	}

	private void UpdateIncomingInvites(bool isOpen)
	{
		_incomingItems.Clear();
		_sectionIncomingInvites.ContentParent.transform.DestroyChildren();
		_sectionIncomingInvites.SetCount(_socialManager.InvitesIncoming.Count);
		if (!isOpen)
		{
			return;
		}
		foreach (Invite item in _socialManager.InvitesIncoming)
		{
			GatheringInviteIncomingTile component = UnityEngine.Object.Instantiate(_prefabInviteIncomingTile, _sectionIncomingInvites.ContentParent.transform).GetComponent<GatheringInviteIncomingTile>();
			component.Init(item);
			_incomingItems.Add(component);
		}
	}

	private void UpdateOutgoingInvites(bool isOpen)
	{
		_outgoingItems.Clear();
		_sectionOutgoingInvites.ContentParent.transform.DestroyChildren();
		_sectionOutgoingInvites.SetCount(_socialManager.InvitesOutgoing.Count);
		if (!isOpen)
		{
			return;
		}
		foreach (Invite item in _socialManager.InvitesOutgoing)
		{
			GatheringInviteOutgoingTile component = UnityEngine.Object.Instantiate(_prefabInviteOutgoingTile, _sectionOutgoingInvites.ContentParent.transform).GetComponent<GatheringInviteOutgoingTile>();
			component.Init(item);
			_outgoingItems.Add(component);
		}
	}

	private void UpdateBlocks(bool isOpen)
	{
		_blockedItems.Clear();
		_sectionBlocks.ContentParent.transform.DestroyChildren();
		_sectionBlocks.SetCount(_socialManager.Blocks.Count);
		if (!isOpen)
		{
			return;
		}
		foreach (Block block in _socialManager.Blocks)
		{
			GatheringBlockTile component = UnityEngine.Object.Instantiate(_prefabBlockTile, _sectionBlocks.ContentParent.transform).GetComponent<GatheringBlockTile>();
			component.Init(block);
			_blockedItems.Add(component);
		}
	}

	private void OnLocalPresenceStatusChanged(PresenceStatus oldStatus, PresenceStatus newStatus)
	{
		if (newStatus == PresenceStatus.Offline)
		{
			_sectionFriends.gameObject.SetActive(value: false);
			_sectionIncomingInvites.gameObject.SetActive(value: false);
			_sectionOutgoingInvites.gameObject.SetActive(value: false);
			_sectionBlocks.gameObject.SetActive(value: false);
		}
		else
		{
			_sectionFriends.gameObject.SetActive(_friendItems.Count > 0);
			_sectionIncomingInvites.gameObject.SetActive(_incomingItems.Count > 0);
			_sectionOutgoingInvites.gameObject.SetActive(_outgoingItems.Count > 0);
			_sectionBlocks.gameObject.SetActive(_blockedItems.Count > 0);
		}
	}

	private void BindListeners()
	{
		_socialManager.FriendsChanged += OnFriendsUpdated;
		_socialManager.InvitesIncomingChanged += OnIncomingUpdated;
		_socialManager.InvitesOutgoingChanged += OnOutgoingUpdated;
		_socialManager.BlocksChanged += OnBlockedUpdated;
		SocialEntityListHeader sectionFriends = _sectionFriends;
		sectionFriends.IsOpenChanged = (Action<bool>)Delegate.Combine(sectionFriends.IsOpenChanged, new Action<bool>(UpdateFriends));
		SocialEntityListHeader sectionIncomingInvites = _sectionIncomingInvites;
		sectionIncomingInvites.IsOpenChanged = (Action<bool>)Delegate.Combine(sectionIncomingInvites.IsOpenChanged, new Action<bool>(UpdateIncomingInvites));
		SocialEntityListHeader sectionOutgoingInvites = _sectionOutgoingInvites;
		sectionOutgoingInvites.IsOpenChanged = (Action<bool>)Delegate.Combine(sectionOutgoingInvites.IsOpenChanged, new Action<bool>(UpdateOutgoingInvites));
		SocialEntityListHeader sectionBlocks = _sectionBlocks;
		sectionBlocks.IsOpenChanged = (Action<bool>)Delegate.Combine(sectionBlocks.IsOpenChanged, new Action<bool>(UpdateBlocks));
		_socialManager.LocalPresenceStatusChanged += OnLocalPresenceStatusChanged;
		_addFriendButton.onClick.AddListener(delegate
		{
			AudioManager.PlayAudio(_addFriendButtonClickedAudioEvent.Name, base.gameObject);
			UnityEngine.Object.Instantiate(_addFriendInvitePanelPrefab, (Transform)_addFriendInvitePanelParent.GetBeaconObject()[0]);
		});
		_addFriendButton.OnMouseHoverEnterEvent.AddListener(delegate
		{
			AudioManager.PlayAudio(_addFriendButtonRolloverAudioEvent.Name, base.gameObject);
		});
	}

	private void OnDestroy()
	{
		_socialManager.FriendsChanged -= OnFriendsUpdated;
		_socialManager.InvitesIncomingChanged -= OnIncomingUpdated;
		_socialManager.InvitesOutgoingChanged -= OnOutgoingUpdated;
		_socialManager.BlocksChanged -= OnBlockedUpdated;
		SocialEntityListHeader sectionFriends = _sectionFriends;
		sectionFriends.IsOpenChanged = (Action<bool>)Delegate.Remove(sectionFriends.IsOpenChanged, new Action<bool>(UpdateFriends));
		SocialEntityListHeader sectionIncomingInvites = _sectionIncomingInvites;
		sectionIncomingInvites.IsOpenChanged = (Action<bool>)Delegate.Remove(sectionIncomingInvites.IsOpenChanged, new Action<bool>(UpdateIncomingInvites));
		SocialEntityListHeader sectionOutgoingInvites = _sectionOutgoingInvites;
		sectionOutgoingInvites.IsOpenChanged = (Action<bool>)Delegate.Remove(sectionOutgoingInvites.IsOpenChanged, new Action<bool>(UpdateOutgoingInvites));
		SocialEntityListHeader sectionBlocks = _sectionBlocks;
		sectionBlocks.IsOpenChanged = (Action<bool>)Delegate.Remove(sectionBlocks.IsOpenChanged, new Action<bool>(UpdateBlocks));
		_socialManager.LocalPresenceStatusChanged -= OnLocalPresenceStatusChanged;
		_addFriendButton.onClick.RemoveAllListeners();
		_addFriendButton.OnMouseHoverEnterEvent.RemoveAllListeners();
	}
}
