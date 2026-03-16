using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.CastingTimeOption;

public readonly struct ModalOption
{
	public readonly uint GrpId;

	public readonly bool Selectable;

	public readonly ModalChoiceAdvisability Advisability;

	public readonly uint AdvisabilityPromptId;

	private ModalOption(uint grpId, bool selectable, ModalChoiceAdvisability advisabilityFieldNumber, uint advisabilityPromptId)
	{
		GrpId = grpId;
		Selectable = selectable;
		Advisability = advisabilityFieldNumber;
		AdvisabilityPromptId = advisabilityPromptId;
	}

	public static ModalOption SelectableOption(uint grpId, ModalChoiceAdvisability advisabilityFieldNumber = ModalChoiceAdvisability.None, uint advisabilityPromptId = 0u)
	{
		return new ModalOption(grpId, selectable: true, advisabilityFieldNumber, advisabilityPromptId);
	}

	public static ModalOption InactvieOption(uint grpId, ModalChoiceAdvisability advisabilityFieldNumber = ModalChoiceAdvisability.None, uint advisabilityPromptId = 0u)
	{
		return new ModalOption(grpId, selectable: false, advisabilityFieldNumber, advisabilityPromptId);
	}
}
