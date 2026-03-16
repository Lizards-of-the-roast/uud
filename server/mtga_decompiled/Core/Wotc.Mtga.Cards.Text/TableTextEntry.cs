using System;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Cards.Text;

public class TableTextEntry : ITableTextEntry, ICardTextEntry
{
	private readonly string _preamble;

	private readonly CardUtilities.RollTableEntry[] _rows;

	public string Preamble => _preamble;

	public CardUtilities.RollTableEntry[] Rows => _rows;

	public static bool TryGetTableTextEntry(AbilityPrintingData abilityPrinting, string formattedText, DieRollResultData? dieRoll, CardTextColorSettings colorSettings, out TableTextEntry tableTextEntry, AbilityState abilityState = AbilityState.Normal)
	{
		tableTextEntry = null;
		if ((abilityPrinting?.ReferencedAbilityTypes.Contains(AbilityType.RollD20)).Value && CardUtilities.TryExtractRollTableFromAbility(formattedText, out var preambleAbilityText, out var rollTable))
		{
			tableTextEntry = new TableTextEntry(preambleAbilityText, rollTable, dieRoll, colorSettings, abilityState);
		}
		return tableTextEntry != null;
	}

	private TableTextEntry(string preamble, CardUtilities.RollTableEntry[] rows, DieRollResultData? dieRoll, CardTextColorSettings colorSettings, AbilityState abilityState)
	{
		_preamble = preamble;
		_rows = rows;
		for (int i = 0; i < rows.Length; i++)
		{
			CardUtilities.RollTableEntry other = rows[i];
			string formattedText = other.FormattedText;
			if (!dieRoll.HasValue)
			{
				switch (abilityState)
				{
				case AbilityState.Normal:
					formattedText = string.Format(colorSettings.DefaultFormat, other.FormattedText);
					break;
				case AbilityState.Added:
					formattedText = string.Format(colorSettings.AddedFormat, other.FormattedText);
					break;
				case AbilityState.Removed:
					formattedText = string.Format(colorSettings.RemovedFormat, other.FormattedText);
					break;
				}
			}
			else
			{
				formattedText = ((dieRoll.Value.Result < other.Min || dieRoll.Value.Result > other.Max) ? string.Format(colorSettings.RemovedFormat, other.FormattedText) : string.Format(colorSettings.DefaultFormat, other.FormattedText));
			}
			rows[i] = new CardUtilities.RollTableEntry(other)
			{
				FormattedText = formattedText
			};
		}
	}

	public string GetText()
	{
		throw new NotImplementedException();
	}
}
