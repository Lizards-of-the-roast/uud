using System;
using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class ActionEffectController : IActionEffectController, IDisposable
{
	private List<IActionEffectController> _subcontrollers;

	public ActionEffectController(params IActionEffectController[] subcontrollers)
	{
		_subcontrollers = new List<IActionEffectController>(subcontrollers);
	}

	public void Dispose()
	{
		foreach (IActionEffectController subcontroller in _subcontrollers)
		{
			if (subcontroller is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
		_subcontrollers.Clear();
	}

	public bool AddActionEffect(ActionInfo actionInfo)
	{
		foreach (IActionEffectController subcontroller in _subcontrollers)
		{
			if (subcontroller.AddActionEffect(actionInfo))
			{
				return true;
			}
		}
		return false;
	}

	public bool RemoveActionEffect(ActionInfo actionInfo)
	{
		foreach (IActionEffectController subcontroller in _subcontrollers)
		{
			if (subcontroller.RemoveActionEffect(actionInfo))
			{
				return true;
			}
		}
		return false;
	}

	public T GetController<T>() where T : class, IActionEffectController
	{
		return _subcontrollers.Find((IActionEffectController x) => x is T) as T;
	}
}
