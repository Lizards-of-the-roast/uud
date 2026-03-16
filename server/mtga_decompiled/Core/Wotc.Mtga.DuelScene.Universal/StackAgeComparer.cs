using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.Universal;

public class StackAgeComparer : IComparer<UniversalBattlefieldStack>
{
	public Dictionary<uint, uint> TitleIdToStackAge { get; } = new Dictionary<uint, uint>();

	public virtual int Compare(UniversalBattlefieldStack a, UniversalBattlefieldStack b)
	{
		int num = 0;
		if (a.StackParentModel.TitleId != b.StackParentModel.TitleId)
		{
			num = TitleIdToStackAge[a.StackParentModel.TitleId].CompareTo(TitleIdToStackAge[b.StackParentModel.TitleId]);
		}
		if (num == 0)
		{
			num = a.Age.CompareTo(b.Age);
		}
		return num;
	}
}
