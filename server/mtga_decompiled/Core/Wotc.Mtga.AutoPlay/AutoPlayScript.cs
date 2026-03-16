using System;
using System.Collections.Generic;
using System.Linq;

namespace Wotc.Mtga.AutoPlay;

public class AutoPlayScript
{
	private readonly AutoPlayScriptMetadata _metadata = new AutoPlayScriptMetadata();

	private readonly Queue<AutoPlayAction> _actions = new Queue<AutoPlayAction>();

	private AutoPlayAction _currentAction;

	private Action<string> _logAction;

	public string Name => _metadata.Name;

	public AutoPlayScript(in string[] fileContents, Action<string> logAction, AutoPlayComponentGetters componentGetters, AutoPlayManager autoPlayManager)
	{
		_logAction = logAction;
		int num = 0;
		string[] array = fileContents;
		foreach (string line in array)
		{
			num++;
			string[] array2 = _metadata.PreprocessLine(line);
			if (array2 != null)
			{
				AutoPlayAction autoPlayAction = AutoPlayActionGenerator.AutoPlayActionFromString(_metadata.Name, num, array2, logAction, componentGetters, autoPlayManager);
				if (autoPlayAction != null)
				{
					_actions.Enqueue(autoPlayAction);
				}
			}
		}
	}

	public bool IsEmpty()
	{
		return _actions.Count == 0;
	}

	public void Enqueue(AutoPlayAction action)
	{
		_actions.Enqueue(action);
	}

	public AutoPlayAction Dequeue()
	{
		return _actions.Dequeue();
	}

	public void Clear()
	{
		_actions.Clear();
	}

	public ScriptResult Update()
	{
		if (_currentAction == null)
		{
			if (IsEmpty())
			{
				_logAction?.Invoke(Name + " No actions remaining ");
				return ScriptResult.Success();
			}
			_currentAction = Dequeue();
			_logAction(_currentAction.Parameters.Aggregate("", (string total, string next) => total = total + next + "|") ?? "");
			_currentAction.Execute();
		}
		if (_currentAction.IsComplete)
		{
			if (_currentAction.HasFailed)
			{
				return ScriptResult.Fail(_currentAction.FailDetails);
			}
			_currentAction = null;
		}
		else
		{
			if (_currentAction.IsBreak)
			{
				_currentAction = null;
				Clear();
				return ScriptResult.Success();
			}
			_currentAction.Update();
		}
		return null;
	}
}
