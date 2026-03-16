using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class PlayMDFCComparer : IComparer<Action>
{
	public int Compare(Action x, Action y)
	{
		bool flag = x.ActionType == ActionType.PlayMdfc;
		bool value = y.ActionType == ActionType.PlayMdfc;
		return flag.CompareTo(value);
	}
}
