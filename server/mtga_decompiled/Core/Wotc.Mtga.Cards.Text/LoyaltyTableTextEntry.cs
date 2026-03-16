using System.Linq;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Cards.Text;

public class LoyaltyTableTextEntry : ILoyaltyTextEntry, ICardTextEntry, ITableTextEntry
{
	private readonly string _loyaltyText;

	private readonly LoyaltyValence _loyaltyValence;

	private readonly string _preamble;

	private readonly CardUtilities.RollTableEntry[] _rows;

	public string Preamble => _preamble;

	public CardUtilities.RollTableEntry[] Rows => _rows;

	public static bool TryParse(AbilityTextData abilityTextData, DieRollResultData? dieRoll, CardTextColorSettings colorSettings, out LoyaltyTableTextEntry textEntry)
	{
		return TryParse(abilityTextData.Printing, abilityTextData.FormattedLocalizedText, dieRoll, colorSettings, out textEntry, abilityTextData.State);
	}

	public static bool TryParse(AbilityPrintingData abilityPrinting, string formattedText, DieRollResultData? dieRoll, CardTextColorSettings colorSettings, out LoyaltyTableTextEntry textEntry, AbilityState abilityState = AbilityState.Normal)
	{
		textEntry = null;
		if ((abilityPrinting?.ReferencedAbilityTypes.Contains(AbilityType.RollDice)).Value && !string.IsNullOrEmpty(abilityPrinting?.LoyaltyCost.RawText) && CardUtilities.TryExtractRollTableFromAbility(formattedText, out var preambleAbilityText, out var rollTable))
		{
			textEntry = new LoyaltyTableTextEntry(abilityPrinting.LoyaltyCost.Value.ToString(), preambleAbilityText, rollTable, dieRoll, colorSettings, abilityState);
		}
		return textEntry != null;
	}

	private LoyaltyTableTextEntry(string loyalty, string preamble, CardUtilities.RollTableEntry[] rows, DieRollResultData? dieRoll, CardTextColorSettings colorSettings, AbilityState abilityState)
	{
		_loyaltyText = loyalty;
		_loyaltyValence = LoyaltyTextEntry.CalcValence(loyalty);
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
		return _preamble;
	}

	public string GetCost()
	{
		return _loyaltyText;
	}

	public LoyaltyValence GetValence()
	{
		return _loyaltyValence;
	}
}
