using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Cosmetics;

public class Cosmetic_AccessoryMod : EvaluatorBase_String
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_String.GetResult(ExpectedValue, Operation, ExpectedResult, bb.CosmeticAccessoryMod);
	}
}
