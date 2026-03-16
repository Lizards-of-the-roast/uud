using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.CardData;

public class DesignationTypeAndValuesParent : DesignationTypeAndValues
{
	public override bool Execute(IBlackboard bb)
	{
		IEnumerable<DesignationData> enumerable = bb.CardData?.Instance?.Parent?.Designations.FindAll((DesignationData x) => x.Type == DesignationType);
		if (enumerable == null)
		{
			return !ExpectedResult;
		}
		return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, GetValues(enumerable), MinCount, MaxCount);
	}
}
