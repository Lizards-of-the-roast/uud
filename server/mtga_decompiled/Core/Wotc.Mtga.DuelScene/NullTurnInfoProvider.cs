using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class NullTurnInfoProvider : ITurnInfoProvider
{
	public static readonly ITurnInfoProvider Default = new NullTurnInfoProvider();

	public ObservableValue<MtgTurnInfo> TurnInfo { get; }

	public ObservableReference<IReadOnlyList<ExtraTurn>> ExtraTurns { get; }

	public uint EventTranslationTurnNumber { get; }
}
