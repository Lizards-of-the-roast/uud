using GreClient.CardData;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.DuelScene.Examine;

public class ModelConverter : IModelConverter
{
	private readonly IModelConverter _instanceConverter = new InstanceConverter();

	private readonly IModelConverter _printingConverter;

	private readonly IModelConverter _mutationPrintingConverter;

	private readonly IModelConverter _stylesConverter;

	public ModelConverter(ICardDatabaseAdapter cardDatabase)
	{
		_printingConverter = new PerpetualChangesDecorator(new PrintingConverter(cardDatabase.CardDataProvider), cardDatabase);
		_mutationPrintingConverter = new PerpetualChangesDecorator(new MutationPrintingConverter(cardDatabase), cardDatabase);
		_stylesConverter = new StylesConverter(cardDatabase.CardDataProvider, cardDatabase.AbilityDataProvider);
	}

	public ICardDataAdapter ConvertModel(ICardDataAdapter sourceModel, ExamineState examineState = ExamineState.None)
	{
		return examineState switch
		{
			ExamineState.Unstyled => _stylesConverter.ConvertModel(sourceModel, examineState), 
			ExamineState.Styled => _instanceConverter.ConvertModel(sourceModel, examineState), 
			ExamineState.Instance => _instanceConverter.ConvertModel(sourceModel, examineState), 
			ExamineState.Specialize => _instanceConverter.ConvertModel(sourceModel, examineState), 
			ExamineState.Printing => _printingConverter.ConvertModel(sourceModel, examineState), 
			ExamineState.PrintingWithMutations => _mutationPrintingConverter.ConvertModel(sourceModel, examineState), 
			_ => NullConverter.Default.ConvertModel(sourceModel, examineState), 
		};
	}
}
