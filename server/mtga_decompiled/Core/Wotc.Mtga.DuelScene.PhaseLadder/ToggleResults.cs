using System;

namespace Wotc.Mtga.DuelScene.PhaseLadder;

[Flags]
public enum ToggleResults
{
	None = 0,
	ClearTeamStops = 1,
	ClearOpponentStops = 2,
	ClearNextPlayerStops = 4,
	ClearActivePlayerStops = 8,
	SetTeamStops = 0x10,
	SetOpponentStops = 0x20,
	SetNextPlayerStops = 0x40,
	SetActivePlayerStops = 0x80,
	ClearAll = 3
}
