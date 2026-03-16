using System;
using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Extensions;

public static class ManaRequirementExtensions
{
	public static IReadOnlyList<ManaRequirement> GetNonEmptyManaCosts(this IReadOnlyList<ManaRequirement> manaRequirements)
	{
		if (manaRequirements == null || manaRequirements.Count == 0)
		{
			return Array.Empty<ManaRequirement>();
		}
		if (manaRequirements.Count == 1 && manaRequirements[0].IsFunctionallyNull())
		{
			return Array.Empty<ManaRequirement>();
		}
		return manaRequirements;
	}

	private static bool IsFunctionallyNull(this ManaRequirement manaRequirement)
	{
		if (manaRequirement != null)
		{
			if (manaRequirement.AbilityGrpId == 0 && manaRequirement.Color.Count == 0 && manaRequirement.CostId == 0 && manaRequirement.Count == 0)
			{
				return manaRequirement.ObjectId == 0;
			}
			return false;
		}
		return true;
	}
}
