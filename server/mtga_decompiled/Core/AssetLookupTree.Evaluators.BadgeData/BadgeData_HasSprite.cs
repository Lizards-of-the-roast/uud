using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.BadgeData;

public class BadgeData_HasSprite : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.BadgeData != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.BadgeData != null && bb.BadgeData.SpriteRef != null && !string.IsNullOrEmpty(bb.BadgeData.SpriteRef.RelativePath));
		}
		return false;
	}
}
