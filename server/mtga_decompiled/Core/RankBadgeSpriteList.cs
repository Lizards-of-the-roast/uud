using System;
using UnityEngine;

public class RankBadgeSpriteList : MonoBehaviour
{
	[Serializable]
	public class BadgeClass
	{
		public Sprite[] BadgeTiers;

		public Sprite GetSprite(int tierInt)
		{
			int value = tierInt - 1;
			return BadgeTiers[Mathf.Clamp(value, 0, BadgeTiers.Length - 1)];
		}
	}

	[SerializeField]
	private BadgeClass[] BadgeClasses;

	public Sprite GetCurrentRankSprite(int currentClassInt, int currentTierInt)
	{
		return GetBadgeClass(currentClassInt).GetSprite(currentTierInt);
	}

	public Sprite GetNextRankSprite(int currentClassInt, int currentTierInt)
	{
		BadgeClass badgeClass = GetBadgeClass(currentClassInt);
		int num = currentTierInt - 1;
		if (num <= 0)
		{
			badgeClass = GetBadgeClass(currentClassInt + 1);
			num = 4;
		}
		return badgeClass.GetSprite(num);
	}

	private BadgeClass GetBadgeClass(int classInt)
	{
		return BadgeClasses[Mathf.Min(classInt, BadgeClasses.Length - 1)];
	}
}
