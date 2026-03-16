using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class MDFCAltCastComparer : IComparer<Action>
{
	public int Compare(Action x, Action y)
	{
		bool flag = x.ActionType == ActionType.CastMdfc && x.AlternativeGrpId != 0;
		bool value = y.ActionType == ActionType.CastMdfc && y.AlternativeGrpId != 0;
		return flag.CompareTo(value);
	}
}
