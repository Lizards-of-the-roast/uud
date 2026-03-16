using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Wotc.Mtga.AutoPlay;

public class AutoPlayAction_EventTrigger : AutoPlayAction
{
	private string _targetTag;

	private EventTriggerType _triggerType;

	protected override void OnInitialize(in string[] parameters, int index)
	{
		if (!Enum.TryParse<EventTriggerType>(AutoPlayAction.FromParameter(in parameters, index + 1), out _triggerType))
		{
			LogAction($"Cannot find trigger type: {_triggerType}");
		}
		_targetTag = AutoPlayAction.FromParameter(in parameters, index + 2);
	}

	protected override void OnExecute()
	{
		Component autoplayHookFromTag = ComponentGetters.GetAutoplayHookFromTag(_targetTag, new Type[1] { typeof(EventTrigger) });
		if (autoplayHookFromTag == null)
		{
			Fail("Failed to find " + _targetTag);
			return;
		}
		foreach (EventTrigger.Entry item in ((EventTrigger)autoplayHookFromTag).triggers.Where((EventTrigger.Entry trigger) => trigger.eventID == _triggerType))
		{
			item.callback?.Invoke(null);
		}
		Complete($"Called all triggers of type {_triggerType} on {_targetTag}");
	}
}
