using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Indirectors.ManaMovement;

public class ManaMovement_SinkPlayer : IIndirector
{
	private MtgPlayer _playerCache;

	public void SetCache(IBlackboard bb)
	{
		_playerCache = bb.Player;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.Player = _playerCache;
		_playerCache = null;
	}

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.ManaMovement.IsValid && bb.ManaMovement.Sink is MtgPlayer player)
		{
			bb.Player = player;
			yield return bb;
		}
	}
}
