using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.Ability;

namespace Wotc.Mtga.Hangers;

public class IconPathProvider_IconId : IPathProvider<string>
{
	private readonly AssetLookupTree<SpecialIcon> _tree;

	private readonly IBlackboard _blackboard;

	public IconPathProvider_IconId(AssetLookupTree<SpecialIcon> iconTree, IBlackboard blackboard)
	{
		_tree = iconTree ?? new AssetLookupTree<SpecialIcon>();
		_blackboard = blackboard ?? new Blackboard();
	}

	public string GetPath(string iconId)
	{
		_blackboard.Clear();
		_blackboard.LookupString = iconId;
		string result = null;
		SpecialIcon payload = _tree.GetPayload(_blackboard);
		if (payload != null)
		{
			result = payload.Reference.RelativePath;
		}
		return result;
	}
}
