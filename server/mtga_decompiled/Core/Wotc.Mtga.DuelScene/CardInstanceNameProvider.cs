using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class CardInstanceNameProvider : IEntityNameProvider<MtgCardInstance>
{
	private readonly IEntityNameProvider<MtgCardInstance> _abilityInstanceProvider;

	private readonly IEntityNameProvider<MtgCardInstance> _cardNameProvider;

	public CardInstanceNameProvider(IEntityNameProvider<MtgCardInstance> abilityInstanceProvider, IEntityNameProvider<MtgCardInstance> cardNameProvider)
	{
		_abilityInstanceProvider = abilityInstanceProvider ?? NullCardNameProvider.Default;
		_cardNameProvider = cardNameProvider ?? NullCardNameProvider.Default;
	}

	public string GetName(MtgCardInstance card, bool formatted = true)
	{
		if (card.ObjectType == GameObjectType.Ability)
		{
			return _abilityInstanceProvider.GetName(card, formatted);
		}
		return _cardNameProvider.GetName(card, formatted);
	}
}
