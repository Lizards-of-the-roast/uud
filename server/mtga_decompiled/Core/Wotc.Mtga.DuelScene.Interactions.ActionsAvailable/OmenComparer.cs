using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class OmenComparer : IComparer<Action>
{
	public int Compare(Action x, Action y)
	{
		bool value = x.ActionType == ActionType.CastOmen;
		return (y.ActionType == ActionType.CastOmen).CompareTo(value);
	}
}
