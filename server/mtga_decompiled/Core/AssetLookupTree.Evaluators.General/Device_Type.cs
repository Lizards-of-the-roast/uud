using AssetLookupTree.Blackboard;
using UnityEngine;

namespace AssetLookupTree.Evaluators.General;

public class Device_Type : EvaluatorBase_List<DeviceType>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<DeviceType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.DeviceType);
	}
}
