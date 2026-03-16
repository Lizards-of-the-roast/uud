using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.Vorthos;

namespace AssetLookupTree.Evaluators.Vorthos;

public class Vorthos_TimeOfDay : EvaluatorBase_List<TimeOfDay>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<TimeOfDay>.GetResult(ExpectedValues, Operation, ExpectedResult, ItsVorthosTime.Get(bb.DateTimeUtc));
	}
}
