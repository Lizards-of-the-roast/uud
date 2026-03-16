using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

public class DeckTypesDetails : MonoBehaviour
{
	private class TypeTracker
	{
		public readonly CardType Type;

		public readonly Dictionary<SubType, SubtypeTracker> Subtypes;

		public uint Quantity;

		public TypeTracker(CardType type)
		{
			Type = type;
			Subtypes = new Dictionary<SubType, SubtypeTracker>();
		}
	}

	private class SubtypeTracker
	{
		public readonly SubType Type;

		public readonly string LocalizedName;

		public uint Quantity;

		public SubtypeTracker(SubType type, IGreLocProvider locMan)
		{
			Type = type;
			LocalizedName = locMan.GetLocalizedTextForEnumValue("SubType", (int)type);
		}
	}

	public Transform ItemParent;

	public DeckDetailsLineItem TypePrefab;

	public DeckDetailsLineItem SubtypePrefab;

	public GameObject SmallSpacerPrefab;

	public GameObject LargeSpacerPrefab;

	private List<GameObject> _itemInstances = new List<GameObject>();

	public void SetDeck(IReadOnlyList<CardPrintingQuantity> deck, IGreLocProvider locMan)
	{
		foreach (GameObject itemInstance in _itemInstances)
		{
			Object.Destroy(itemInstance);
		}
		_itemInstances.Clear();
		Dictionary<CardType, TypeTracker> dictionary = new Dictionary<CardType, TypeTracker>();
		foreach (CardPrintingQuantity item in deck)
		{
			foreach (CardType type in item.Printing.Types)
			{
				if (!dictionary.TryGetValue(type, out var value))
				{
					dictionary.Add(type, value = new TypeTracker(type));
				}
				value.Quantity += item.Quantity;
				foreach (SubType subtype in item.Printing.Subtypes)
				{
					if (!value.Subtypes.TryGetValue(subtype, out var value2))
					{
						value.Subtypes.Add(subtype, value2 = new SubtypeTracker(subtype, locMan));
					}
					value2.Quantity += item.Quantity;
				}
			}
		}
		CardType cardType = CardType.None;
		CardType[] array = new CardType[8]
		{
			CardType.Creature,
			CardType.Instant,
			CardType.Sorcery,
			CardType.Artifact,
			CardType.Enchantment,
			CardType.Planeswalker,
			CardType.Battle,
			CardType.Land
		};
		foreach (CardType key in array)
		{
			if (dictionary.TryGetValue(key, out var value3))
			{
				switch (cardType)
				{
				case CardType.Creature:
					_itemInstances.Add(Object.Instantiate(LargeSpacerPrefab, ItemParent));
					break;
				default:
					_itemInstances.Add(Object.Instantiate(SmallSpacerPrefab, ItemParent));
					break;
				case CardType.None:
					break;
				}
				cardType = value3.Type;
				DisplayType(value3, locMan);
			}
		}
	}

	private void DisplayType(TypeTracker typeTracker, IGreLocProvider locMan)
	{
		DeckDetailsLineItem deckDetailsLineItem = Object.Instantiate(TypePrefab, ItemParent);
		deckDetailsLineItem.Name.text = locMan.GetLocalizedTextForEnumValue("CardType", (int)typeTracker.Type);
		deckDetailsLineItem.Quantity.text = typeTracker.Quantity.ToString("N0");
		_itemInstances.Add(deckDetailsLineItem.gameObject);
		foreach (SubtypeTracker item in typeTracker.Subtypes.Values.OrderBy((SubtypeTracker t) => t.LocalizedName))
		{
			deckDetailsLineItem = Object.Instantiate(SubtypePrefab, ItemParent);
			deckDetailsLineItem.Name.text = item.LocalizedName;
			deckDetailsLineItem.Quantity.text = item.Quantity.ToString("N0");
			_itemInstances.Add(deckDetailsLineItem.gameObject);
		}
	}
}
