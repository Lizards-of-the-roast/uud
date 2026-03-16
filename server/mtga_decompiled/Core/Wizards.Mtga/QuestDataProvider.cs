using System;
using System.Collections.Generic;
using System.Linq;
using Core.Meta.Quests;
using Wizards.Arena.Promises;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Wizards.Mtga;

public class QuestDataProvider
{
	private readonly IQuestServiceWrapper _questServiceWrapper;

	private List<Client_QuestData> _questData;

	private PlayerProgressDailyWeekly _dailyWeeklyProgress;

	public static QuestDataProvider Create()
	{
		return new QuestDataProvider(Pantry.Get<IQuestServiceWrapper>());
	}

	public QuestDataProvider(IQuestServiceWrapper questServiceWrapper)
	{
		_questServiceWrapper = questServiceWrapper;
	}

	public List<Client_QuestData> GetQuests()
	{
		return _questData;
	}

	public PlayerProgressDailyWeekly GetDailyWeeklyProgress()
	{
		return _dailyWeeklyProgress;
	}

	public Promise<List<Client_QuestData>> RefreshQuestData()
	{
		return _questServiceWrapper.GetQuests().IfSuccess(delegate(Promise<List<Client_QuestData>> promise)
		{
			_questData = promise.Result;
		});
	}

	public Promise<PlayerProgressDailyWeekly> RefreshDailyWeeklyQuests()
	{
		return _questServiceWrapper.GetPlayerProgressDailyWeekly().IfSuccess(delegate(Promise<PlayerProgressDailyWeekly> promise)
		{
			_dailyWeeklyProgress = promise.Result;
		});
	}

	public Promise<List<Client_QuestData>> SwapPlayerQuest(string id)
	{
		return _questServiceWrapper.SwapPlayerQuest(id).IfSuccess(delegate(Promise<List<Client_QuestData>> promise)
		{
			_questData = promise.Result;
		});
	}

	public void UpdateQuestsFromPostMatch(PostMatchClientUpdate postMatchClientUpdate)
	{
		List<Client_QuestData> list = new List<Client_QuestData>();
		if (_questData != null)
		{
			list.AddRange(_questData.Where((Client_QuestData q) => q != null));
		}
		if (postMatchClientUpdate?.questUpdate != null)
		{
			foreach (Client_QuestData quest in postMatchClientUpdate.questUpdate.Select((QuestData q) => new Client_QuestData(q)))
			{
				int num = list.FindIndex(delegate(Client_QuestData x)
				{
					Guid? guid = x?.Id;
					Guid id = quest.Id;
					if (!guid.HasValue)
					{
						return false;
					}
					return !guid.HasValue || guid.GetValueOrDefault() == id;
				});
				if (num != -1)
				{
					list[num] = quest;
				}
				else
				{
					list.Add(quest);
				}
			}
		}
		_questData = list;
		if (_dailyWeeklyProgress == null)
		{
			return;
		}
		if (postMatchClientUpdate != null)
		{
			PeriodicRewardsTrackUpdate dailyWinUpdates = postMatchClientUpdate.dailyWinUpdates;
			if (dailyWinUpdates != null && dailyWinUpdates.CurrentSequenceId.HasValue)
			{
				_dailyWeeklyProgress.dailySequence = postMatchClientUpdate.dailyWinUpdates.CurrentSequenceId.Value;
			}
		}
		if (postMatchClientUpdate != null)
		{
			PeriodicRewardsTrackUpdate weeklyWinUpdates = postMatchClientUpdate.weeklyWinUpdates;
			if (weeklyWinUpdates != null && weeklyWinUpdates.CurrentSequenceId.HasValue)
			{
				_dailyWeeklyProgress.weeklySequence = postMatchClientUpdate.weeklyWinUpdates.CurrentSequenceId.Value;
			}
		}
	}
}
