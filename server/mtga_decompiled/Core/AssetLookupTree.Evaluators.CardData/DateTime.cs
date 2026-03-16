using System;
using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class DateTime : EvaluatorBase_DateTime
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.DateTimeUtc != default(System.DateTime))
		{
			return EvaluatorBase_DateTime.GetResult(MinExpectedResult, MaxExpectedResult, bb.DateTimeUtc);
		}
		return false;
	}
}
