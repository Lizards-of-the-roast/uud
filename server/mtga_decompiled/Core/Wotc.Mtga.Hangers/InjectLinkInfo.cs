using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.Hangers;

public class InjectLinkInfo : IParameterizedInjector
{
	private const string LINKED_INFO_ITEMS_STRING = "{linkedInfoItems}";

	private readonly IGreLocProvider _greLocProvider;

	public InjectLinkInfo(IGreLocProvider greLocProvider)
	{
		_greLocProvider = greLocProvider ?? NullGreLocManager.Default;
	}

	public string Inject(string value, ICardDataAdapter model, AbilityPrintingData ability)
	{
		List<string> list = new List<string>();
		foreach (LinkedInfoText item in model.LinkedInfoText)
		{
			list.Add(_greLocProvider.GetLocalizedTextForEnumValue(item.EnumName, item.Value));
			if (item.Highlighted)
			{
				list[list.Count - 1] = "<#FF9C01>" + list[list.Count - 1] + "</color>";
			}
		}
		string newValue = "\n" + string.Join(", ", list);
		return value.Replace("{linkedInfoItems}", newValue);
	}
}
