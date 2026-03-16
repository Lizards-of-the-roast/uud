using AssetLookupTree;
using AssetLookupTree.Nodes;

namespace Wizards.Mtga.AssetLookupTree.Watcher.AltRequestEntry;

public class PriorityLogEntry : IAltRequestLogEntry
{
	public readonly PriorityNodeResult Result;

	public static PriorityLogEntry CreateEntry<T>(PriorityNode<T> node, PriorityNodeResult result) where T : class, IPayload
	{
		return new PriorityLogEntry(node, result);
	}

	public PriorityLogEntry(INode node, PriorityNodeResult result)
	{
		Node = node;
		Result = result;
		ResultString = Result.ToString();
		HighlightType = result switch
		{
			PriorityNodeResult.Success => LogHighlightType.Passed, 
			PriorityNodeResult.NoResult => LogHighlightType.Failed, 
			_ => LogHighlightType.Error, 
		};
	}
}
