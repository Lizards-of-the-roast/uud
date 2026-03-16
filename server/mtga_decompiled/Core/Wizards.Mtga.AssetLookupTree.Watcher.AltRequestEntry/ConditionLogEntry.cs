using AssetLookupTree;
using AssetLookupTree.Nodes;

namespace Wizards.Mtga.AssetLookupTree.Watcher.AltRequestEntry;

public class ConditionLogEntry : IAltRequestLogEntry
{
	public readonly ConditionNodeResult Result;

	public static ConditionLogEntry CreateEntry<T>(ConditionNode<T> node, ConditionNodeResult result) where T : class, IPayload
	{
		return new ConditionLogEntry(node, result);
	}

	public ConditionLogEntry(INode node, ConditionNodeResult result)
	{
		Node = node;
		Result = result;
		ResultString = Result.ToString();
		HighlightType = result switch
		{
			ConditionNodeResult.Success => LogHighlightType.Passed, 
			ConditionNodeResult.NoEvaluator => LogHighlightType.Error, 
			ConditionNodeResult.NoChild => LogHighlightType.Error, 
			_ => LogHighlightType.Failed, 
		};
	}
}
