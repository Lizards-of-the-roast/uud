using GreClient.CardData;

namespace Wotc.Mtga.DuelScene;

public class GameEffectBuilder : IGameEffectBuilder
{
	private readonly IFakeCardViewController _fakeCardController;

	private readonly ICardHolderProvider _cardHolderProvider;

	public GameEffectBuilder(IFakeCardViewController fakeCardViewController, ICardHolderProvider cardHolderProvider)
	{
		_fakeCardController = fakeCardViewController ?? NullFakeCardViewController.Default;
		_cardHolderProvider = cardHolderProvider ?? NullCardHolderProvider.Default;
	}

	public DuelScene_CDC Create(GameEffectType effectType, string key, ICardDataAdapter cardData)
	{
		if (_cardHolderProvider.TryGetCardHolder(GetControllerEnum(cardData), CardHolderType.Command, out var cardHolder) && cardHolder is IGameEffectController gameEffectController)
		{
			DuelScene_CDC duelScene_CDC = _fakeCardController.CreateFakeCard(key, cardData, isVisible: true);
			gameEffectController.AddGameEffect(duelScene_CDC, effectType);
			return duelScene_CDC;
		}
		return null;
	}

	private static GREPlayerNum GetControllerEnum(ICardDataAdapter cardData)
	{
		return cardData?.ControllerNum ?? GREPlayerNum.Invalid;
	}

	public bool Destroy(string key)
	{
		return _fakeCardController.DeleteFakeCard(key);
	}
}
