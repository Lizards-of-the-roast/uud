using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Cards.Database;

public class BotBattleConfig_SetTest : BotBattleConfigView
{
	[SerializeField]
	private Toggle _localPlayerToggle;

	[SerializeField]
	private Toggle _opponentToggle;

	[SerializeField]
	private InputField _setsToTestInputField;

	public override BotBattleSessionType PanelType { get; protected set; } = BotBattleSessionType.SetTest;

	public override BotBattleDSConfig GetConfig()
	{
		BotBattleDSConfig botBattleDSConfig = new BotBattleDSConfig();
		botBattleDSConfig.SessionType = BotBattleSessionType.SetTest;
		List<string> list = new List<string>(_setsToTestInputField.text.Split(','));
		CardDatabase localCardDatabase = GetLocalCardDatabase();
		List<List<uint>> list2 = new List<List<uint>>();
		foreach (string item2 in list)
		{
			List<uint> item = (from x in localCardDatabase.DatabaseUtilities.GetPrintingsByExpansion(item2)
				select x.GrpId).ToList();
			list2.Add(item);
		}
		botBattleDSConfig.LocalPlayerCardsToTest = (_localPlayerToggle.isOn ? list2 : new List<List<uint>>());
		botBattleDSConfig.OpponentCardsToTest = (_opponentToggle.isOn ? list2 : new List<List<uint>>());
		return botBattleDSConfig;
	}
}
