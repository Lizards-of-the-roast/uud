using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Examine;

public class MutationPrintingConverter : IModelConverter
{
	public readonly ICardDatabaseAdapter _cardDatabase;

	public MutationPrintingConverter(ICardDatabaseAdapter cardDatabase)
	{
		_cardDatabase = cardDatabase;
	}

	public ICardDataAdapter ConvertModel(ICardDataAdapter sourceModel, ExamineState examineState = ExamineState.None)
	{
		if (sourceModel == null)
		{
			return null;
		}
		MtgCardInstance instance = sourceModel.Instance;
		if (instance == null)
		{
			return null;
		}
		if (instance.FaceDownState.ReasonFaceDown == ReasonFaceDown.Morph)
		{
			return CardDataExtensions.CreateFaceDown(instance, _cardDatabase);
		}
		CardPrintingData cardPrintingById = _cardDatabase.CardDataProvider.GetCardPrintingById(instance.GrpId, instance.SkinCode);
		MtgCardInstance mtgCardInstance = cardPrintingById.CreateInstance(sourceModel.ObjectType);
		mtgCardInstance.InstanceId = GetInstanceId(instance);
		mtgCardInstance.BaseSkinCode = instance.BaseSkinCode;
		mtgCardInstance.SkinCode = instance.SkinCode;
		return new CardData(mtgCardInstance, cardPrintingById);
	}

	private uint GetInstanceId(MtgCardInstance srcInstance)
	{
		if (srcInstance.MutationChildren.Count > 0)
		{
			for (int i = 0; i < srcInstance.MutationChildren.Count; i++)
			{
				MtgCardInstance mtgCardInstance = srcInstance.MutationChildren[i];
				if (srcInstance.OverlayGrpId.HasValue && srcInstance.OverlayGrpId.Value == mtgCardInstance.GrpId)
				{
					return mtgCardInstance.InstanceId;
				}
			}
		}
		return srcInstance.InstanceId;
	}
}
