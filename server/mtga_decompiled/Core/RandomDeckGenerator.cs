using System;
using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

public class RandomDeckGenerator
{
	private class WeightedChanceParam
	{
		public System.Action Func { get; }

		public double Ratio { get; }

		public WeightedChanceParam(System.Action func, double ratio)
		{
			Func = func;
			Ratio = ratio;
		}
	}

	private class WeightedChanceExecutor
	{
		private Random r;

		public WeightedChanceParam[] Parameters { get; }

		public double RatioSum
		{
			get
			{
				double num = 0.0;
				if (Parameters != null)
				{
					WeightedChanceParam[] parameters = Parameters;
					foreach (WeightedChanceParam weightedChanceParam in parameters)
					{
						num += weightedChanceParam.Ratio;
					}
				}
				return num;
			}
		}

		public WeightedChanceExecutor(Random rng, params WeightedChanceParam[] parameters)
		{
			Parameters = parameters;
			r = rng;
		}

		public void Execute()
		{
			double num = r.NextDouble() * RatioSum;
			WeightedChanceParam[] parameters = Parameters;
			foreach (WeightedChanceParam weightedChanceParam in parameters)
			{
				num -= weightedChanceParam.Ratio;
				if (num <= 0.0)
				{
					weightedChanceParam.Func();
					break;
				}
			}
		}
	}

	private Random _rng;

	private List<CardPrintingData> _cardData;

	public RandomDeckGenerator(List<CardPrintingData> cardData)
	{
		_rng = new Random();
		_cardData = cardData;
	}

