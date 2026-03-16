using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Battlefield;

public class NullBattlefieldStackFactory : IBattlefieldStackFactory
{
	public static readonly IBattlefieldStackFactory Default = new NullBattlefieldStackFactory();

	public BattlefieldCardHolder.BattlefieldStack Create(DuelScene_CDC parent, List<DuelScene_CDC> attachmentsAndExiles)
	{
		return null;
	}

	public BattlefieldCardHolder.BattlefieldStack Create(ICardDataAdapter parentModel, List<DuelScene_CDC> attachmentsAndExiles)
	{
		return null;
	}

	public BattlefieldCardHolder.BattlefieldStack Create(MtgCardInstance parentInstance, List<DuelScene_CDC> attachmentsAndExiles)
	{
		return null;
	}
}
