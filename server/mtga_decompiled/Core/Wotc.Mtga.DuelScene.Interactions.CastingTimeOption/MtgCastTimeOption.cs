using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.CastingTimeOption;

public readonly struct MtgCastTimeOption
{
	public readonly CastingTimeOptionType Type;

	public readonly uint GrpId;

	public readonly uint AffectorId;

	public readonly uint AffectedId;

	public static MtgCastTimeOption Done => new MtgCastTimeOption(CastingTimeOptionType.Done, 0u, 0u, 0u);

	public MtgCastTimeOption(CastingTimeOptionType type, uint grpId, uint affectorId, uint affectedId)
	{
		Type = type;
		GrpId = grpId;
		AffectorId = affectorId;
		AffectedId = affectedId;
	}
}
