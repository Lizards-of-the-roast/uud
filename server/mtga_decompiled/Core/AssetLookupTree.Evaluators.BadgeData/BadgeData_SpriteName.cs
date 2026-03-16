using System.IO;
using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.BadgeData;

public class BadgeData_SpriteName : EvaluatorBase_String
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.BadgeData?.SpriteRef?.RelativePath != null)
		{
			return EvaluatorBase_String.GetResult(ExpectedValue, Operation, ExpectedResult, Path.GetFileNameWithoutExtension(bb.BadgeData.SpriteRef.RelativePath));
		}
		return false;
	}
}
