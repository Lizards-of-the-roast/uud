using System;
using System.Collections.Generic;
using Core.Shared.Code.CardFilters;

public class AndReqs : ICardMatcherReq
{
	public readonly List<OrReqs> Reqs;

	public AndReqs()
	{
		Reqs = new List<OrReqs>();
	}

	public AndReqs(params OrReqs[] orReqs)
	{
		Reqs = new List<OrReqs>(orReqs);
	}

	public void Print(int indent)
	{
		Console.WriteLine(new string(' ', indent) + "AND:");
		foreach (OrReqs req in Reqs)
		{
			req.Print(indent + 1);
		}
	}

	public CardFilterGroup Evaluate(CardFilterGroup cards, CardMatcher.CardMatcherMetadata metadata)
	{
		foreach (OrReqs req in Reqs)
		{
			cards = req.Evaluate(cards, metadata);
		}
		return cards;
	}
}
