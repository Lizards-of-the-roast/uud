using AssetLookupTree;
using AssetLookupTree.Nodes;

namespace Wizards.Mtga.AssetLookupTree.Watcher.AltRequestEntry;

public class IndirectionLogEntry : IAltRequestLogEntry
{
	public readonly IndirectionNodeResult Result;

	public static IndirectionLogEntry CreateEntry<T>(IndirectionNode<T> node, IndirectionNodeResult result) where T : class, IPayload
	{
		return new IndirectionLogEntry(node, result);
	}

	public IndirectionLogEntry(INode node, IndirectionNodeResult result)
	{
		Node = node;
		Result = result;
		ResultString = Result.ToString();
		HighlightType = result switch
		{
			IndirectionNodeResult.Success => LogHighlightType.Passed, 
			IndirectionNodeResult.NoResult => LogHighlightType.Failed, 
			_ => LogHighlightType.Error, 
		};
	}
}
