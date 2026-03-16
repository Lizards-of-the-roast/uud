using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class AltGrpIdComparer : IComparer<Action>
{
	public int Compare(Action x, Action y)
	{
		return x.AlternativeGrpId.CompareTo(y.AlternativeGrpId);
	}
}
