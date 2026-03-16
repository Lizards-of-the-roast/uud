using System;

namespace Core.NPEStitcher;

[Flags]
public enum NpeModuleState
{
	Uninitialized = 0,
	CanJoin = 1,
	CanPlay = 2,
	CanSkip = 4,
	HaveRewards = 8,
	Error = 0x10,
	Complete = 0x20
}
