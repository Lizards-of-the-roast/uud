using AssetLookupTree;
using AssetLookupTree.Nodes;

namespace Wizards.Mtga.AssetLookupTree.Watcher.AltRequestEntry;

public class BucketLogEntry : IAltRequestLogEntry
{
	public readonly BucketNodeResult Result;

	public static BucketLogEntry CreateEntry<T, U>(BucketNode<T, U> node, BucketNodeResult result) where T : class, IPayload
	{
		return new BucketLogEntry(node, result);
	}

	public BucketLogEntry(INode node, BucketNodeResult result)
	{
		Node = node;
		Result = result;
		ResultString = Result.ToString();
		HighlightType = result switch
		{
			BucketNodeResult.Success => LogHighlightType.Passed, 
			BucketNodeResult.NoExtractor => LogHighlightType.Error, 
			BucketNodeResult.NullChildInBucket => LogHighlightType.Error, 
			_ => LogHighlightType.Failed, 
		};
	}
}
