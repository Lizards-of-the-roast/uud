using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.Card;
using GreClient.CardData;

namespace Wotc.Mtga.Hangers;

public class PhyrexianManaIconPathProvider_ALT : IPathProvider<ICardDataAdapter>
{
	private readonly AssetLookupTree<PhyrexianManaIcon> _tree;

	private readonly IBlackboard _blackboard;

	public PhyrexianManaIconPathProvider_ALT(AssetLookupTree<PhyrexianManaIcon> tree, IBlackboard blackboard)
	{
		_tree = tree ?? new AssetLookupTree<PhyrexianManaIcon>();
		_blackboard = blackboard ?? new Blackboard();
	}

	public string GetPath(ICardDataAdapter model)
	{
		string result = string.Empty;
		if (model != null)
		{
			_blackboard.Clear();
			_blackboard.SetCardDataExtensive(model);
			PhyrexianManaIcon payload = _tree.GetPayload(_blackboard);
			result = ((payload != null) ? payload.SpriteRef.RelativePath : string.Empty);
			_blackboard.Clear();
		}
		return result;
	}
}
