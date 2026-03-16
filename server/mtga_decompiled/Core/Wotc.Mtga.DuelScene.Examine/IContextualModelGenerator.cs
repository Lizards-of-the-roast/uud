using System.Collections.Generic;
using GreClient.CardData;

namespace Wotc.Mtga.DuelScene.Examine;

public interface IContextualModelGenerator
{
	IReadOnlyList<ICardDataAdapter> GenerateContextualModels(ICardDataAdapter source, ExamineState examineState = ExamineState.None);
}
