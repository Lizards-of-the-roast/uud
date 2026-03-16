using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

public class DeckColorsDetails : MonoBehaviour
{
	[Serializable]
	public class ColorLineItem
	{
		public GameObject Root;

		public TMP_Text Quantity;

		public TMP_Text Percent;
	}

	public ColorLineItem WhiteItem;

	public ColorLineItem BlueItem;

	public ColorLineItem BlackItem;

	public ColorLineItem RedItem;

	public ColorLineItem GreenItem;

	public ColorLineItem MulticolorItem;

	public ColorLineItem ColorlessItem;

	public void SetDeck(IReadOnlyList<CardPrintingQuantity> deck)
	{
		uint num = 0u;
		uint num2 = 0u;
		uint num3 = 0u;
		uint num4 = 0u;
		uint num5 = 0u;
		uint num6 = 0u;
		uint num7 = 0u;
		uint num8 = 0u;
		foreach (CardPrintingQuantity item in deck)
		{
			if (item.Printing.Types.Contains(CardType.Land))
			{
				continue;
			}
			CardColorFlags colorFlags = item.Printing.ColorFlags;
			if (colorFlags == CardColorFlags.None)
			{
				num6 += item.Quantity;
			}
			else
			{
				if ((colorFlags & (colorFlags - 1)) != CardColorFlags.None)
				{
					num7 += item.Quantity;
				}
				if ((colorFlags & CardColorFlags.White) == CardColorFlags.White)
				{
					num += item.Quantity;
				}
				if ((colorFlags & CardColorFlags.Blue) == CardColorFlags.Blue)
				{
					num2 += item.Quantity;
				}
				if ((colorFlags & CardColorFlags.Black) == CardColorFlags.Black)
				{
					num3 += item.Quantity;
				}
				if ((colorFlags & CardColorFlags.Red) == CardColorFlags.Red)
				{
					num4 += item.Quantity;
				}
				if ((colorFlags & CardColorFlags.Green) == CardColorFlags.Green)
				{
					num5 += item.Quantity;
				}
			}
			num8 += item.Quantity;
		}
		UpdateItem(WhiteItem, num, num8);
		UpdateItem(BlueItem, num2, num8);
		UpdateItem(BlackItem, num3, num8);
		UpdateItem(RedItem, num4, num8);
		UpdateItem(GreenItem, num5, num8);
		UpdateItem(MulticolorItem, num7, num8);
		UpdateItem(ColorlessItem, num6, num8);
	}

	private void UpdateItem(ColorLineItem item, uint quantity, uint total)
	{
		if (quantity == 0)
		{
			item.Root.SetActive(value: false);
			return;
		}
		item.Root.SetActive(value: true);
		item.Quantity.text = quantity.ToString("N0");
		float num = ((total == 0) ? 0f : ((float)quantity / (float)total));
		item.Percent.text = num.ToString("0%");
	}
}
