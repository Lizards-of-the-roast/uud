using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class ActivateActionComparer : IComparer<Action>
{
	public int Compare(Action x, Action y)
	{
		return x.IsActivateAction().CompareTo(y.IsActivateAction());
	}
}
