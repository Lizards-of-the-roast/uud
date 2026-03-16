using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class CastMDFCComparer : IComparer<Action>
{
	public int Compare(Action x, Action y)
	{
		bool flag = x.ActionType == ActionType.CastMdfc;
		bool value = y.ActionType == ActionType.CastMdfc;
		return flag.CompareTo(value);
	}
}
