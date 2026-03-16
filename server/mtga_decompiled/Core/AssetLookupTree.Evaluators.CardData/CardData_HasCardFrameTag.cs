using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using Wotc.Mtga.Cards.Database;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_HasCardFrameTag : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData == null || bb.CardData.Tags == null)
		{
			return !ExpectedResult;
		}
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, GetMetaTagNames(bb.CardData.Tags));
		static bool GetMetaTagNames(IReadOnlyCollection<MetaDataTag> tags)
		{
			foreach (MetaDataTag tag in tags)
			{
				if (tag.ToString().StartsWith("Card_", ignoreCase: true, null) && tag != MetaDataTag.Card_SilverBorder)
				{
					return true;
				}
			}
			return false;
		}
	}
}
