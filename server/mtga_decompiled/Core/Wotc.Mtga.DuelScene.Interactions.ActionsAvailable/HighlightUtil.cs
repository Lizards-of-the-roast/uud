using System;
using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public static class HighlightUtil
{
	public static HighlightType GetHottestHighlightForActions(MtgEntity entity, IReadOnlyCollection<GreInteraction> actions)
	{
		if (!(entity is MtgCardInstance mtgCardInstance))
		{
			if (entity is MtgPlayer)
			{
				return GetHottestHighlightForActions(actions, (IReadOnlyCollection<ShouldntPlayData>)(object)Array.Empty<ShouldntPlayData>());
			}
			return HighlightType.None;
		}
		return GetHottestHighlightForActions(actions, mtgCardInstance.PlayWarnings);
	}

	public static HighlightType GetHottestHighlightForActions(IReadOnlyCollection<GreInteraction> actions, IReadOnlyCollection<ShouldntPlayData> warnings)
	{
		HighlightType highlightType = HighlightType.None;
		foreach (GreInteraction item in (IEnumerable<GreInteraction>)(((object)actions) ?? ((object)Array.Empty<GreInteraction>())))
		{
			HighlightType highlightForAction = GetHighlightForAction(item, warnings);
			if (highlightForAction.IsHotterThan(highlightType))
			{
				highlightType = highlightForAction;
			}
		}
		return highlightType;
	}

	public static HighlightType GetHighlightForAction(GreInteraction greInteraction, IReadOnlyCollection<ShouldntPlayData> warnings)
	{
		if (greInteraction == null || greInteraction.GreAction == null)
		{
			return HighlightType.None;
		}
		if (!greInteraction.IsActive)
		{
			return HighlightType.None;
		}
		if (!greInteraction.GreAction.CanAffordToCast())
		{
			return HighlightType.None;
		}
		HighlightType highlightType = (HighlightType)((greInteraction.Type == ActionType.ActivateMana) ? ((Wotc.Mtgo.Gre.External.Messaging.HighlightType)10) : ((greInteraction.GreAction.Highlight == Wotc.Mtgo.Gre.External.Messaging.HighlightType.None) ? Wotc.Mtgo.Gre.External.Messaging.HighlightType.Hot : greInteraction.GreAction.Highlight));
		if (highlightType.IsHotterThan(HighlightType.Cold) && warnings != null && warnings.Exists((ShouldntPlayData x) => x.AbilityId == greInteraction.GreAction.AbilityGrpId || x.Reasons.Contains(ShouldntPlayData.ReasonType.Legendary)))
		{
			highlightType = HighlightType.Cold;
		}
		return highlightType;
	}

	public static bool IsHotterThan(this HighlightType lhs, HighlightType rhs)
	{
		return lhs.ToHotness() > rhs.ToHotness();
	}

	private static float ToHotness(this HighlightType highlightType)
	{
		return highlightType switch
		{
			HighlightType.None => 0f, 
			HighlightType.Cold => 1f, 
			HighlightType.ColdMana => 1.5f, 
			HighlightType.Tepid => 2f, 
			HighlightType.Hot => 3f, 
			HighlightType.AutoPay => 4f, 
			HighlightType.Selected => 4f, 
			HighlightType.OpponentHover => 5f, 
			HighlightType.Invalid => 0f, 
			_ => 0f, 
		};
	}
}
