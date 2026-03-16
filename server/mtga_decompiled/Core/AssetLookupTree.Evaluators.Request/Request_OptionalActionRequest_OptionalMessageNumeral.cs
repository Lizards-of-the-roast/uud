using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.Request;

public class Request_OptionalActionRequest_OptionalMessageNumeral : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Request == null)
		{
			return false;
		}
		if (bb.Request.Prompt == null)
		{
			return false;
		}
		if (bb.GameState == null)
		{
			return false;
		}
		foreach (PromptParameter parameter in bb.Request.Prompt.Parameters)
		{
			if (!bb.GameState.TryGetCard((uint)parameter.NumberValue, out var _))
			{
				return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, parameter.NumberValue);
			}
		}
		return false;
	}
}
