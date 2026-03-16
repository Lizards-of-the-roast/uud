using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.Ability;
using GreClient.CardData;

namespace Wotc.Mtga.Hangers;

public class IconPathProvider_ALT : IPathProvider<AbilityPrintingData>
{
	private readonly AssetLookupTree<BadgeEntry> _tree;

	private readonly IBlackboard _blackboard;

	public IconPathProvider_ALT(AssetLookupTree<BadgeEntry> badgeEntryTree, IBlackboard blackboard)
	{
		_tree = badgeEntryTree ?? new AssetLookupTree<BadgeEntry>();
		_blackboard = blackboard ?? new Blackboard();
	}

	public string GetPath(AbilityPrintingData ability)
	{
		_blackboard.Clear();
		_blackboard.Ability = ability;
		BadgeEntry payload = _tree.GetPayload(_blackboard);
		if (payload == null)
		{
			return string.Empty;
		}
		return payload.Data.SpriteRef.RelativePath;
	}
}
