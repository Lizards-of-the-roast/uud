using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class AdventureComparer : IComparer<Action>
{
	public int Compare(Action x, Action y)
	{
		bool value = x.ActionType == ActionType.CastAdventure;
		return (y.ActionType == ActionType.CastAdventure).CompareTo(value);
	}
}
