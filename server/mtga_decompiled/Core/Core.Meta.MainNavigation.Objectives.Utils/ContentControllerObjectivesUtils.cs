using System;
using Wizards.Unification.Models.Graph;

namespace Core.Meta.MainNavigation.Objectives.Utils;

public class ContentControllerObjectivesUtils
{
	public static bool DoesObjectiveNodeHaveHigherDynamicPriority(ClientNodeDefinition candidateObjectiveNodeDefinition, ClientNodeDefinition existingObjectiveNodeDefinition, ClientCampaignGraphState graphState)
	{
		graphState.NodeStates.TryGetValue(candidateObjectiveNodeDefinition.Id, out var value);
		graphState.NodeStates.TryGetValue(existingObjectiveNodeDefinition.Id, out var value2);
		if (value == null)
		{
			return false;
		}
		if (value2 == null)
		{
			return true;
		}
		if (value.ProgressionHistoryStateDataState == null || value2.ProgressionHistoryStateDataState == null)
		{
			if (value.ProgressionHistoryStateDataState != null)
			{
				return true;
			}
			if (value2.ProgressionHistoryStateDataState != null)
			{
				return false;
			}
			if (existingObjectiveNodeDefinition.UXInfo.ObjectiveBubbleUXInfo.DisplayPriority > candidateObjectiveNodeDefinition.UXInfo.ObjectiveBubbleUXInfo.DisplayPriority)
			{
				return true;
			}
			return false;
		}
		DateTime dateTime = ((!candidateObjectiveNodeDefinition.UXInfo.ObjectiveBubbleUXInfo.ForceVisibilityWhenNew || !(value.ProgressionHistoryStateDataState.availableDateTimeUTC > value.ProgressionHistoryStateDataState.progressedDateTimeUTC)) ? value.ProgressionHistoryStateDataState.progressedDateTimeUTC : value.ProgressionHistoryStateDataState.availableDateTimeUTC);
		DateTime dateTime2 = ((!existingObjectiveNodeDefinition.UXInfo.ObjectiveBubbleUXInfo.ForceVisibilityWhenNew || !(value2.ProgressionHistoryStateDataState.availableDateTimeUTC > value2.ProgressionHistoryStateDataState.progressedDateTimeUTC)) ? value2.ProgressionHistoryStateDataState.progressedDateTimeUTC : value2.ProgressionHistoryStateDataState.availableDateTimeUTC);
		if ((dateTime - dateTime2).TotalSeconds > 1.0)
		{
			return true;
		}
		if ((dateTime2 - dateTime).TotalSeconds > 1.0)
		{
			return false;
		}
		return existingObjectiveNodeDefinition.UXInfo.ObjectiveBubbleUXInfo.DisplayPriority > candidateObjectiveNodeDefinition.UXInfo.ObjectiveBubbleUXInfo.DisplayPriority;
	}
}
