using System.Collections.Generic;
using GreClient.CardData;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Cards.Database;

public class BotBattleConfig_DeckTest : BotBattleConfigView
{
	[SerializeField]
	private InputField _gamesToPlayInputField;

	[Space(10f)]
	[Header("LocalPlayer")]
	[SerializeField]
	private Toggle _localPlayerRandomizeToggle;

	[SerializeField]
	private Text _localPlayerDeckDescriptionText;

	[SerializeField]
	private InputField _localPlayerDeckInputField;

	[SerializeField]
	private Dropdown _localPlayerStrategyDropdown;

	[Space(10f)]
	[Header("Opponent")]
	[SerializeField]
	private Toggle _opponentRandomizeToggle;

	[SerializeField]
	private Text _opponentDeckDescriptionText;

	[SerializeField]
	private InputField _opponentDeckInputField;

	[SerializeField]
	private Dropdown _opponentStrategyDropdown;

	public override BotBattleSessionType PanelType { get; protected set; }

	private void Awake()
	{
		List<string> list = new List<string>();
		foreach (BotBattleStrategyType value in EnumHelper.GetValues(typeof(BotBattleStrategyType)))
		{
			list.Add(value.ToString());
		}
		_localPlayerRandomizeToggle.onValueChanged.AddListener(delegate(bool v)
		{
			_localPlayerDeckDescriptionText.text = (v ? "Sets to use..." : "Deck filepath");
			_localPlayerDeckInputField.text = "";
		});
		_localPlayerStrategyDropdown.AddOptions(list);
		_opponentRandomizeToggle.onValueChanged.AddListener(delegate(bool v)
		{
			_opponentDeckDescriptionText.text = (v ? "Sets to use..." : "Deck filepath");
			_opponentDeckInputField.text = "";
		});
		_opponentStrategyDropdown.AddOptions(list);
	}

	public override BotBattleDSConfig GetConfig()
	{
		BotBattleDSConfig botBattleDSConfig = new BotBattleDSConfig
		{
			SessionType = BotBattleSessionType.DeckTest
		};
		if (int.TryParse(_gamesToPlayInputField.text, out var result))
		{
			botBattleDSConfig.MatchesToPlay = result;
		}
		CardDatabase localCardDatabase = GetLocalCardDatabase();
		if (_localPlayerRandomizeToggle.isOn)
		{
			string text = _localPlayerDeckInputField.text;
			List<uint> item = GenerateRandomDeckFromSets(localCardDatabase.DatabaseUtilities, text);
			botBattleDSConfig.LocalPlayerCardsToTest = new List<List<uint>> { item };
		}
		botBattleDSConfig.LocalPlayerStrategy = (BotBattleStrategyType)_localPlayerStrategyDropdown.value;
		if (_opponentRandomizeToggle.isOn)
		{
			string text2 = _opponentDeckInputField.text;
			List<uint> item2 = GenerateRandomDeckFromSets(localCardDatabase.DatabaseUtilities, text2);
			botBattleDSConfig.OpponentCardsToTest = new List<List<uint>> { item2 };
		}
		botBattleDSConfig.OpponentStrategy = (BotBattleStrategyType)_opponentStrategyDropdown.value;
		return botBattleDSConfig;
	}

	public static List<uint> GenerateRandomDeckFromSets(IDatabaseUtilities cardDatabaseUtils, string setText)
	{
		List<string> list = (string.IsNullOrWhiteSpace(setText) ? new List<string>() : new List<string>(setText.Split(',')));
		List<CardPrintingData> list2 = new List<CardPrintingData>();
		foreach (CardPrintingData primaryPrinting in cardDatabaseUtils.GetPrimaryPrintings())
		{
			if (list.Count <= 0 || list.Contains(primaryPrinting.ExpansionCode))
			{
				list2.Add(primaryPrinting);
			}
		}
		return new RandomDeckGenerator(list2).GenerateRandomDeck();
	}
}
