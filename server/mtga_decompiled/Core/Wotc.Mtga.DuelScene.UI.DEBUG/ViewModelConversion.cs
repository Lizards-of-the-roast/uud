using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GreClient.Network;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public static class ViewModelConversion
{
	public static MatchConfigEditor.ViewModel ConvertToViewModel(this MatchConfig matchConfig, string myPlayerId, IReadOnlyDictionary<string, IReadOnlyList<DeckConfig>> deckOptions, IReadOnlyList<BattlefieldData> battlefieldOptions, IReadOnlyList<EmblemData> emblemOptions, IReadOnlyList<string> avatarOptions, IReadOnlyList<string> sleeveOptions, IReadOnlyList<(string petId, string variantId)> petOptions, IReadOnlyDictionary<DeckConfig, IReadOnlyList<CardStyle>> cardStyleOptions, IReadOnlyList<string> titleOptions)
	{
		return new MatchConfigEditor.ViewModel(matchConfig.Name, new BattlefieldSelectionEditor.ViewModel(matchConfig.BattlefieldSelection, battlefieldOptions), matchConfig.GameType, matchConfig.GameVariant, matchConfig.WinCondition, matchConfig.MulliganType, matchConfig.UseSpecifiedSeed, matchConfig.RngSeed, matchConfig.FreeMulligans, matchConfig.MaxHandSize, matchConfig.ShuffleRestriction, matchConfig.Timers, matchConfig.LandsPerTurn, matchConfig.Teams.ConvertToViewModel(myPlayerId, deckOptions, emblemOptions, avatarOptions, sleeveOptions, petOptions, cardStyleOptions, titleOptions));
	}

	internal static MatchConfigEditor.ViewModel ConvertToSparseModel(this MatchConfig self, string playerId)
	{
		return self.ConvertToViewModel(playerId, new Dictionary<string, IReadOnlyList<DeckConfig>>(), new List<BattlefieldData>(), new List<EmblemData>(), new List<string>(), new List<string>(), new List<(string, string)>(), new Dictionary<DeckConfig, IReadOnlyList<CardStyle>>(), new List<string>());
	}

	private static TeamConfigListEditor.ViewModel ConvertToViewModel(this IReadOnlyList<TeamConfig> teams, string myPlayerId, IReadOnlyDictionary<string, IReadOnlyList<DeckConfig>> deckOptions, IReadOnlyList<EmblemData> emblemOptions, IReadOnlyList<string> avatarOptions, IReadOnlyList<string> sleeveOptions, IReadOnlyList<(string petId, string variantId)> petOptions, IReadOnlyDictionary<DeckConfig, IReadOnlyList<CardStyle>> cardStyleOptions, IReadOnlyList<string> titleOptions)
	{
		uint playerId = 0u;
		List<TeamConfigEditor.ViewModel> list = new List<TeamConfigEditor.ViewModel>();
		for (int i = 0; i < teams.Count; i++)
		{
			list.Add(teams[i].ConvertToViewModel(ref playerId, $"Team {i + 1}", myPlayerId, deckOptions, emblemOptions, avatarOptions, sleeveOptions, petOptions, cardStyleOptions, titleOptions));
		}
		return new TeamConfigListEditor.ViewModel(list, myPlayerId, deckOptions, cardStyleOptions, emblemOptions, avatarOptions, sleeveOptions, petOptions, titleOptions);
	}

	private static TeamConfigEditor.ViewModel ConvertToViewModel(this TeamConfig teamConfig, ref uint playerId, string teamName, string myPlayerId, IReadOnlyDictionary<string, IReadOnlyList<DeckConfig>> deckOptions, IReadOnlyList<EmblemData> emblemOptions, IReadOnlyList<string> avatarOptions, IReadOnlyList<string> sleeveOptions, IReadOnlyList<(string petId, string variantId)> petOptions, IReadOnlyDictionary<DeckConfig, IReadOnlyList<CardStyle>> cardStyleOptions, IReadOnlyList<string> titleOptions)
	{
		return new TeamConfigEditor.ViewModel(teamName, teamConfig.Players.ConvertToViewModel(myPlayerId, deckOptions, emblemOptions, avatarOptions, sleeveOptions, petOptions, cardStyleOptions, titleOptions));
	}

	private static PlayerConfigListEditor.ViewModel ConvertToViewModel(this IReadOnlyList<PlayerConfig> players, string myPlayerId, IReadOnlyDictionary<string, IReadOnlyList<DeckConfig>> deckOptions, IReadOnlyList<EmblemData> emblemOptions, IReadOnlyList<string> avatarOptions, IReadOnlyList<string> sleeveOptions, IReadOnlyList<(string petId, string variantId)> petOptions, IReadOnlyDictionary<DeckConfig, IReadOnlyList<CardStyle>> cardStyleOptions, IReadOnlyList<string> titleOptions)
	{
		List<PlayerConfigEditor.ViewModel> list = new List<PlayerConfigEditor.ViewModel>();
		foreach (PlayerConfig player in players)
		{
			PlayerConfig playerConfig = player;
			EmblemConfigListEditor.ViewModel emblems = player.Emblems.ConvertToViewModel(emblemOptions);
			IReadOnlyList<CardStyle> cardStyleOptions2;
			if (!cardStyleOptions.TryGetValue(player.Deck, out var value))
			{
				IReadOnlyList<CardStyle> readOnlyList = Array.Empty<CardStyle>();
				cardStyleOptions2 = readOnlyList;
			}
			else
			{
				cardStyleOptions2 = value;
			}
			list.Add(playerConfig.ConvertToViewModel(myPlayerId, deckOptions, emblems, avatarOptions, sleeveOptions, petOptions, cardStyleOptions2, titleOptions));
		}
		return new PlayerConfigListEditor.ViewModel(list, myPlayerId, deckOptions, cardStyleOptions, emblemOptions, sleeveOptions, sleeveOptions, petOptions, titleOptions);
	}

	private static PlayerConfigEditor.ViewModel ConvertToViewModel(this PlayerConfig playerConfig, string myPlayerId, IReadOnlyDictionary<string, IReadOnlyList<DeckConfig>> deckOptions, EmblemConfigListEditor.ViewModel emblems, IReadOnlyList<string> avatarOptions, IReadOnlyList<string> sleeveOptions, IReadOnlyList<(string petId, string variantId)> petOptions, IReadOnlyList<CardStyle> cardStyleOptions, IReadOnlyList<string> titleOptions)
	{
		return new PlayerConfigEditor.ViewModel(playerConfig.Name, playerConfig.PlayerType, (playerConfig.PlayerType == PlayerType.Human) ? playerConfig.PlayerId : myPlayerId, playerConfig.Deck.ConvertToViewModel(playerConfig.DeckDirectory, deckOptions), playerConfig.CardStyles.ConvertToViewModel(cardStyleOptions), playerConfig.FamiliarStrategy, playerConfig.Avatar, playerConfig.Sleeve, playerConfig.Pet, playerConfig.Title, playerConfig.Rank.ConvertToViewModel(), playerConfig.ShuffleRestriction, playerConfig.StartingLife, playerConfig.StartingHandSize, playerConfig.TreeOfCongress, playerConfig.StartingPlayer, emblems, myPlayerId, avatarOptions, sleeveOptions, petOptions, titleOptions);
	}

	internal static DeckConfigEditor.ViewModel ConvertToSparseModel(this DeckConfig deck)
	{
		return deck.ConvertToViewModel("", new Dictionary<string, IReadOnlyList<DeckConfig>>());
	}

	private static DeckConfigEditor.ViewModel ConvertToViewModel(this DeckConfig playerDeck, string deckDirectory, IReadOnlyDictionary<string, IReadOnlyList<DeckConfig>> deckOptions)
	{
		string text = (deckOptions.ContainsKey(deckDirectory) ? deckDirectory : string.Empty);
		IReadOnlyList<DeckConfig> readOnlyList2;
		if (!deckOptions.TryGetValue(text, out var value))
		{
			IReadOnlyList<DeckConfig> readOnlyList = Array.Empty<DeckConfig>();
			readOnlyList2 = readOnlyList;
		}
		else
		{
			readOnlyList2 = value;
		}
		IReadOnlyList<DeckConfig> readOnlyList3 = readOnlyList2;
		DeckConfig selectedDeck = ((!readOnlyList3.Contains(playerDeck) && deckOptions.Count > 0) ? readOnlyList3[0] : playerDeck);
		return new DeckConfigEditor.ViewModel(text, selectedDeck, deckOptions);
	}

	public static string ConvertIdsToCardNames(this DeckConfig deck, ICardDataProvider cardDataProvider, ICardTitleProvider cardTitleProvider)
	{
		StringBuilder parsedList = new StringBuilder();
		string value = DeckCollection.ConvertGrpIdsToCardList(cardDataProvider, cardTitleProvider, deck.Deck);
		parsedList.Append(value);
		AppendIfExists("Sideboard", deck.Sideboard);
		AppendIfExists("Commander", deck.Commander);
		AppendIfExists("Companion", new List<uint> { deck.Companion });
		return parsedList.ToString();
		void AppendIfExists(string sectionName, IReadOnlyList<uint> grpids)
		{
			if (grpids.Count >= 1 && (grpids.Count != 1 || grpids[0] != 0))
			{
				parsedList.AppendLine("\n" + sectionName);
				string value2 = DeckCollection.ConvertGrpIdsToCardList(cardDataProvider, cardTitleProvider, grpids);
				parsedList.AppendLine(value2);
			}
		}
	}

	private static CardStyleListEditor.ViewModel ConvertToViewModel(this IReadOnlyList<(uint grpId, string style)> styles, IReadOnlyList<CardStyle> allStyles)
	{
		List<CardStyle> list = new List<CardStyle>();
		List<CardStyle> list2 = new List<CardStyle>(allStyles);
		foreach (var style in styles)
		{
			foreach (CardStyle allStyle in allStyles)
			{
				if (allStyle.GrpId == style.grpId && !(allStyle.Style != style.style))
				{
					list.Add(allStyle);
					list2.Remove(allStyle);
					break;
				}
			}
		}
		List<CardStyleEditor.ViewModel> list3 = new List<CardStyleEditor.ViewModel>();
		foreach (CardStyle item in list)
		{
			list3.Add(item.ConvertToViewModel(list2));
		}
		return new CardStyleListEditor.ViewModel(list3, list2);
	}

	private static CardStyleEditor.ViewModel ConvertToViewModel(this CardStyle style, IEnumerable<CardStyle> unselected)
	{
		List<CardStyle> list = new List<CardStyle> { style };
		list.AddRange(unselected);
		return new CardStyleEditor.ViewModel(style, list);
	}

	private static EmblemConfigListEditor.ViewModel ConvertToViewModel(this IReadOnlyList<uint> emblemIds, IReadOnlyList<EmblemData> emblemOptions)
	{
		List<EmblemConfigEditor.ViewModel> list = new List<EmblemConfigEditor.ViewModel>();
		foreach (uint emblemId in emblemIds)
		{
			list.Add(new EmblemConfigEditor.ViewModel(GetSelectedEmblem(emblemId, emblemOptions), emblemOptions));
		}
		return new EmblemConfigListEditor.ViewModel(list, emblemOptions);
	}

	private static RankConfigEditor.ViewModel ConvertToViewModel(this RankConfig rankConfig)
	{
		return new RankConfigEditor.ViewModel((RankingClassType)rankConfig.Class, rankConfig.Tier, rankConfig.MythicPercent, rankConfig.MythicPlacement);
	}

	private static EmblemData GetSelectedEmblem(uint grpId, IEnumerable<EmblemData> emblems)
	{
		foreach (EmblemData emblem in emblems)
		{
			if (emblem.Id == grpId)
			{
				return emblem;
			}
		}
		return new EmblemData(0u, "NULL", "NO DESCRIPTION");
	}

	public static MatchConfig ConvertFromViewModel(this MatchConfigEditor.ViewModel viewModel)
	{
		string selectedBattlefield = viewModel.BattlefieldSelection.SelectedBattlefield;
		List<TeamConfig> list = new List<TeamConfig>();
		foreach (TeamConfigEditor.ViewModel config in viewModel.Teams.Configs)
		{
			list.Add(config.ConvertFromViewModel());
		}
		return new MatchConfig(viewModel.Name, 1u, selectedBattlefield, viewModel.GameType, viewModel.GameVariant, viewModel.WinCondition, viewModel.MulliganType, viewModel.UseSpecifiedSeed, viewModel.RngSeed, viewModel.FreeMulligans, viewModel.MaxHandSize, viewModel.ShuffleRestriction, viewModel.Timers, viewModel.LandsPerTurn, list);
	}

	private static TeamConfig ConvertFromViewModel(this TeamConfigEditor.ViewModel viewModel)
	{
		List<PlayerConfig> list = new List<PlayerConfig>();
		foreach (PlayerConfigEditor.ViewModel config in viewModel.Players.Configs)
		{
			list.Add(config.ConvertFromViewModel());
		}
		return new TeamConfig(list);
	}

	internal static PlayerConfig ConvertFromViewModel(this PlayerConfigEditor.ViewModel viewModel)
	{
		return new PlayerConfig(viewModel.Name, viewModel.PlayerType, viewModel.PlayerId, viewModel.SelectedDeck, viewModel.Styles.ConvertFromViewModel(), viewModel.FamiliarStrategy, viewModel.ShuffleRestriction, viewModel.StartingLife, viewModel.StartingHandSize, viewModel.TreeOfCongress, viewModel.StartingPlayer, viewModel.Emblems.ConvertFromViewModel(), viewModel.SelectedDeckDirectory, viewModel.SelectedAvatar, viewModel.SelectedSleeve, viewModel.SelectedPet, viewModel.SelectedTitle, viewModel.Rank.ConvertFromViewModel());
	}

	private static IReadOnlyList<(uint grpId, string style)> ConvertFromViewModel(this CardStyleListEditor.ViewModel viewModel)
	{
		if (viewModel.Styles.Count == 0)
		{
			return Array.Empty<(uint, string)>();
		}
		List<(uint, string)> list = new List<(uint, string)>();
		foreach (CardStyleEditor.ViewModel style in viewModel.Styles)
		{
			list.Add((style.Style.GrpId, style.Style.Style));
		}
		return list;
	}

	private static IReadOnlyList<uint> ConvertFromViewModel(this EmblemConfigListEditor.ViewModel viewModel)
	{
		if (viewModel.Emblems.Count == 0)
		{
			return Array.Empty<uint>();
		}
		List<uint> list = new List<uint>();
		foreach (EmblemConfigEditor.ViewModel emblem in viewModel.Emblems)
		{
			list.Add(emblem.Emblem.Id);
		}
		return list;
	}

	private static RankConfig ConvertFromViewModel(this RankConfigEditor.ViewModel viewModel)
	{
		return new RankConfig((int)viewModel.RankClass, viewModel.Tier, viewModel.MythicPercent, viewModel.MythicPlacement);
	}
}
