using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.PhaseLadder;

public static class PhaseLadderUtility
{
	public static ToggleResults GetToggleResults(GREPlayerNum associatedPlayer, SettingStatus currentState, bool nextTurnPhase, bool isCombatPhase, SettingScope appliesTo, SettingScope nextPlayerScope, SettingScope activePlayerScope)
	{
		switch (associatedPlayer)
		{
		case GREPlayerNum.LocalPlayer:
			if (currentState != SettingStatus.Set)
			{
				return ToggleResults.SetTeamStops;
			}
			return ToggleResults.ClearTeamStops;
		case GREPlayerNum.Opponent:
			if (currentState != SettingStatus.Set)
			{
				return ToggleResults.SetOpponentStops;
			}
			return ToggleResults.ClearOpponentStops;
		default:
			if (appliesTo == SettingScope.AnyPlayer)
			{
				return ToggleResults.ClearAll;
			}
			if (isCombatPhase)
			{
				if (currentState == SettingStatus.Clear || currentState == SettingStatus.None)
				{
					if (!nextTurnPhase)
					{
						return ToggleResults.SetActivePlayerStops;
					}
					return ToggleResults.None;
				}
				if (appliesTo == activePlayerScope)
				{
					return ToggleResults.ClearActivePlayerStops;
				}
				if (appliesTo == nextPlayerScope)
				{
					return ToggleResults.ClearNextPlayerStops;
				}
				return ToggleResults.None;
			}
			if (appliesTo == SettingScope.None)
			{
				if (!nextTurnPhase)
				{
					return ToggleResults.SetActivePlayerStops;
				}
				return ToggleResults.SetNextPlayerStops;
			}
			if (nextTurnPhase)
			{
				if (appliesTo != activePlayerScope)
				{
					return ToggleResults.ClearNextPlayerStops;
				}
				return ToggleResults.ClearActivePlayerStops;
			}
			if (appliesTo == activePlayerScope)
			{
				return ToggleResults.SetNextPlayerStops;
			}
			if (appliesTo != nextPlayerScope)
			{
				return ToggleResults.None;
			}
			return ToggleResults.ClearAll;
		}
	}
}
