using Core.Shared.Code.CardFilters;

public abstract class CardPropertyFilter
{
	public enum PropertyType
	{
		None,
		Title,
		ExpansionCode,
		Type,
		Text,
		ManaCost,
		Artist,
		Flavor,
		AnyText,
		CMC,
		Power,
		Toughness,
		Loyalty,
		Rarity,
		Owned,
		Color,
		ColorIdentity,
		Trait,
		NormalizedCommon,
		Rebalanced,
		Nickname
	}

	public bool Negate;

	public abstract CardFilterGroup Evaluate(CardFilterGroup filterSets, CardMatcher.CardMatcherMetadata metadata);
}
