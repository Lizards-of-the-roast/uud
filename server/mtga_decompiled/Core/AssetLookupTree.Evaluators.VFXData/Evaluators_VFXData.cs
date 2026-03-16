using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.VFXData;

internal class Evaluators_VFXData
{
	public class VFXData_LutVfxType : EvaluatorBase_List<LUTVFXType>
	{
		public override bool Execute(IBlackboard bb)
		{
			return EvaluatorBase_List<LUTVFXType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.LutVfxType);
		}
	}
}
