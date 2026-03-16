using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;

namespace AssetLookupTree.Indirectors.GameState;

public class GameState_TopCardOnStack : IIndirector
{
	private AbilityPrintingData _cachedAbilityPrinting;

	public void SetCache(IBlackboard bb)
	{
		_cachedAbilityPrinting = bb.Ability;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.Ability = _cachedAbilityPrinting;
		_cachedAbilityPrinting = null;
	}

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.GameState != null && bb.CardDataProvider != null)
		{
			MtgCardInstance topCardOnStack = bb.GameState.GetTopCardOnStack();
			if (topCardOnStack != null)
			{
				bb.SetCardDataExtensive(topCardOnStack);
				yield return bb;
			}
		}
	}
}
