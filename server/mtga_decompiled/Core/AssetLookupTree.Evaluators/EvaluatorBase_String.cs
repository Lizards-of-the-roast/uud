using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AssetLookupTree.Blackboard;
using Wotc.Mtga.Extensions;

namespace AssetLookupTree.Evaluators;

public abstract class EvaluatorBase_String : IEvaluator
{
	public string ExpectedValue = string.Empty;

	public StringOperationType Operation = StringOperationType.Contains;

	public bool ExpectedResult = true;

	private static readonly Regex _wordMatcher = new Regex("(?<=[\\W|_]|^)([a-zA-Z0-9]*)(?=[\\W|_]|$)", RegexOptions.Compiled);

	public abstract bool Execute(IBlackboard bb);

	public static bool GetResult(string expectedValue, StringOperationType opType, bool expectedResult, string inValue)
	{
		if (inValue == null)
		{
			return false;
		}
		return opType switch
		{
			StringOperationType.Equals => expectedResult == inValue.Equals(expectedValue, StringComparison.InvariantCultureIgnoreCase), 
			StringOperationType.StartsWith => expectedResult == inValue.StartsWith(expectedValue, StringComparison.InvariantCultureIgnoreCase), 
			StringOperationType.EndsWith => expectedResult == inValue.EndsWith(expectedValue, StringComparison.InvariantCultureIgnoreCase), 
			StringOperationType.Contains => expectedResult == inValue.Contains(expectedValue, caseIndependent: true), 
			StringOperationType.ContainedIn => expectedResult == expectedValue.Contains(inValue, caseIndependent: true), 
			StringOperationType.HasWords => HasWords(inValue, expectedValue), 
			_ => throw new InvalidOperationException($"Unhandled StringOperationType of {opType} in GetResult()!"), 
		};
	}

	private static bool HasWords(string input, string expected)
	{
		IEnumerable<string> list = GetWords(input);
		foreach (string item in GetWords(expected))
		{
			if (!list.Exists(item, (string inVal, string value) => inVal.Equals(value, StringComparison.InvariantCultureIgnoreCase)))
			{
				return false;
			}
		}
		return true;
		static IEnumerable<string> GetWords(string input2)
		{
			foreach (Match item2 in _wordMatcher.Matches(input2))
			{
				if (item2.Success && !string.IsNullOrWhiteSpace(item2.Value))
				{
					yield return item2.Value;
				}
			}
		}
	}
}
