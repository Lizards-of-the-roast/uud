using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class SpecialActionComparer : IComparer<Action>
{
	public int Compare(Action x, Action y)
	{
		bool flag = x.ActionType == ActionType.Special;
		bool value = y.ActionType == ActionType.Special;
		return flag.CompareTo(value);
	}
}
