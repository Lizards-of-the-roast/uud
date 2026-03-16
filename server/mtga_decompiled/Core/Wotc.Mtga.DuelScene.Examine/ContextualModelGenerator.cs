using System;
using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.DuelScene.Examine;

public class ContextualModelGenerator : IContextualModelGenerator
{
	private readonly IContextualModelGenerator _printingWithMutationsGenerator;

	private readonly IContextualModelGenerator _specializeGenerator;

	public ContextualModelGenerator(ICardDatabaseAdapter cardDatabase)
	{
		_printingWithMutationsGenerator = new PerpetualChangesContextualDecorator(new MutationGenerator(cardDatabase), cardDatabase);
		_specializeGenerator = new PerpetualChangesContextualDecorator(new SpecializeGenerator(cardDatabase.CardDataProvider), cardDatabase);
	}

	public IReadOnlyList<ICardDataAdapter> GenerateContextualModels(ICardDataAdapter source, ExamineState examineState = ExamineState.None)
	{
		return examineState switch
		{
			ExamineState.PrintingWithMutations => _printingWithMutationsGenerator.GenerateContextualModels(source, examineState), 
			ExamineState.Specialize => _specializeGenerator.GenerateContextualModels(source, examineState), 
			_ => Array.Empty<ICardDataAdapter>(), 
		};
	}
}
