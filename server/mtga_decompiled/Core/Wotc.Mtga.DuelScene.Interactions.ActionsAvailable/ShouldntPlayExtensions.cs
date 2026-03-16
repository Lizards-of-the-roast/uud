using System;
using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public static class ShouldntPlayExtensions
{
	public static string GetSubHeaderKey(this IEnumerable<ShouldntPlayData> playWarnings)
	{
		foreach (ShouldntPlayData item in playWarnings ?? Array.Empty<ShouldntPlayData>())
		{
			if (item.Reasons.Count != 0)
			{
				string locKeyForReason = item.Reasons[0].GetLocKeyForReason();
				if (!string.IsNullOrEmpty(locKeyForReason))
				{
					return locKeyForReason;
				}
			}
		}
		return "DuelScene/ClientPrompt/Click_Yes_No_Text";
	}

	public static string GetLocKeyForReason(this ShouldntPlayData.ReasonType reasonType)
	{
		return reasonType switch
		{
			ShouldntPlayData.ReasonType.StartingPlayer => "AbilityHanger/PlayWarning/Body_StartingPlayer", 
			ShouldntPlayData.ReasonType.Legendary => "AbilityHanger/PlayWarning/Body_Legendary", 
			ShouldntPlayData.ReasonType.UnpayableCost => "AbilityHanger/PlayWarning/Body_UnpayableCost", 
			ShouldntPlayData.ReasonType.AdaptHasCounters => "AbilityHanger/PlayWarning/Body_AdaptHasCounters", 
			ShouldntPlayData.ReasonType.WouldReturnSource => "AbilityHanger/PlayWarning/Body_WouldReturnSource", 
			ShouldntPlayData.ReasonType.EntersTapped => "AbilityHanger/PlayWarning/Body_EntersTapped", 
			ShouldntPlayData.ReasonType.WouldSacrificeSource => "AbilityHanger/PlayWarning/Body_WouldSacrificeSource", 
			ShouldntPlayData.ReasonType.RedundantActivation => string.Empty, 
			ShouldntPlayData.ReasonType.ConsequentialConditionNotMet => string.Empty, 
			ShouldntPlayData.ReasonType.Champion => "AbilityHanger/PlayWarning/Body_Champion", 
			_ => throw new NotImplementedException("Received an unhandled ShouldntPlay reason from GRE. Please investigate"), 
		};
	}
}
