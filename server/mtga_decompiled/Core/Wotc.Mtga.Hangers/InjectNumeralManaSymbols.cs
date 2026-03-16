using System.Collections.Generic;
using System.Linq;
using System.Text;
using GreClient.CardData;

namespace Wotc.Mtga.Hangers;

public class InjectNumeralManaSymbols : IParameterizedInjector, IUseParameterizedData
{
	private const string MANA_SYMBOLS = "{mana}";

	private string manaSymbolString = string.Empty;

	public string Inject(string value, ICardDataAdapter model, AbilityPrintingData ability)
	{
		if (manaSymbolString == string.Empty)
		{
			return value;
		}
		if (ability.BaseIdNumeral.HasValue && ability.BaseIdNumeral != 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("{");
			stringBuilder.Append(manaSymbolString);
			stringBuilder.Append("}");
			value = value.Replace("{mana}", string.Concat(Enumerable.Repeat(manaSymbolString, (int)ability.BaseIdNumeral.Value)));
		}
		return value;
	}

	public void SetData(IReadOnlyDictionary<string, string> data)
	{
		if (data.TryGetValue("Mana Symbol", out var value))
		{
			manaSymbolString = value;
		}
		else
		{
			manaSymbolString = string.Empty;
		}
	}
}
