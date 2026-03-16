using System.Collections.Generic;

namespace Wotc.Mtga.Hangers;

public class TokenPowerComparer : IComparer<FaceHanger.FaceCardInfo>
{
	public int Compare(FaceHanger.FaceCardInfo x, FaceHanger.FaceCardInfo y)
	{
		return (x.CardData.Power.DefinedValue - y.CardData.Power.DefinedValue).GetValueOrDefault();
	}
}
