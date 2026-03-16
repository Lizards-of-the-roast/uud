using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class NullActionEffectController : IActionEffectController
{
	public static readonly IActionEffectController Default = new NullActionEffectController();

	public bool AddActionEffect(ActionInfo actionInfo)
	{
		return false;
	}

	public bool RemoveActionEffect(ActionInfo actionInfo)
	{
		return false;
	}

	public T GetController<T>() where T : class, IActionEffectController
	{
		return this as T;
	}
}
