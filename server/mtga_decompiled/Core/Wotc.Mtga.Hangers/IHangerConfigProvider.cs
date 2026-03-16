using System.Collections.Generic;
using GreClient.CardData;

namespace Wotc.Mtga.Hangers;

public interface IHangerConfigProvider
{
	IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model);
}
