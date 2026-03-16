using System.Collections.Generic;
using System.Text;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class EmblemDataProvider : IEmblemDataProvider
{
	private readonly ICardDatabaseAdapter _cardDatabase;

	private IReadOnlyList<EmblemData> _allEmblems;

	public EmblemDataProvider(ICardDatabaseAdapter cardDatabase)
	{
		_cardDatabase = cardDatabase ?? NullCardDatabaseAdapter.Default;
	}

	public IReadOnlyList<EmblemData> GetAllEmblems()
	{
		return _allEmblems ?? (_allEmblems = new List<EmblemData>(LoadAllEmblems(_cardDatabase)));
	}

	private static IReadOnlyList<EmblemData> LoadAllEmblems(ICardDatabaseAdapter cardDatabase)
	{
		List<EmblemData> list = new List<EmblemData>();
		foreach (CardPrintingData item in cardDatabase.DatabaseUtilities.GetPrintingsByExpansion("ArenaSUP"))
		{
			if (!IgnoreThisPrintingData(item))
			{
				list.Add(new EmblemData(item.GrpId, cardDatabase.GreLocProvider.GetLocalizedText(item.TitleId, "en-US", formatted: false), GenerateDescriptionText(item, cardDatabase.GreLocProvider)));
			}
		}
		list.Sort(SortEmblemData);
		return list;
	}

	private static bool IgnoreThisPrintingData(CardPrintingData data)
	{
		if (data.ExpansionCode != "ArenaSUP")
		{
			return true;
		}
		if (data.Abilities.Exists((AbilityPrintingData ability) => ability.Category == AbilityCategory.Activated || ability.Category == AbilityCategory.ActivatedTest || ability.Category == AbilityCategory.Static || ability.Category == AbilityCategory.Triggered))
		{
			return false;
		}
		return true;
	}

	private static int SortEmblemData(EmblemData lhs, EmblemData rhs)
	{
		int num = lhs.Title.CompareTo(rhs.Title);
		if (num != 0)
		{
			return num;
		}
		num = lhs.Description.CompareTo(rhs.Description);
		if (num != 0)
		{
			return num;
		}
		return lhs.Id.CompareTo(rhs.Id);
	}

	private static string GenerateDescriptionText(CardPrintingData data, IGreLocProvider locProvider)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < data.Abilities.Count; i++)
		{
			stringBuilder.Append(locProvider.GetLocalizedText(data.Abilities[i].TextId));
			if (i < data.Abilities.Count - 1)
			{
				stringBuilder.AppendLine();
			}
		}
		return stringBuilder.ToString();
	}
}
