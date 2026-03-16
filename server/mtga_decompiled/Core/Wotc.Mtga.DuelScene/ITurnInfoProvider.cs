using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public interface ITurnInfoProvider
{
	ObservableValue<MtgTurnInfo> TurnInfo { get; }

	ObservableReference<IReadOnlyList<ExtraTurn>> ExtraTurns { get; }

	uint EventTranslationTurnNumber { get; }
}
