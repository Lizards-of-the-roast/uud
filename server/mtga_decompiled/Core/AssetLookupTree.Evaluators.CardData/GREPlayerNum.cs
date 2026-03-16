using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class GREPlayerNum : EvaluatorBase_List<global::GREPlayerNum>
{
	public override bool Execute(IBlackboard bb)
	{
		global::GREPlayerNum inValue = ((bb.GREPlayerNum != global::GREPlayerNum.Invalid) ? bb.GREPlayerNum : (bb.Player?.ClientPlayerEnum ?? global::GREPlayerNum.Invalid));
		return EvaluatorBase_List<global::GREPlayerNum>.GetResult(ExpectedValues, Operation, ExpectedResult, inValue);
	}
}
