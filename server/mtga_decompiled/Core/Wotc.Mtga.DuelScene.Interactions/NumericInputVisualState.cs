using System;

namespace Wotc.Mtga.DuelScene.Interactions;

[Flags]
public enum NumericInputVisualState
{
	None = 0,
	IncrementEnabled = 1,
	IncrementManyEnabled = 2,
	DecrementEnabled = 4,
	DecrementManyEnabled = 8,
	CanSubmit = 0x10
}
