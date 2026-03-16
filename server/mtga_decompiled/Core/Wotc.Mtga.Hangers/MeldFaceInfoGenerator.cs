using Pooling;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class MeldFaceInfoGenerator : FromIntoInfoFaceGenerator
{
	public MeldFaceInfoGenerator(IAbilityDataProvider abilityProvider, IClientLocProvider locManager, IObjectPool genericPool)
		: base(LinkedFace.MeldedPermanent, LinkedFace.MeldCard, FaceHanger.HangerType.Meld, "DuelScene/FaceHanger/MeldedFrom", "DuelScene/FaceHanger/MeldedInto", FromIntoArrows.Default, LinkedFaceInfoGenerator.Options.DefaultHideForCopies, abilityProvider, locManager, genericPool)
	{
	}
}
