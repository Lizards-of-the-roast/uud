using System.Collections.Generic;

namespace Wotc.Mtga.Hangers;

public class NullFaceHangerComparer : IComparer<FaceHanger.FaceCardInfo>
{
	public int Compare(FaceHanger.FaceCardInfo x, FaceHanger.FaceCardInfo y)
	{
		return 0;
	}
}
