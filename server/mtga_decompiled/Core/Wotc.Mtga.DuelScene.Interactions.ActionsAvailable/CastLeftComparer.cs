using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class CastLeftComparer : IComparer<Action>
{
	public int Compare(Action x, Action y)
	{
		bool value = x.ActionType == ActionType.CastLeft;
		return (y.ActionType == ActionType.CastLeft).CompareTo(value);
	}
}
