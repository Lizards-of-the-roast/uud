using AssetLookupTree;
using AssetLookupTree.Nodes;

namespace Wizards.Mtga.AssetLookupTree.Watcher.AltRequestEntry;

public class ValueLogEntry : IAltRequestLogEntry
{
	public readonly ValueNodeResult Result;

	public static ValueLogEntry CreateEntry<T>(ValueNode<T> node, ValueNodeResult result) where T : class, IPayload
	{
		return new ValueLogEntry(node, result);
	}

	public ValueLogEntry(INode node, ValueNodeResult result)
	{
		Node = node;
		Result = result;
		ResultString = Result.ToString();
		HighlightType = result switch
		{
			ValueNodeResult.Success => LogHighlightType.Success, 
			ValueNodeResult.Failed => LogHighlightType.Failed, 
			_ => LogHighlightType.None, 
		};
	}
}
