using System.Collections.Generic;
using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.LinkInfo;

public class LinkInfo_Details : EvaluatorBase_List<string>
{
	public override bool Execute(IBlackboard bb)
	{
		if (!bb.LinkInfo.Equals(default(LinkInfoData)))
		{
			return EvaluatorBase_List<string>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.LinkInfo.Details.Select((KeyValuePair<string, object> x) => $"{x.Key}:{x.Value}"), MinCount, MaxCount);
		}
		return false;
	}
}
