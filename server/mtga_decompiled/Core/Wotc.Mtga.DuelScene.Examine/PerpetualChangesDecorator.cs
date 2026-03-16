using GreClient.CardData;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.DuelScene.Examine;

public class PerpetualChangesDecorator : IModelConverter
{
	private readonly ICardDatabaseAdapter _cardDataProvider;

	private readonly IModelConverter _converter;

	private readonly PerpetualChanges _perpetualChanges;

	public PerpetualChangesDecorator(IModelConverter converter, ICardDatabaseAdapter cardDatabase)
	{
		_cardDataProvider = cardDatabase ?? NullCardDatabaseAdapter.Default;
		_converter = converter ?? NullConverter.Default;
		_perpetualChanges = new PerpetualChanges(cardDatabase);
	}

	public ICardDataAdapter ConvertModel(ICardDataAdapter sourceModel, ExamineState examineState = ExamineState.None)
	{
		ICardDataAdapter cardDataAdapter = _converter.ConvertModel(sourceModel, examineState);
		if (cardDataAdapter == null)
		{
			return null;
		}
		if (PerpetualChangeUtilities.CanCopyPerpetualEffectsFromLinkedFace(cardDataAdapter))
		{
			sourceModel = sourceModel.GetLinkedFaceAtIndex(0, ignoreInstance: false, _cardDataProvider.CardDataProvider);
		}
		if (_perpetualChanges.CanApplyPerpetualChanges(sourceModel))
		{
			cardDataAdapter = _perpetualChanges.ApplyPerpetualChanges(sourceModel, cardDataAdapter);
			if (PerpetualChangeUtilities.CanCopyPerpetualEffectsToLinkedFace(sourceModel))
			{
				cardDataAdapter = _perpetualChanges.ApplyPerpetualChangesToLinkedFaces(sourceModel, cardDataAdapter);
			}
			return cardDataAdapter;
		}
		return cardDataAdapter;
	}
}
