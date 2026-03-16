using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.LinkInfo;

public class LinkInfo_Type : EvaluatorBase_List<LinkType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (!bb.LinkInfo.Equals(default(LinkInfoData)))
		{
			return EvaluatorBase_List<LinkType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.LinkInfo.LinkType);
		}
		return false;
	}
}
