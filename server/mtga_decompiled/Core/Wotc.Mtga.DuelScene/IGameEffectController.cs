using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public interface IGameEffectController
{
	void AddGameEffect(DuelScene_CDC card, GameEffectType effectType);

	IEnumerable<DuelScene_CDC> GetAllGameEffects();
}
