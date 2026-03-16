using System;
using Core.Shared.Code.CardFilters;

public class ReqTerm : ICardMatcherReq
{
	public AndReqs AndList;

	public CardPropertyFilter Req;

	public void Print(int indent)
	{
		if (AndList != null)
		{
			Console.WriteLine(new string(' ', indent) + "()");
			AndList.Print(indent + 1);
		}
		else
		{
			Console.WriteLine(new string(' ', indent) + "REQ: " + (Req.Negate ? "Not" : ""));
		}
	}

	public CardFilterGroup Evaluate(CardFilterGroup cards, CardMatcher.CardMatcherMetadata metadata)
	{
		if (AndList != null)
		{
			return AndList.Evaluate(cards, metadata);
		}
		return Req.Evaluate(cards, metadata);
	}
}
