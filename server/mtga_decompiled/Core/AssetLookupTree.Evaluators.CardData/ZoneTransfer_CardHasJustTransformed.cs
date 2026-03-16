using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class ZoneTransfer_CardHasJustTransformed : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if ((bb.CardData?.LinkedFaceType ?? LinkedFace.None) != LinkedFace.DfcFront)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, inValue: false);
		}
		MtgCardInstance mtgCardInstance = bb.CardData?.Instance ?? null;
		ICardDataAdapter value;
		MtgCardInstance mtgCardInstance2 = (bb.SupplementalCardData.TryGetValue(SupplementalKey.OldInstance, out value) ? value.Instance : null);
		if (mtgCardInstance == null || mtgCardInstance2 == null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, inValue: false);
		}
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, mtgCardInstance2.InstanceId != mtgCardInstance.InstanceId && mtgCardInstance2.OthersideGrpId == mtgCardInstance.GrpId);
	}
}
