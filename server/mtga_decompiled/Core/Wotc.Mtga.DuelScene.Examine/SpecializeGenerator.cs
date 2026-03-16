using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Examine;

public class SpecializeGenerator : IContextualModelGenerator
{
	private readonly ICardDataProvider _cardDatabase;

	private readonly List<ICardDataAdapter> _models = new List<ICardDataAdapter>();

	public SpecializeGenerator(ICardDataProvider cardDatabase)
	{
		_cardDatabase = cardDatabase ?? new NullCardDataProvider();
	}

	public IReadOnlyList<ICardDataAdapter> GenerateContextualModels(ICardDataAdapter source, ExamineState examineState = ExamineState.None)
	{
		_models.Clear();
		IReadOnlyCollection<CardPrintingData> linkedFacePrintings = source.LinkedFacePrintings;
		if (source.LinkedFaceType != LinkedFace.SpecializeChild)
		{
			MtgCardInstance instance = source.Instance;
			linkedFacePrintings = _cardDatabase.GetCardPrintingById(instance.BaseGrpId).LinkedFacePrintings;
		}
		foreach (CardPrintingData item in linkedFacePrintings)
		{
			if (item.LinkedFaceType == LinkedFace.SpecializeParent)
			{
				MtgCardInstance mtgCardInstance = item.CreateInstance();
				mtgCardInstance.InstanceId = source.InstanceId;
				mtgCardInstance.Zone = new MtgZone
				{
					Type = ZoneType.Library,
					Owner = MtgPlayer.DummyLocal
				};
				mtgCardInstance.Visibility = Visibility.None;
				mtgCardInstance.SkinCode = source.SkinCode;
				_models.Add(new CardData(mtgCardInstance, item));
			}
		}
		return _models;
	}
}
