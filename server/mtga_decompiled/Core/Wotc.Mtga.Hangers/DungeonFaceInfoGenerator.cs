using System;
using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class DungeonFaceInfoGenerator : IFaceInfoGenerator
{
	private static uint[] _acererakDungeonGrpIds = new uint[3] { 78770u, 78768u, 78769u };

	private static uint[] _dungeonGrpids = new uint[3] { 78768u, 78769u, 78770u };

	private const uint ACERERAK_TITLE_ID = 498563u;

	private const uint GRPID_DUNGEON_PHANDELVER = 78768u;

	private const uint GRPID_DUNGEON_MADMAGE = 78769u;

	private const uint GRPID_DUNGEON_TOMB = 78770u;

	private readonly ICardDataProvider _cardDataProvider;

	private readonly IClientLocProvider _locManager;

	private readonly HashSet<FaceHanger.FaceCardInfo> _data = new HashSet<FaceHanger.FaceCardInfo>();

	public DungeonFaceInfoGenerator(ICardDataProvider cardDataProvider, IClientLocProvider locManager)
	{
		_cardDataProvider = cardDataProvider ?? new NullCardDataProvider();
		_locManager = locManager ?? NullLocProvider.Default;
	}

	public IReadOnlyCollection<FaceHanger.FaceCardInfo> GenerateFaceCardInfo(ICardDataAdapter cardData, ICardDataAdapter sourceModel)
	{
		_data.Clear();
		uint[] array = DungeonGrpIds(cardData);
		foreach (uint id in array)
		{
			CardPrintingData cardPrintingById = _cardDataProvider.GetCardPrintingById(id);
			if (cardPrintingById != null)
			{
				_data.Add(DungeonFaceHanger(cardPrintingById));
			}
		}
		return _data;
	}

	private FaceHanger.FaceCardInfo DungeonFaceHanger(CardPrintingData printing)
	{
		CardData cardData = new CardData(printing.CreateInstance(), printing);
		string localizedText = _locManager.GetLocalizedText("DuelScene/FaceHanger/Dungeon");
		FaceHanger.FaceHangerArrowData arrowData = new FaceHanger.FaceHangerArrowData(FaceHanger.FaceHangerArrowType.None);
		return new FaceHanger.FaceCardInfo(cardData, localizedText, arrowData, FaceHanger.HangerType.RelatedObj_Dungeon);
	}

	private uint[] DungeonGrpIds(ICardDataAdapter cardData)
	{
		if (VenturesIntoTheDungeon(cardData))
		{
			if (IsAcererak(cardData))
			{
				return _acererakDungeonGrpIds;
			}
			return _dungeonGrpids;
		}
		return Array.Empty<uint>();
	}

	private bool VenturesIntoTheDungeon(ICardDataAdapter cardData)
	{
		if (cardData == null)
		{
			return false;
		}
		foreach (AbilityPrintingData ability in cardData.Abilities)
		{
			foreach (AbilityType referencedAbilityType in ability.ReferencedAbilityTypes)
			{
				if (referencedAbilityType == AbilityType.VentureIntoTheDungeon)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool IsAcererak(ICardDataAdapter cardData)
	{
		if (cardData == null)
		{
			return false;
		}
		return cardData.TitleId == 498563;
	}
}
