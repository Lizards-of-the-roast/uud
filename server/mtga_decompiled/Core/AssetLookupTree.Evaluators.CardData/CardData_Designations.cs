using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_Designations : EvaluatorBase_List<Designation>
{
	public override bool Execute(IBlackboard bb)
	{
		IEnumerable<Designation> inValues = Array.Empty<Designation>();
		if (bb.CardData?.Instance != null)
		{
			inValues = bb.CardData.Instance.Designations.Select((DesignationData x) => x.Type);
		}
		return EvaluatorBase_List<Designation>.GetResult(ExpectedValues, Operation, ExpectedResult, inValues, MinCount, MaxCount);
	}
}
