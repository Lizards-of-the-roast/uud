using UnityEngine;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public class AutoPlayAction_TimeScale : AutoPlayAction
{
	private float _timeScale;

	protected override void OnInitialize(in string[] parameters, int index)
	{
		_timeScale = AutoPlayAction.FromParameter(in parameters, index + 1)?.IntoFloat() ?? 0f;
	}

	protected override void OnExecute()
	{
		Time.timeScale = _timeScale;
		Complete($"Set Time.timeScale to {_timeScale}x");
	}
}
