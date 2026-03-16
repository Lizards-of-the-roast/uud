using System.ComponentModel;

namespace Wotc.Mtga.DuelScene.Universal;

public enum ValidatorBehavior
{
	[Description("ALL the validators in the list must be valid for a card to end up in this group.")]
	AND,
	[Description("ANY of the validators in the list can be valid for a card to end up in this group.")]
	OR
}
