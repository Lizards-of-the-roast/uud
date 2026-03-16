using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public interface IActionEffectController
{
	bool AddActionEffect(ActionInfo actionInfo);

	bool RemoveActionEffect(ActionInfo actionInfo);

	T GetController<T>() where T : class, IActionEffectController;
}
