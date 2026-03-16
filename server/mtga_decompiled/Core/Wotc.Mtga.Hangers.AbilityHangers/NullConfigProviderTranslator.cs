using GreClient.CardData;

namespace Wotc.Mtga.Hangers.AbilityHangers;

public class NullConfigProviderTranslator : IConfigProviderTranslator
{
	public IHangerConfigProvider Translate(ICardDataAdapter sourceModel)
	{
		return new NullConfigProvider();
	}
}
