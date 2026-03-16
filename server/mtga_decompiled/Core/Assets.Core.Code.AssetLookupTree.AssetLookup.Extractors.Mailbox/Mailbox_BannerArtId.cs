using AssetLookupTree.Blackboard;
using AssetLookupTree.Extractors;

namespace Assets.Core.Code.AssetLookupTree.AssetLookup.Extractors.Mailbox;

public class Mailbox_BannerArtId : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = bb.LetterBannerArtId;
		return !string.IsNullOrEmpty(value);
	}
}
