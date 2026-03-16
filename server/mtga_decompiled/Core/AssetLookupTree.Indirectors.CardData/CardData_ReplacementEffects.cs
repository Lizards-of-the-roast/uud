using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Indirectors.CardData;

public class CardData_ReplacementEffects : IIndirector
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
		if (bb.CardData?.Instance == null)
		{
			yield break;
		}
		ReplacementEffectData cacheEffect = bb.ReplacementEffectData;
		foreach (ReplacementEffectData replacementEffect in bb.CardData.Instance.ReplacementEffects)
		{
			bb.ReplacementEffectData = replacementEffect;
			yield return bb;
		}
		bb.ReplacementEffectData = cacheEffect;
	}
}
