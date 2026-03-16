using System;
using System.Collections.Generic;
using System.Linq;
using Core.Meta.MainNavigation.Achievements;
using Wizards.Arena.Enums.CampaignGraph;
using Wizards.GeneralUtilities.Extensions;
using Wizards.Unification.Models.Graph;
using Wotc.Mtga.Extensions;

namespace Core.Achievements;

public class AchievementToastsProvider : IAchievementsToastProvider
{
	private static readonly AchievementNotificationType[] _defaultNotificationTypeFilter = new AchievementNotificationType[2]
	{
		AchievementNotificationType.Completed,
		AchievementNotificationType.Progress
	};

	private readonly Dictionary<AchievementNotificationType, Queue<AchievementNotification>> _receivedAchievementToasts = new Dictionary<AchievementNotificationType, Queue<AchievementNotification>>();

	public event Action ToastReceived;

	public static AchievementToastsProvider Create()
	{
		return new AchievementToastsProvider();
	}

	public bool HasAchievementNotificationInQueue(params AchievementNotificationType[] achievementTypesToCount)
	{
		if (_receivedAchievementToasts.Count == 0)
		{
			return false;
		}
		if (achievementTypesToCount.Length == 0)
		{
			achievementTypesToCount = _defaultNotificationTypeFilter;
		}
		Queue<AchievementNotification> value;
		return achievementTypesToCount.Exists(_receivedAchievementToasts, (AchievementNotificationType typeToCount, Dictionary<AchievementNotificationType, Queue<AchievementNotification>> recdToasts) => recdToasts.TryGetValue(typeToCount, out value) && value.Count > 0);
	}

	public IReadOnlyList<AchievementNotification> GetMultipleNextAchievementNotificationsInQueue(int numberToRetrieve = int.MaxValue, params AchievementNotificationType[] achievementTypesToGet)
	{
		if (_receivedAchievementToasts.Count == 0 || numberToRetrieve <= 0)
		{
			return Array.Empty<AchievementNotification>();
		}
		if (achievementTypesToGet.Length == 0)
		{
			achievementTypesToGet = _defaultNotificationTypeFilter;
		}
		List<AchievementNotification> list = new List<AchievementNotification>();
		bool flag = false;
		AchievementNotificationType[] array = achievementTypesToGet;
		foreach (AchievementNotificationType key in array)
		{
			if (_receivedAchievementToasts.TryGetValue(key, out var value))
			{
				while (value.Count > 0)
				{
					AchievementNotification item = value.Dequeue();
					list.Add(item);
					flag = list.Count >= numberToRetrieve;
					if (flag)
					{
						break;
					}
				}
			}
			if (flag)
			{
				break;
			}
		}
		return list;
	}

	public IReadOnlyList<AchievementNotification> GetMultipleNextAchievementNotificationsInQueue(params AchievementNotificationType[] achievementTypesToGet)
	{
		return GetMultipleNextAchievementNotificationsInQueue(int.MaxValue, achievementTypesToGet);
	}

	public void AddAchievementToast(CampaignGraphDeltas response, IClientAchievement achievement)
	{
		AchievementNotificationType achievementNotificationType = AchievementNotificationType.Unknown;
		foreach (CampaignGraphDeltaNodeDelta item2 in response.AchievementToasts.Select((GraphIdNodeId curGraphNodeId) => response.Deltas[curGraphNodeId.GraphId][curGraphNodeId.NodeId]))
		{
			achievementNotificationType = item2.PostState.Status switch
			{
				Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Locked => AchievementNotificationType.Unknown, 
				Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Available => AchievementNotificationType.Progress, 
				Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Completed => AchievementNotificationType.Completed, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
		AchievementNotification item = new AchievementNotification(achievementNotificationType, achievement);
		_receivedAchievementToasts.GetOrCreate(achievementNotificationType).Enqueue(item);
		this.ToastReceived?.Invoke();
	}
}
