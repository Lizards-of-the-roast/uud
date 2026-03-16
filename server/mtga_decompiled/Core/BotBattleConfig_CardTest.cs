using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Cards.Database;

public class BotBattleConfig_CardTest : BotBattleConfigView
{
	[SerializeField]
	private Toggle _localPlayerToggle;

	[SerializeField]
	private Toggle _opponentToggle;

	[SerializeField]
	private InputField _pathInputField;

	public override BotBattleSessionType PanelType { get; protected set; } = BotBattleSessionType.CardTest;

	public override BotBattleDSConfig GetConfig()
	{
		BotBattleDSConfig botBattleDSConfig = new BotBattleDSConfig();
		botBattleDSConfig.SessionType = BotBattleSessionType.CardTest;
		string text = _pathInputField.text.Trim();
		text = text.Trim('"');
		if (!File.Exists(text))
		{
			return null;
		}
		string text2 = File.ReadAllText(text);
		if (string.IsNullOrEmpty(text2))
		{
			return null;
		}
		CardDatabase localCardDatabase = GetLocalCardDatabase();
		List<List<uint>> list = new List<List<uint>>
		{
			new List<uint>()
		};
		string[] array = Regex.Split(text2, "\r\n|\r|\n");
		for (int i = 0; i < array.Length; i++)
		{
			string text3 = array[i].Trim();
			if (text3 == "")
			{
				continue;
			}
			if (text3 == "---")
			{
				list.Add(new List<uint>());
				continue;
			}
			uint result = 0u;
			string title = text3.Trim();
			if (!uint.TryParse(text3, out result))
			{
				result = localCardDatabase.DatabaseUtilities.GetPrintingsByEnglishTitle(title).FirstOrDefault()?.GrpId ?? 0;
			}
			if (result != 0)
			{
				list[list.Count - 1].Add(result);
			}
		}
		botBattleDSConfig.LocalPlayerCardsToTest = (_localPlayerToggle.isOn ? list : new List<List<uint>>());
		botBattleDSConfig.OpponentCardsToTest = (_opponentToggle.isOn ? list : new List<List<uint>>());
		return botBattleDSConfig;
	}
}
