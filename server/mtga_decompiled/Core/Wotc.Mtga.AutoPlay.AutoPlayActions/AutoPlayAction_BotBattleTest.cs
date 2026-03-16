using System.Collections.Generic;
using System.IO;
using Core.BI;
using Wizards.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public class AutoPlayAction_BotBattleTest : AutoPlayAction
{
	private BotBattleDSConfig _dsConfig;

	private string _playerDeck;

	private string _opponentDeck;

	protected override void OnInitialize(in string[] parameters, int index)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		for (int i = index + 1; i < parameters.Length; i++)
		{
			string[] array = parameters[i].Split(':');
			dictionary[array[0]] = array[1];
		}
		_dsConfig = new BotBattleDSConfig
		{
			FileName = dictionary.GetValueOrDefault("fileName"),
			SessionType = dictionary.GetValueOrDefault("sessionType").IntoEnum<BotBattleSessionType>(),
			OpponentStrategy = dictionary.GetValueOrDefault("opponentStrategy").IntoEnum<BotBattleStrategyType>(),
			LocalPlayerStrategy = dictionary.GetValueOrDefault("localPlayerStrategy").IntoEnum<BotBattleStrategyType>(),
			MatchesToPlay = dictionary.GetValueOrDefault("matchesToPlay").IntoInt()
		};
		_playerDeck = dictionary.GetValueOrDefault("playerDeck");
		_opponentDeck = dictionary.GetValueOrDefault("opponentDeck");
	}

	private static List<List<uint>> CardsToTest(ICardDatabaseAdapter cardDatabase, string fileName)
	{
		List<List<uint>> list = new List<List<uint>>();
		if (!string.IsNullOrWhiteSpace(fileName))
		{
			string path = Path.Combine(new DirectoryInfo(AutoPlayManager.GetConfigRoot).ToString(), fileName);
			if (File.Exists(path))
			{
				string text = File.ReadAllText(path);
				if (!string.IsNullOrWhiteSpace(text) && DeckCollection.CreateDeckFromText(cardDatabase, text, out var deck))
				{
					list.Add(deck.mainDeckCards);
				}
			}
		}
		return list;
	}

	protected override void OnExecute()
	{
		_dsConfig.LocalPlayerCardsToTest = CardsToTest(base._cardDatabase, _playerDeck);
		_dsConfig.OpponentCardsToTest = CardsToTest(base._cardDatabase, _opponentDeck);
		if (_dsConfig.LocalPlayerCardsToTest.Count == 0)
		{
			_dsConfig.LocalPlayerCardsToTest.Add(BotBattleConfig_DeckTest.GenerateRandomDeckFromSets(base._cardDatabase.DatabaseUtilities, string.Empty));
		}
		if (_dsConfig.OpponentCardsToTest.Count == 0)
		{
			_dsConfig.OpponentCardsToTest.Add(BotBattleConfig_DeckTest.GenerateRandomDeckFromSets(base._cardDatabase.DatabaseUtilities, string.Empty));
		}
		BIEventType.EnteringGame.SendWithDefaults(("Type", "Autoplay"));
		BIEventType.PreparingAssetsEnd.SendWithDefaults();
		BotBattleScene.Load(_dsConfig);
		Complete("Loaded DS BotBattle");
	}
}
