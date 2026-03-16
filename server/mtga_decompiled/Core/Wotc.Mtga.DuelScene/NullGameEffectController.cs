using System;
using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public class NullGameEffectController : IGameEffectController
{
	public static readonly IGameEffectController Default = new NullGameEffectController();

	public void AddGameEffect(DuelScene_CDC card, GameEffectType effectType)
	{
	}

	public IEnumerable<DuelScene_CDC> GetAllGameEffects()
	{
		return Array.Empty<DuelScene_CDC>();
	}
}
