using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

public class DeckCostsDetails : MonoBehaviour
{
	[Serializable]
	public class CostBarItem
	{
		public RectTransform AvailableSize;

		public RectTransform Creatures;

		public RectTransform Others;

		public TMP_Text QuantityLabel;
	}

	[Serializable]
	public class TypeLineItem
	{
		public GameObject Root;

		public TMP_Text Quantity;

		public TMP_Text Percent;
	}

	private class Bucket
	{
		public CostBarItem Item;

		public uint Creatures;

		public uint Others;

		public uint Lands;

		public Bucket(CostBarItem item)
		{
			Item = item;
		}
	}

	[Header("Bars")]
	public CostBarItem OneOrLessItem;

	public CostBarItem TwoItem;

	public CostBarItem ThreeItem;

	public CostBarItem FourItem;

	public CostBarItem FiveItem;

	public CostBarItem SixOrGreaterItem;

	[Header("AverageCost")]
	public RectTransform AveragePlacement;

	public TMP_Text AverageText;

	[Header("Types")]
	public TypeLineItem CreaturesItem;

	public TypeLineItem OthersItem;

	public TypeLineItem LandsItem;

	public void SetDeck(IReadOnlyList<CardPrintingQuantity> deck)
	{
		Dictionary<int, Bucket> dictionary = new Dictionary<int, Bucket>
		{
			{
				1,
				new Bucket(OneOrLessItem)
			},
			{
				2,
				new Bucket(TwoItem)
			},
			{
				3,
				new Bucket(ThreeItem)
			},
			{
				4,
				new Bucket(FourItem)
			},
			{
				5,
				new Bucket(FiveItem)
			},
			{
				6,
				new Bucket(SixOrGreaterItem)
			}
		};
		uint num = 0u;
		uint num2 = 0u;
		uint num3 = 0u;
		uint num4 = 0u;
		uint num5 = 0u;
		uint num6 = 0u;
		foreach (CardPrintingQuantity item in deck)
		{
			bool flag = false;
			bool flag2 = false;
			foreach (CardType type in item.Printing.Types)
			{
				if (type == CardType.Creature)
				{
					flag = true;
				}
				if (type == CardType.Land)
				{
					flag2 = true;
				}
			}
			Bucket bucket = dictionary[Mathf.Clamp((int)item.Printing.ConvertedManaCost, 1, 6)];
			if (flag)
			{
				bucket.Creatures += item.Quantity;
				num3 += item.Quantity;
			}
			if (flag2)
			{
				bucket.Lands += item.Quantity;
				num5 += item.Quantity;
			}
			else
			{
				num += item.Quantity;
				num2 += item.Quantity * item.Printing.ConvertedManaCost;
			}
			if (!flag && !flag2)
			{
				bucket.Others += item.Quantity;
				num4 += item.Quantity;
			}
			num6 += item.Quantity;
		}
		uint num7 = dictionary.Values.Max((Bucket b) => b.Creatures + b.Others);
		foreach (Bucket value in dictionary.Values)
		{
			value.Item.QuantityLabel.text = (value.Creatures + value.Others).ToString("N0");
			float height = value.Item.AvailableSize.rect.height;
			float num8 = ((num7 == 0) ? 0f : ((float)value.Creatures / (float)num7));
			value.Item.Creatures.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num8 * height);
			float num9 = ((num7 == 0) ? 0f : ((float)value.Others / (float)num7));
			value.Item.Others.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num9 * height);
		}
		float num10 = ((num == 0) ? 0f : ((float)num2 / (float)num));
		AverageText.text = num10.ToString("N1");
		Vector3 position = AveragePlacement.position;
		position.x = Mathf.Lerp(OneOrLessItem.QuantityLabel.transform.position.x, SixOrGreaterItem.QuantityLabel.transform.position.x, Mathf.Clamp(num10 - 1f, 0f, 5f) / 5f);
		AveragePlacement.position = position;
		float num11 = ((num6 == 0) ? 0f : ((float)num3 / (float)num6));
		CreaturesItem.Percent.text = num11.ToString("P0");
		CreaturesItem.Quantity.text = num3.ToString("N0");
		float num12 = ((num6 == 0) ? 0f : ((float)num4 / (float)num6));
		OthersItem.Percent.text = num12.ToString("P0");
		OthersItem.Quantity.text = num4.ToString("N0");
		float num13 = ((num6 == 0) ? 0f : ((float)num5 / (float)num6));
		if (LandsItem.Percent != null)
		{
			LandsItem.Percent.text = num13.ToString("P0");
		}
		if (LandsItem.Quantity != null)
		{
			LandsItem.Quantity.text = num5.ToString("N0");
		}
	}
}
