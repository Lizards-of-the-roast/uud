using System;
using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators;

public abstract class EvaluatorBase_IntToInt : IEvaluator
{
	public bool ExpectedResult = true;

	public IntToIntOperationType Operation;

	public int ValueOneModifier;

	public int ValueTwoModifier;

	public abstract bool Execute(IBlackboard bb);

	public static bool GetResult(int valueOneModifier, int valueTwoModifier, IntToIntOperationType opType, bool expectedResult, int inValueOne, int inValueTwo)
	{
		inValueOne += valueOneModifier;
		inValueTwo += valueTwoModifier;
		return opType switch
		{
			IntToIntOperationType.Equals => inValueOne == inValueTwo == expectedResult, 
			IntToIntOperationType.LessThan => inValueOne < inValueTwo == expectedResult, 
			IntToIntOperationType.GreaterThan => inValueOne > inValueTwo == expectedResult, 
			IntToIntOperationType.LessThanOrEqualTo => inValueOne <= inValueTwo == expectedResult, 
			IntToIntOperationType.GreaterThanOrEqualTo => inValueOne >= inValueTwo == expectedResult, 
			_ => throw new InvalidOperationException($"Unhandled IntToIntOperationType of {opType} in GetResult()!"), 
		};
	}
}
