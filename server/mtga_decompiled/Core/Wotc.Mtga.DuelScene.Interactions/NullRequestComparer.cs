using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Interactions;

public class NullRequestComparer : IComparer<BaseUserRequest>
{
	public int Compare(BaseUserRequest x, BaseUserRequest y)
	{
		return 0;
	}
}
