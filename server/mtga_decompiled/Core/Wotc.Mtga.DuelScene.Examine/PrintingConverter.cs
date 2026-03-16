using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Examine;

public class PrintingConverter : IModelConverter
{
	private readonly ICardDataProvider _cardDatabase;

	public PrintingConverter(ICardDataProvider cardDatabase)
	{
		_cardDatabase = cardDatabase ?? NullCardDataProvider.Default;
	}

	public ICardDataAdapter ConvertModel(ICardDataAdapter sourceModel, ExamineState examineState = ExamineState.None)
	{
		MtgCardInstance mtgCardInstance = sourceModel?.Instance;
		if (mtgCardInstance == null)
		{
			return null;
		}
		string skinCode = mtgCardInstance.SkinCode;
		string baseSkinCode = mtgCardInstance.BaseSkinCode;
		if (TryGetPendingSpecializationGrpId(mtgCardInstance, out var specializeGrpId))
		{
			return GetConvertedModel(specializeGrpId, mtgCardInstance.Parent.ObjectType, skinCode, baseSkinCode);
		}
		if (IsAbilityWithParent(mtgCardInstance))
		{
			if (mtgCardInstance.Parent.CatalogId == WellKnownCatalogId.WellKnownCatalogId_TransientEffect)
			{
				return GetConvertedModel(mtgCardInstance.Parent.BaseGrpId, (mtgCardInstance.Parent.ObjectType == GameObjectType.Ability) ? GameObjectType.Card : mtgCardInstance.Parent.ObjectType, skinCode, baseSkinCode);
			}
			return GetConvertedModel(mtgCardInstance.Parent.BaseGrpId, mtgCardInstance.Parent.ObjectType, skinCode, baseSkinCode);
		}
		if (ShouldUseLinkedGrpId(sourceModel, out var linkedGrpId))
		{
			return GetConvertedModel(linkedGrpId, GameObjectType.Card, skinCode, baseSkinCode);
		}
		return GetConvertedModel(mtgCardInstance.BaseGrpId, mtgCardInstance.ObjectType, skinCode, baseSkinCode);
	}

	private bool TryGetPendingSpecializationGrpId(MtgCardInstance card, out uint specializeGrpId)
	{
		if (card != null && card.Parent.TryGetPendingSpecializationPrinting(_cardDatabase, out var specializationPrinting))
		{
			specializeGrpId = specializationPrinting.GrpId;
			return true;
		}
		specializeGrpId = 0u;
		return false;
	}

	private bool IsAbilityWithParent(MtgCardInstance srcInstance)
	{
		if (srcInstance.ObjectType == GameObjectType.Ability)
		{
			return srcInstance.Parent != null;
		}
		return false;
	}

	private bool ShouldUseLinkedGrpId(ICardDataAdapter sourceModel, out uint linkedGrpId)
	{
		linkedGrpId = 0u;
		if (!CardUtilities.IsParentFacet(sourceModel.LinkedFaceType))
		{
			return false;
		}
		if (sourceModel.LinkedFaceGrpIds.Count == 1)
		{
			linkedGrpId = sourceModel.LinkedFaceGrpIds[0];
			return true;
		}
		if (_cardDatabase.TryGetCardPrintingById(sourceModel.GrpId, out var card) && card.LinkedFaceGrpIds.Count == 1)
		{
			linkedGrpId = card.LinkedFaceGrpIds[0];
			return true;
		}
		return false;
	}

	private ICardDataAdapter GetConvertedModel(uint baseGrpId, GameObjectType objectType, string skinCode, string baseSkinCode)
	{
		skinCode = (string.IsNullOrEmpty(skinCode) ? baseSkinCode : skinCode);
		CardPrintingData cardPrintingById = _cardDatabase.GetCardPrintingById(baseGrpId, skinCode);
		MtgCardInstance mtgCardInstance = cardPrintingById.CreateInstance(objectType);
		mtgCardInstance.BaseSkinCode = baseSkinCode;
		mtgCardInstance.SkinCode = skinCode;
		mtgCardInstance.Visibility = Visibility.None;
		return new CardData(mtgCardInstance, cardPrintingById);
	}
}
