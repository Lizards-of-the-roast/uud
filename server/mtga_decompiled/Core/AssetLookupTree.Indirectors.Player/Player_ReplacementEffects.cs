using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Indirectors.Player;

public class Player_ReplacementEffects : IIndirector
{
	private ReplacementEffectData _cache;

	public void SetCache(IBlackboard bb)
	{
		_cache = bb.ReplacementEffectData;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.ReplacementEffectData = _cache;
		_cache = default(ReplacementEffectData);
	}

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.Player == null)
		{
			yield break;
		}
		ReplacementEffectData cacheAbility = bb.ReplacementEffectData;
		foreach (ReplacementEffectData replacementEffect in bb.Player.ReplacementEffects)
		{
			bb.ReplacementEffectData = replacementEffect;
			yield return bb;
		}
		bb.ReplacementEffectData = cacheAbility;
	}
}
