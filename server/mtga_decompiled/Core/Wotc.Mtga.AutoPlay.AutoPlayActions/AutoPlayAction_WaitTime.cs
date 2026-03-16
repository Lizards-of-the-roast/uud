using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public class AutoPlayAction_WaitTime : AutoPlayAction
{
	private float _waitTime;

	protected override void OnInitialize(in string[] parameters, int index)
	{
		_waitTime = AutoPlayAction.FromParameter(in parameters, index + 1)?.IntoFloat() ?? 0f;
	}

	protected override void OnUpdate()
	{
		if (!base.IsComplete && GetRunTime() >= _waitTime)
		{
			Complete($"Waited for {_waitTime}");
		}
	}
}
