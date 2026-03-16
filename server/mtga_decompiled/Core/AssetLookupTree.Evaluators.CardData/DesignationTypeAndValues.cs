using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class DesignationTypeAndValues : EvaluatorBase_List<int>
{
	public Designation DesignationType;

	public override bool Execute(IBlackboard bb)
	{
		IEnumerable<DesignationData> enumerable = bb.CardData?.Instance?.Designations.FindAll((DesignationData x) => x.Type == DesignationType);
		if (enumerable == null)
		{
			return !ExpectedResult;
		}
		return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, GetValues(enumerable), MinCount, MaxCount);
	}

	protected IEnumerable<int> GetValues(IEnumerable<DesignationData> foundDesignationData)
	{
		foreach (DesignationData foundDesignationDatum in foundDesignationData)
		{
			if (foundDesignationDatum.Value.HasValue)
			{
				yield return (int)foundDesignationDatum.Value.Value;
			}
		}
	}
}
