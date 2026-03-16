namespace Wizards.Mtga.AssetLookupTree.Watcher.AltRequestEntry;

public enum BucketNodeResult
{
	Success,
	NoResult,
	NoExtractor,
	ExtractorFailed,
	NoBucketForValue,
	NullChildInBucket
}