	public List<uint> GenerateRandomDeck()
	{
		List<CardColor> deckColors = new List<CardColor>();
		new WeightedChanceExecutor(_rng, new List<WeightedChanceParam>
		{
			new WeightedChanceParam(delegate
			{
				deckColors = GetDeckColors(_rng, 1);
			}, 15.0),
			new WeightedChanceParam(delegate
			{
				deckColors = GetDeckColors(_rng, 2);
			}, 50.0),
			new WeightedChanceParam(delegate
			{
				deckColors = GetDeckColors(_rng, 3);
			}, 35.0)
		}.ToArray()).Execute();
		List<CardPrintingData> lands = new List<CardPrintingData>();
		List<CardPrintingData> creatures = new List<CardPrintingData>();
		List<CardPrintingData> instants = new List<CardPrintingData>();
		List<CardPrintingData> sorceries = new List<CardPrintingData>();
		List<CardPrintingData> enchantments = new List<CardPrintingData>();
		List<CardPrintingData> artifacts = new List<CardPrintingData>();
		List<CardPrintingData> planeswalkers = new List<CardPrintingData>();
		foreach (CardPrintingData cardDatum in _cardData)
		{
			bool flag = true;
			foreach (CardColor item in cardDatum.ColorIdentity)
			{
				if (!deckColors.Contains(item))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				if (cardDatum.Types.Contains(CardType.Land))
				{
					lands.Add(cardDatum);
				}
				if (cardDatum.Types.Contains(CardType.Artifact))
				{
					artifacts.Add(cardDatum);
				}
				if (cardDatum.Types.Contains(CardType.Creature))
				{
					creatures.Add(cardDatum);
				}
				if (cardDatum.Types.Contains(CardType.Instant))
				{
					instants.Add(cardDatum);
				}
				if (cardDatum.Types.Contains(CardType.Sorcery))
				{
					sorceries.Add(cardDatum);
				}
				if (cardDatum.Types.Contains(CardType.Enchantment))
				{
					enchantments.Add(cardDatum);
				}
				if (cardDatum.Types.Contains(CardType.Planeswalker))
				{
					planeswalkers.Add(cardDatum);
				}
			}
		}
		List<CardPrintingData> deckCardData = new List<CardPrintingData>();
		WeightedChanceExecutor weightedChanceExecutor = new WeightedChanceExecutor(_rng, new List<WeightedChanceParam>
		{
			new WeightedChanceParam(delegate
			{
				GetCardFromList(_rng, deckCardData, creatures);
			}, 33.0),
			new WeightedChanceParam(delegate
			{
				GetCardFromList(_rng, deckCardData, instants);
			}, 16.0),
			new WeightedChanceParam(delegate
			{
				GetCardFromList(_rng, deckCardData, sorceries);
			}, 16.0),
			new WeightedChanceParam(delegate
			{
				GetCardFromList(_rng, deckCardData, enchantments);
			}, 16.0),
			new WeightedChanceParam(delegate
			{
				GetCardFromList(_rng, deckCardData, artifacts);
			}, 13.0),
			new WeightedChanceParam(delegate
			{
				GetCardFromList(_rng, deckCardData, planeswalkers);
			}, 6.0)
		}.ToArray());
		while (deckCardData.Count < 36)
		{
			weightedChanceExecutor.Execute();
		}
		Func<ManaColor, CardColor> func = (ManaColor c) => c switch
		{
			ManaColor.White => CardColor.White, 
			ManaColor.Blue => CardColor.Blue, 
			ManaColor.Black => CardColor.Black, 
			ManaColor.Red => CardColor.Red, 
			ManaColor.Green => CardColor.Green, 
			_ => CardColor.Colorless, 
		};
		double num = 0.0;
		Dictionary<CardColor, uint> dictionary = new Dictionary<CardColor, uint>();
		foreach (CardPrintingData item2 in deckCardData)
		{
			num += (double)item2.ConvertedManaCost;
			foreach (ManaQuantity item3 in item2.CastingCost)
			{
				CardColor key = func(item3.Color);
				if (!dictionary.ContainsKey(key))
				{
					dictionary.Add(key, 0u);
				}
				dictionary[key] += item3.Count;
				if (item3.Hybrid)
				{
					if (!dictionary.ContainsKey(key))
					{
						dictionary.Add(key, 0u);
					}
					dictionary[key] += item3.Count;
				}
			}
		}
		List<WeightedChanceParam> list = new List<WeightedChanceParam>();
		foreach (CardColor col in dictionary.Keys)
		{
			double ratio = (double)dictionary[col] / num;
			list.Add(new WeightedChanceParam(delegate
			{
				GetLandFromList(_rng, col, deckCardData, lands);
			}, ratio));
		}
		WeightedChanceExecutor weightedChanceExecutor2 = new WeightedChanceExecutor(_rng, list.ToArray());
		while (deckCardData.Count < 60)
		{
			weightedChanceExecutor2.Execute();
		}
		return deckCardData.ConvertAll((CardPrintingData x) => x.GrpId);
	}

	private void GetCardFromList(Random rng, List<CardPrintingData> deckList, List<CardPrintingData> possiblePrinting)
	{
		if (possiblePrinting.Count > 0)
		{
			int index = rng.Next(0, possiblePrinting.Count);
			deckList.Add(possiblePrinting[index]);
		}
	}

	private void GetLandFromList(Random rng, CardColor col, List<CardPrintingData> deckList, List<CardPrintingData> possiblePrinting)
	{
		List<CardPrintingData> list = new List<CardPrintingData>(possiblePrinting);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (!list[num].ColorIdentity.Contains(col))
			{
				list.RemoveAt(num);
			}
		}
		if (list.Count > 0)
		{
			int index = rng.Next(0, list.Count);
			deckList.Add(list[index]);
		}
	}

	private List<CardColor> GetDeckColors(Random rng, int colorCount)
	{
		List<CardColor> list = new List<CardColor>();
		List<CardColor> list2 = new List<CardColor>
		{
			CardColor.White,
			CardColor.Blue,
			CardColor.Black,
			CardColor.Red,
			CardColor.Green
		};
		while (list.Count < colorCount)
		{
			int index = rng.Next(0, list2.Count);
			list.Add(list2[index]);
			list2.RemoveAt(index);
		}
		return list;
	}
}
