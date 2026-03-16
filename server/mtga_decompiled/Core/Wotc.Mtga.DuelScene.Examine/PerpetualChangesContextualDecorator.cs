using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.DuelScene.Examine;

public class PerpetualChangesContextualDecorator : IContextualModelGenerator
{
	private readonly IContextualModelGenerator _generator;

	private PerpetualChanges _perpetualChanges;

	private readonly List<ICardDataAdapter> _models = new List<ICardDataAdapter>();

	public PerpetualChangesContextualDecorator(IContextualModelGenerator generator, ICardDatabaseAdapter cardDatabase)
	{
		_generator = generator ?? new NullGenerator();
		_perpetualChanges = new PerpetualChanges(cardDatabase);
	}

	public IReadOnlyList<ICardDataAdapter> GenerateContextualModels(ICardDataAdapter sourceModel, ExamineState examineState = ExamineState.None)
	{
		IReadOnlyList<ICardDataAdapter> readOnlyList = _generator.GenerateContextualModels(sourceModel, examineState);
		if (readOnlyList == null)
		{
			return null;
		}
		if (sourceModel.HasPerpetualChanges())
		{
			_models.Clear();
			for (int i = 0; i < readOnlyList.Count; i++)
			{
				ICardDataAdapter convertedModel = readOnlyList[i];
				convertedModel = _perpetualChanges.ApplyPerpetualChanges(sourceModel, convertedModel);
				_models.Add(convertedModel);
			}
			return _models;
		}
		return readOnlyList;
	}
}
