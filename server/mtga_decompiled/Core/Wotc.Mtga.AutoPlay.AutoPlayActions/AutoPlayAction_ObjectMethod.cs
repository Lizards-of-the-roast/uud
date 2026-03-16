using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public class AutoPlayAction_ObjectMethod : AutoPlayAction
{
	private enum ParameterType
	{
		None,
		BaseEventData
	}

	private string _obName;

	private string _method;

	private ParameterType _parameter;

	protected override void OnInitialize(in string[] parameters, int index)
	{
		_obName = AutoPlayAction.FromParameter(in parameters, index + 1);
		_method = AutoPlayAction.FromParameter(in parameters, index + 2);
		_parameter = AutoPlayAction.FromParameter(in parameters, index + 3).IntoEnum<ParameterType>();
	}

	protected override void OnExecute()
	{
		GameObject gameObject = GameObject.Find(_obName);
		if (gameObject == null)
		{
			Fail("Cannot find object: " + _obName);
			return;
		}
		switch (_parameter)
		{
		case ParameterType.BaseEventData:
			gameObject.SendMessage(_method, new BaseEventData(EventSystem.current), SendMessageOptions.RequireReceiver);
			break;
		case ParameterType.None:
			gameObject.SendMessage(_method, null, SendMessageOptions.RequireReceiver);
			break;
		default:
			throw new ArgumentOutOfRangeException("_parameter", _parameter, null);
		}
		Complete("Event fired: " + _obName + ":" + _method);
	}
}
