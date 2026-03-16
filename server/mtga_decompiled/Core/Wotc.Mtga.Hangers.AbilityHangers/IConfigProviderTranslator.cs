using GreClient.CardData;

namespace Wotc.Mtga.Hangers.AbilityHangers;

public interface IConfigProviderTranslator
{
	IHangerConfigProvider Translate(ICardDataAdapter sourceModel);
}
