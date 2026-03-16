using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators;

public abstract class EvaluatorBase_List<T> : IEvaluator
{
	public readonly HashSet<T> ExpectedValues = new HashSet<T>();

	public SetOperationType Operation;

	public int MinCount = 1;

	public int MaxCount = 1;

	public bool ExpectedResult = true;

	public abstract bool Execute(IBlackboard bb);

	public static bool GetResult(HashSet<T> expectedValues, SetOperationType opType, bool expectedResult, IEnumerable<T> inValues, int minCount, int maxCount)
	{
		return opType switch
		{
			SetOperationType.Overlaps => expectedResult == ComparisonHelpers.Overlaps(expectedValues, inValues), 
			SetOperationType.SupersetOf => expectedResult == ComparisonHelpers.IsSuperSet(expectedValues, inValues), 
			SetOperationType.EqualTo => expectedResult == ComparisonHelpers.IsSetEqual(expectedValues, inValues), 
			SetOperationType.SubsetOf => expectedResult == ComparisonHelpers.IsSubset(expectedValues, inValues), 
			SetOperationType.MinimumCount => expectedResult == ComparisonHelpers.GetIntersectionCount(expectedValues, inValues) >= minCount, 
			SetOperationType.MaximumCount => expectedResult == ComparisonHelpers.GetIntersectionCount(expectedValues, inValues) <= maxCount, 
			SetOperationType.MinimumDistinctCount => expectedResult == ComparisonHelpers.GetIntersectionCount(expectedValues, inValues.Distinct()) >= minCount, 
			_ => throw new ArgumentOutOfRangeException("opType", opType, null), 
		};
	}

	public static bool GetResult(HashSet<T> expectedValues, SetOperationType opType, bool expectedResult, T inValue)
	{
		switch (opType)
		{
		case SetOperationType.Overlaps:
		case SetOperationType.SupersetOf:
			return expectedResult == expectedValues.Contains(inValue);
		case SetOperationType.EqualTo:
		case SetOperationType.SubsetOf:
			return expectedResult == (expectedValues.Count == 1 && expectedValues.Contains(inValue));
		default:
			throw new ArgumentOutOfRangeException("opType", opType, null);
		}
	}
}
