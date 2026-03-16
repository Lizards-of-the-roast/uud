using Pooling;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class PrototypeFaceInfoGenerator : LinkedFaceInfoGenerator
{
	public PrototypeFaceInfoGenerator(IAbilityDataProvider abilityProvider, IClientLocProvider locManager, IObjectPool genericPool)
		: base(LinkedFace.PrototypeChild, LinkedFace.PrototypeParent, "DuelScene/FaceHanger/Original", FaceHanger.HangerType.Prototype, new Options(Options.Default)
		{
			HideForCopies = true,
			CopyPerpetualEffects = true
		}, abilityProvider, locManager, genericPool)
	{
	}
}
