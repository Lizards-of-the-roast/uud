using System;
using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators;

public abstract class EvaluatorBase_DateTime : IEvaluator
{
	public long MinExpectedResult;

	public long MaxExpectedResult;

	public DateTimeOperationType Operation = DateTimeOperationType.After;

	public abstract bool Execute(IBlackboard bb);

	public static bool GetResult(long minExpectedResult, long maxExpectedResult, DateTime inValue)
	{
		long ticks = inValue.Ticks;
		if (ticks > minExpectedResult)
		{
			return ticks < maxExpectedResult;
		}
		return false;
	}
}
