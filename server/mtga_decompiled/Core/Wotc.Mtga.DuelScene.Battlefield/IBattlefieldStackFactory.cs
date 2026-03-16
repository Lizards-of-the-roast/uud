using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Battlefield;

public interface IBattlefieldStackFactory
{
	BattlefieldCardHolder.BattlefieldStack Create(DuelScene_CDC parent, List<DuelScene_CDC> attachmentsAndExiles);

	BattlefieldCardHolder.BattlefieldStack Create(ICardDataAdapter parentModel, List<DuelScene_CDC> attachmentsAndExiles);

	BattlefieldCardHolder.BattlefieldStack Create(MtgCardInstance parentInstance, List<DuelScene_CDC> attachmentsAndExiles);
}
