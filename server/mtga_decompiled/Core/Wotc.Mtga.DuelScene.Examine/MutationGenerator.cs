using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Examine;

public class MutationGenerator : IContextualModelGenerator
{
	private const int MAX_MUTATIONS_SHOWN = 18;

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly List<ICardDataAdapter> _models = new List<ICardDataAdapter>();

	public MutationGenerator(ICardDatabaseAdapter cardDatabase)
	{
		_cardDatabase = cardDatabase;
	}

	public IReadOnlyList<ICardDataAdapter> GenerateContextualModels(ICardDataAdapter source, ExamineState examineState = ExamineState.None)
	{
		_models.Clear();
		bool flag = false;
		int num = Mathf.Min(source.Instance.MutationChildren.Count, 18);
		for (int i = 0; i < num; i++)
		{
			MtgCardInstance mtgCardInstance = source.Instance.MutationChildren[i];
			if (!flag && source.Instance.OverlayGrpId.HasValue && source.Instance.OverlayGrpId.Value == mtgCardInstance.GrpId)
			{
				flag = true;
			}
			else if (mtgCardInstance.FaceDownState.MutatedOverFaceDownReasonFaceDown != ReasonFaceDown.None || mtgCardInstance.FaceDownState.MutatedOverFaceDownSourceAbilityGrpId != 0)
			{
				MtgCardInstance faceDownExamineCopy = mtgCardInstance.GetFaceDownExamineCopy();
				_models.Add(CardDataExtensions.CreateBlankFaceDown(faceDownExamineCopy, _cardDatabase));
			}
			else
			{
				_models.Add(CardDataExtensions.CreateWithDatabase(mtgCardInstance.GetCopy(), _cardDatabase));
			}
		}
		if (num < 18 && source.Instance.OverlayGrpId.HasValue)
		{
			MtgCardInstance instance = source.Instance;
			if (source.Instance.BaseGrpId == 3 && (source.Instance.FaceDownState.MutatedOverFaceDownReasonFaceDown != ReasonFaceDown.None || source.Instance.FaceDownState.MutatedOverFaceDownSourceAbilityGrpId != 0))
			{
				MtgCardInstance faceDownExamineCopy2 = source.Instance.GetFaceDownExamineCopy();
				faceDownExamineCopy2.FaceDownState.SetReasonFaceDown(faceDownExamineCopy2.FaceDownState.MutatedOverFaceDownReasonFaceDown);
				faceDownExamineCopy2.FaceDownState.SetReasonFaceDownSourceAbilityGrpId(faceDownExamineCopy2.FaceDownState.MutatedOverFaceDownSourceAbilityGrpId);
				_models.Add(CardDataExtensions.CreateBlankFaceDown(faceDownExamineCopy2, _cardDatabase));
			}
			else
			{
				CardPrintingData cardPrintingById = _cardDatabase.CardDataProvider.GetCardPrintingById(instance.BaseGrpId, instance.BaseSkinCode);
				MtgCardInstance mtgCardInstance2 = cardPrintingById.CreateInstance();
				mtgCardInstance2.InstanceId = source.InstanceId;
				mtgCardInstance2.Zone = new MtgZone
				{
					Type = ZoneType.Library,
					Owner = MtgPlayer.DummyLocal
				};
				mtgCardInstance2.SkinCode = instance.BaseSkinCode;
				mtgCardInstance2.Visibility = Visibility.None;
				_models.Add(new CardData(mtgCardInstance2, cardPrintingById));
			}
		}
		return _models;
	}
}
