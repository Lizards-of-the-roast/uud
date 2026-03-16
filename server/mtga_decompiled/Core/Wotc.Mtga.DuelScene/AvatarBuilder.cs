using AssetLookupTree;
using AssetLookupTree.Payloads.Avatar;
using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene;

public class AvatarBuilder : IAvatarBuilder
{
	private readonly ISignalDispatch<PlayerCreatedSignalArgs> _playerCreatedEvent;

	private readonly ISignalDispatch<PlayerDeletedSignalArgs> _playerDeletedEvent;

	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly Transform _root;

	public AvatarBuilder(ISignalDispatch<PlayerCreatedSignalArgs> playerCreatedEvent, ISignalDispatch<PlayerDeletedSignalArgs> playerDeletedEvent, AssetLookupSystem assetLookupSystem, Transform root)
	{
		_playerCreatedEvent = playerCreatedEvent;
		_playerDeletedEvent = playerDeletedEvent;
		_assetLookupSystem = assetLookupSystem;
		_root = root;
	}

	public DuelScene_AvatarView Create(MtgPlayer player)
	{
		DuelScene_AvatarView duelScene_AvatarView = CreateAvatar(player.ClientPlayerEnum);
		if (duelScene_AvatarView == null)
		{
			return null;
		}
		duelScene_AvatarView.name = $"Player #{player.InstanceId}";
		_playerCreatedEvent.Dispatch(new PlayerCreatedSignalArgs(this, duelScene_AvatarView));
		return duelScene_AvatarView;
	}

	private DuelScene_AvatarView CreateAvatar(GREPlayerNum playerType)
	{
		_assetLookupSystem.Blackboard.Clear();
		AvatarPayload avatarPayload = null;
		avatarPayload = ((playerType != GREPlayerNum.LocalPlayer && playerType != GREPlayerNum.Teammate) ? ((AvatarPayload)_assetLookupSystem.TreeLoader.LoadTree<OpponentAvatar>().GetPayload(_assetLookupSystem.Blackboard)) : ((AvatarPayload)_assetLookupSystem.TreeLoader.LoadTree<LocalAvatar>().GetPayload(_assetLookupSystem.Blackboard)));
		if (avatarPayload == null)
		{
			Debug.LogError($"No avatar could be loaded for GREPlayerNum: {playerType}");
			return null;
		}
		return AssetLoader.Instantiate<DuelScene_AvatarView>(avatarPayload.Prefab.RelativePath, _root);
	}

	public bool Destroy(DuelScene_AvatarView avatar)
	{
		_playerDeletedEvent.Dispatch(new PlayerDeletedSignalArgs(this, avatar));
		Object.Destroy(avatar.gameObject);
		return true;
	}
}
