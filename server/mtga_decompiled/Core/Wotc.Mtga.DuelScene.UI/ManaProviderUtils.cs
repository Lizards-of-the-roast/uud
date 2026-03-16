using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UI;

public static class ManaProviderUtils
{
	public static void Sort(ref List<ManaColorSelector.ManaProducedData> list, ICardDataAdapter model)
	{
		if (list.Count >= 5)
		{
			list.Sort((ManaColorSelector.ManaProducedData lhs, ManaColorSelector.ManaProducedData rhs) => lhs.PrimaryColor.CompareTo(rhs.PrimaryColor));
		}
		else if (model != null)
		{
			list.Sort(delegate(ManaColorSelector.ManaProducedData lhs, ManaColorSelector.ManaProducedData rhs)
			{
				CardColor value = CardUtilities.ConvertManaColorToCardColor(lhs.PrimaryColor);
				CardColor value2 = CardUtilities.ConvertManaColorToCardColor(rhs.PrimaryColor);
				int num = model.GetFrameColors.IndexOf(value);
				int value3 = model.GetFrameColors.IndexOf(value2);
				return num.CompareTo(value3);
			});
		}
	}
}
