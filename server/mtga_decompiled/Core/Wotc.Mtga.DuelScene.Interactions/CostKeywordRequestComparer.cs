using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Interactions;

public class CostKeywordRequestComparer : IComparer<BaseUserRequest>
{
	public int Compare(BaseUserRequest x, BaseUserRequest y)
	{
		return (y is CastingTimeOption_DoneRequest).CompareTo(x is CastingTimeOption_DoneRequest);
	}
}
