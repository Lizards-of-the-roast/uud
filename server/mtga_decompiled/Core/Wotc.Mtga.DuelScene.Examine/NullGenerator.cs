using System;
using System.Collections.Generic;
using GreClient.CardData;

namespace Wotc.Mtga.DuelScene.Examine;

public class NullGenerator : IContextualModelGenerator
{
	public static readonly IContextualModelGenerator Default = new NullGenerator();

	public IReadOnlyList<ICardDataAdapter> GenerateContextualModels(ICardDataAdapter source, ExamineState examineState = ExamineState.None)
	{
		return Array.Empty<ICardDataAdapter>();
	}
}
