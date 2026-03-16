using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class FreeCastActionFilter : IListFilter<GreInteraction>
{
	private readonly IGameplaySettingsProvider _gameplaySettings;

	public FreeCastActionFilter(IGameplaySettingsProvider gameplaySettings)
	{
		_gameplaySettings = gameplaySettings ?? NullGameplaySettingsProvider.Default;
	}

	public void Filter(ref List<GreInteraction> list)
	{
		if (_gameplaySettings.FullControlEnabled)
		{
			return;
		}
		int num = 0;
		while (num < list.Count)
		{
			if (HasCorrespondingFreeCast(num, list))
			{
				list.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
	}

	private bool HasCorrespondingFreeCast(int idx, IReadOnlyList<GreInteraction> interactions)
	{
		Action greAction = interactions[idx].GreAction;
		if (!greAction.IsCastAction())
		{
			return false;
		}
		if (greAction.IsFreeCast())
		{
			return false;
		}
		if (greAction.AlternativeGrpId != 0)
		{
			return false;
		}
		if (greAction.ContainsXCost())
		{
			return false;
		}
		for (int i = 0; i < interactions.Count; i++)
		{
			if (i != idx)
			{
				Action greAction2 = interactions[i].GreAction;
				if (greAction2.IsFreeCast() && greAction2.GrpId == greAction.GrpId)
				{
					return true;
				}
			}
		}
		return false;
	}
}
