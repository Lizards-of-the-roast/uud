using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class PrototypeCastComparer : IComparer<Action>
{
	public int Compare(Action x, Action y)
	{
		bool value = x.ActionType == ActionType.CastPrototype;
		return (y.ActionType == ActionType.CastPrototype).CompareTo(value);
	}
}
