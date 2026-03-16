using AssetLookupTree.Blackboard;
using Wotc.Mtga.Cards.Database;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_SkinCodeOrMetaTag : EvaluatorBase_String
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData == null)
		{
			return !ExpectedResult;
		}
		string text = ((bb.CardData.Instance == null) ? string.Empty : bb.CardData.Instance.SkinCode);
		if (string.IsNullOrEmpty(text))
		{
			foreach (MetaDataTag tag in bb.CardData.Printing.Record.Tags)
			{
				if (tag.ToString().StartsWith("Style_"))
				{
					text = tag.ToString().Split('_')[1].Trim();
					break;
				}
			}
		}
		return EvaluatorBase_String.GetResult(ExpectedValue, Operation, ExpectedResult, text);
	}
}
