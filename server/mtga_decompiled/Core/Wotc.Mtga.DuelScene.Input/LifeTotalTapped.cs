namespace Wotc.Mtga.DuelScene.Input;

public class LifeTotalTapped : IEntityInputEvent<IAvatarView>
{
	private readonly ICardHolderProvider _cardHolderProvider;

	public LifeTotalTapped(ICardHolderProvider cardHolderProvider)
	{
		_cardHolderProvider = cardHolderProvider ?? NullCardHolderProvider.Default;
	}

	public void Execute(IAvatarView avatar)
	{
		if (avatar.IsLocalPlayer && _cardHolderProvider.TryGetCardHolder(GREPlayerNum.LocalPlayer, CardHolderType.Hand, out var cardHolder) && cardHolder is HandCardHolder handCardHolder)
		{
			handCardHolder.OnLifePillClicked();
		}
	}
}
