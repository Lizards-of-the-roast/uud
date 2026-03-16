using System;
using System.Collections.Generic;
using System.Linq;
using Core.Shared.Code.CardFilters;

public class OrReqs : ICardMatcherReq
{
	public readonly List<ReqTerm> Reqs;

	public OrReqs()
	{
		Reqs = new List<ReqTerm>();
	}

	public OrReqs(params ReqTerm[] terms)
	{
		Reqs = new List<ReqTerm>(terms);
	}

	public void Print(int indent)
	{
		Console.WriteLine(new string(' ', indent) + "OR:");
		foreach (ReqTerm req in Reqs)
		{
			req.Print(indent + 1);
		}
	}

	public CardFilterGroup Evaluate(CardFilterGroup cards, CardMatcher.CardMatcherMetadata metadata)
	{
		if (Reqs.Count == 0)
		{
			return cards;
		}
		HashSet<int> passedIndices = new HashSet<int>();
		Dictionary<int, CardFilterGroup.FilteredReason> dictionary = new Dictionary<int, CardFilterGroup.FilteredReason>();
		foreach (ReqTerm req in Reqs)
		{
			CardFilterGroup cards2 = new CardFilterGroup((from card in cards.Cards
				where !passedIndices.Contains(card.Index)
				select new CardFilterGroup.FilteredCard(card)).ToList());
			foreach (CardFilterGroup.FilteredCard card in req.Evaluate(cards2, metadata).Cards)
			{
				if (card.PassedFilter)
				{
					passedIndices.Add(card.Index);
					continue;
				}
				if (!dictionary.TryGetValue(card.Index, out var value))
				{
					dictionary[card.Index] = card.FailReason;
					continue;
				}
				value |= card.FailReason;
				dictionary[card.Index] = value;
			}
		}
		foreach (CardFilterGroup.FilteredCard card2 in cards.Cards)
		{
			if (!passedIndices.Contains(card2.Index) && dictionary.TryGetValue(card2.Index, out var value2))
			{
				card2.FailFilter(value2);
			}
		}
		return cards;
	}
}
