using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions;

public readonly struct TargetSelectionParams
{
	public readonly MtgCardInstance SourceCard;

	public readonly MtgCardInstance TargetCard;

	public readonly uint TargetingAbilityGrpId;

	public readonly Wotc.Mtgo.Gre.External.Messaging.HighlightType TargetingHighlightType;

	public TargetSelectionParams(MtgCardInstance source, MtgCardInstance target, uint targetingAbilityGrpId, Wotc.Mtgo.Gre.External.Messaging.HighlightType targetingHighlightType)
	{
		SourceCard = source;
		TargetCard = target;
		TargetingAbilityGrpId = targetingAbilityGrpId;
		TargetingHighlightType = targetingHighlightType;
	}
}
