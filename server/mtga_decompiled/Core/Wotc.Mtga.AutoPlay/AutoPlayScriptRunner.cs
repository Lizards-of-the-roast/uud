using System;
using UnityEngine;
using Wizards.Mtga.UI;

namespace Wotc.Mtga.AutoPlay;

public class AutoPlayScriptRunner : IDisposable
{
	private readonly Action<string> _logAction;

	private readonly AutoPlayScript _script;

	private AutoPlayAction _currentAction;

	private bool _finished;

	public AutoPlayReport Report;

	public string RunningScriptName => _script.Name;

	public AutoPlayScriptRunner(AutoPlayScript script, Action<string> logAction)
	{
		Report = new AutoPlayReport(script.Name);
		_script = script;
		_logAction = logAction;
		_logAction?.Invoke(script.Name + " Starting");
	}

	public void Update()
	{
		if (_finished)
		{
			return;
		}
		ScreenKeepAlive.KeepScreenAwake();
		Report.Update(Time.unscaledDeltaTime);
		ScriptResult scriptResult = _script.Update();
		if (scriptResult != null)
		{
			if (scriptResult.IsSuccess)
			{
				Complete();
			}
			else
			{
				Fail(scriptResult.Failure);
			}
		}
	}

	private void Fail(AutoPlayAction.Failure failDetails)
	{
		Report.SetFailure(failDetails);
		Finish();
	}

	private void Complete()
	{
		Report.SetSuccess();
		Finish();
	}

	private void Finish()
	{
		_finished = true;
		_logAction?.Invoke(_script.Name + " exiting");
	}

	public bool IsFinished()
	{
		return _finished;
	}

	public void Dispose()
	{
		Report.Dispose();
	}
}
