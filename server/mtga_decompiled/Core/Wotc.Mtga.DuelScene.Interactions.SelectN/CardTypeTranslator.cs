using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.SelectN;

public static class CardTypeTranslator
{
	public static IEnumerable<int> ConvertIdsToCardTypes(IReadOnlyCollection<uint> input)
	{
		if (input.Count > 0)
		{
			return FilterCardTypeIds(input);
		}
		return CardTypeEnumToIds();
	}

	private static IEnumerable<int> FilterCardTypeIds(IReadOnlyCollection<uint> ids)
	{
		foreach (uint id in ids)
		{
			if (id != 0)
			{
				yield return (int)id;
			}
		}
	}

	private static IEnumerable<int> CardTypeEnumToIds()
	{
		foreach (CardType value in EnumHelper.GetValues(typeof(CardType)))
		{
			if (value != CardType.None)
			{
				yield return (int)value;
			}
		}
	}
}
