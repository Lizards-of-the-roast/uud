using System;
using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators;

public abstract class EvaluatorBase_Int : IEvaluator
{
	public bool ExpectedResult = true;

	public int MinExpectedResult;

	public int MaxExpectedResult;

	public IntOperationType Operation;

	public abstract bool Execute(IBlackboard bb);

	public static bool GetResult(int minExpectedResult, int maxExpectedResult, IntOperationType opType, bool expectedResult, int inValue)
	{
		switch (opType)
		{
		case IntOperationType.Equals:
			return inValue == minExpectedResult == expectedResult;
		case IntOperationType.LessThan:
		case IntOperationType.BetweenExclusive:
		case IntOperationType.GreaterThan:
			return (inValue > minExpectedResult && inValue < maxExpectedResult) == expectedResult;
		case IntOperationType.LessThanOrEqualTo:
		case IntOperationType.BetweenInclusive:
		case IntOperationType.GreaterThanOrEqualTo:
			return (inValue >= minExpectedResult && inValue <= maxExpectedResult) == expectedResult;
		default:
			throw new InvalidOperationException($"Unhandled IntOperationType of {opType} in GetResult()!");
		}
	}
}
