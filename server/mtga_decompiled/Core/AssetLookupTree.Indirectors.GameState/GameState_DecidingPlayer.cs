using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Indirectors.GameState;

public class GameState_DecidingPlayer : IIndirector
{
	private MtgPlayer _cachePlayer;

	private GREPlayerNum _cachePlayerNum;

	public void SetCache(IBlackboard bb)
	{
		_cachePlayer = bb.Player;
		_cachePlayerNum = bb.GREPlayerNum;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.Player = _cachePlayer;
		bb.GREPlayerNum = _cachePlayerNum;
		_cachePlayer = null;
		_cachePlayerNum = GREPlayerNum.Invalid;
	}

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.GameState?.DecidingPlayer != null)
		{
			bb.Player = bb.GameState.DecidingPlayer;
			bb.GREPlayerNum = bb.GameState.DecidingPlayer.ClientPlayerEnum;
			yield return bb;
		}
	}
}
