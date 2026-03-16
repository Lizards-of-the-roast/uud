namespace Wotc.Mtga.Events;

public enum EventTimerState
{
	Unjoined_Locked,
	Preview,
	Unjoined,
	Unjoined_LockingSoon,
	Joined,
	Joined_ClosingSoon,
	ClosedAndCompleted
}
