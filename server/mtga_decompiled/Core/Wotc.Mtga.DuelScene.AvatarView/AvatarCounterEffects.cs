using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Player.Counters;
using GreClient.Rules;
using Wotc.Mtga.DuelScene.VFX;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.AvatarView;

public class AvatarCounterEffects
{
	private CounterTypeDiffOutput _counterDiff = CounterTypeDiffOutput.empty;

	private readonly Func<MtgPlayer> _getPlayer;

	private readonly GameManager _gameManager;

	private readonly IVfxProvider _vfxProvider;

	public AvatarCounterEffects(GameManager gameManager, Func<MtgPlayer> getPlayer)
	{
		_gameManager = gameManager;
		_vfxProvider = gameManager.VfxProvider;
		_getPlayer = getPlayer;
	}

	public void UpdateCounterFX(Dictionary<CounterType, int> existingCounters, Dictionary<CounterType, int> newCounters)
	{
		CounterTypeDiff.GetOutput(ref _counterDiff, new CounterTypeDiffInput(existingCounters, newCounters));
		IncrementFX(_counterDiff.Incremented);
	}

	private void IncrementFX(HashSet<CounterType> incremented)
	{
		MtgPlayer mtgPlayer = _getPlayer();
		AssetLookupSystem assetLookupSystem = _gameManager.AssetLookupSystem;
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.Player = mtgPlayer;
		assetLookupSystem.Blackboard.GREPlayerNum = mtgPlayer.ClientPlayerEnum;
		foreach (CounterType item in incremented)
		{
			assetLookupSystem.Blackboard.CounterType = item;
			if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<IncrementVfx> loadedTree))
			{
				IncrementVfx payload = loadedTree.GetPayload(assetLookupSystem.Blackboard);
				if (payload != null && _gameManager.ViewManager.TryGetAvatarById(mtgPlayer.InstanceId, out var avatar))
				{
					_vfxProvider.PlayVFX(payload.VfxData, null, mtgPlayer, avatar.EffectsRoot);
				}
			}
		}
	}
}
