using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_QualificationTypes : EvaluatorBase_List<QualificationType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null)
		{
			return EvaluatorBase_List<QualificationType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.AffectedByQualifications.Select((QualificationData x) => x.Type), MinCount, MaxCount);
		}
		return false;
	}
}
