namespace Wotc.Mtga.DuelScene;

public class CardHolderReference<T> where T : ICardHolder
{
	private readonly ICardHolderProvider _cardHolderProvider;

	private readonly GREPlayerNum _playerType;

	private readonly CardHolderType _cardHolderType;

	private T _cacheRef;

	public CardHolderReference(ICardHolderProvider cardHolderProvider, GREPlayerNum playerType, CardHolderType cardHolderType)
	{
		_cardHolderProvider = cardHolderProvider;
		_playerType = playerType;
		_cardHolderType = cardHolderType;
	}

	public T Get()
	{
		T cacheRef = _cacheRef;
		if (cacheRef == null)
		{
			return _cacheRef = _cardHolderProvider.GetCardHolder<T>(_playerType, _cardHolderType);
		}
		return cacheRef;
	}

	public void ClearCache()
	{
		_cacheRef = default(T);
	}

	public static CardHolderReference<StackCardHolder> Stack(ICardHolderProvider cardHolderProvider)
	{
		return new CardHolderReference<StackCardHolder>(cardHolderProvider, GREPlayerNum.Invalid, CardHolderType.Stack);
	}

	public static CardHolderReference<IBattlefieldCardHolder> Battlefield(ICardHolderProvider cardHolderProvider)
	{
		return new CardHolderReference<IBattlefieldCardHolder>(cardHolderProvider, GREPlayerNum.Invalid, CardHolderType.Battlefield);
	}

	public static CardHolderReference<ICardHolder> DefaultBrowser(ICardHolderProvider cardHolderProvider)
	{
		return new CardHolderReference<ICardHolder>(cardHolderProvider, GREPlayerNum.Invalid, CardHolderType.CardBrowserDefault);
	}
}
