using Core.Shared.Code.CardFilters;

public interface ICardMatcherReq
{
	CardFilterGroup Evaluate(CardFilterGroup cards, CardMatcher.CardMatcherMetadata metadata);
}
