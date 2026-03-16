using System.Collections.Generic;

namespace Wotc.Mtga.Hangers;

public class FaceHangerComparer : IComparer<FaceHanger.FaceCardInfo>
{
	private readonly FaceHanger.HangerType[] _typePriority = new FaceHanger.HangerType[13]
	{
		FaceHanger.HangerType.RoleReference,
		FaceHanger.HangerType.MorphReference,
		FaceHanger.HangerType.CopyReference,
		FaceHanger.HangerType.Prototype,
		FaceHanger.HangerType.DFC,
		FaceHanger.HangerType.MDFC,
		FaceHanger.HangerType.NamedCard,
		FaceHanger.HangerType.RelatedObj_Dungeon,
		FaceHanger.HangerType.RelatedObj_DayNight,
		FaceHanger.HangerType.ConjureReference,
		FaceHanger.HangerType.Specialized,
		FaceHanger.HangerType.Meld,
		FaceHanger.HangerType.TokenReference
	};

	public int Compare(FaceHanger.FaceCardInfo x, FaceHanger.FaceCardInfo y)
	{
		FaceHanger.HangerType hangerType = x.HangerType;
		FaceHanger.HangerType hangerType2 = y.HangerType;
		FaceHanger.HangerType[] typePriority = _typePriority;
		foreach (FaceHanger.HangerType hangerType3 in typePriority)
		{
			bool value = hangerType == hangerType3;
			int num = (hangerType2 == hangerType3).CompareTo(value);
			if (num != 0)
			{
				return num;
			}
		}
		return 0;
	}
}
