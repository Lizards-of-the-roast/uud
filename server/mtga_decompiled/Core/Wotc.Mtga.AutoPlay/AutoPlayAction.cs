using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.AutoPlay;

public abstract class AutoPlayAction
{
	public class Failure
	{
		public string Message;

		public Stack<string> StackTrace = new Stack<string>();

		public string StringifyStackTrace()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string item in StackTrace.Reverse())
			{
				stringBuilder.AppendLine(item);
			}
			return stringBuilder.ToString();
		}
	}

	private float _timeStart;

	private float _timeStop;

	protected Action<string> LogAction;

	protected AutoPlayComponentGetters ComponentGetters;

	public Failure FailDetails;

	protected bool IsOptional;

	protected float Timeout = 300f;

	private float _delay;

	private bool _hasStarted;

	protected AutoPlayManager _autoplayManager;

	public string[] Parameters;

	public bool IsComplete { get; private set; }

	public bool IsBreak { get; private set; }

	public bool HasFailed => FailDetails != null;

	protected ICardDatabaseAdapter _cardDatabase => _autoplayManager.CardDatabase;

	protected CardViewBuilder _cardViewBuilder => _autoplayManager.CardViewBuilder;

	protected CardMaterialBuilder _cardMaterialBuilder => _autoplayManager.CardMaterialBuilder;

	public int LineNumber { get; private set; }

	public string FileName { get; private set; }

	public void Initialize(string filename, int lineNumber, Action<string> logAction, AutoPlayComponentGetters componentGetters, float delay, bool isOptional, in string[] parameters, int index, AutoPlayManager autoPlayManager)
	{
		FileName = filename;
		LineNumber = lineNumber;
		Parameters = parameters;
		LogAction = logAction;
		IsOptional = isOptional;
		ComponentGetters = componentGetters;
		_delay = delay;
		_autoplayManager = autoPlayManager;
		OnInitialize(in parameters, index);
	}

	protected virtual void OnInitialize(in string[] parameters, int index)
	{
	}

	protected static string FromParameter(in string[] parameters, int index)
	{
		if (parameters.Length <= index)
		{
			return null;
		}
		return parameters[index].Trim();
	}

	protected float GetRunTime()
	{
		return Time.realtimeSinceStartup - _timeStart;
	}

	private float GetMaxRunTime()
	{
		return Timeout + _delay;
	}

	public void Update()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (!(realtimeSinceStartup < _timeStart))
		{
			if (!_hasStarted)
			{
				_hasStarted = true;
				OnExecute();
			}
			if (realtimeSinceStartup > _timeStop)
			{
				Fail("timed out");
			}
			if (!IsComplete)
			{
				OnUpdate();
			}
		}
	}

	protected virtual void OnUpdate()
	{
	}

	public void Execute()
	{
		_timeStart = Time.realtimeSinceStartup + _delay;
		_timeStop = _timeStart + Timeout;
	}

	protected virtual void OnExecute()
	{
	}

	protected void Complete(string message)
	{
		IsComplete = true;
		if (!string.IsNullOrWhiteSpace(message))
		{
			Log($"{message} at {DateTime.UtcNow}");
		}
	}

	protected void Fail(string message, Failure existingFailure = null)
	{
		IsComplete = true;
		if (!IsOptional)
		{
			if (existingFailure != null)
			{
				FailDetails = existingFailure;
			}
			else
			{
				FailDetails = new Failure
				{
					Message = message
				};
			}
			FailDetails.StackTrace.Push(GetTraceInfo());
			Log("FAIL: " + message + " at " + GetTraceInfo());
		}
		else
		{
			Log("Optional Fail: " + message + " at " + GetTraceInfo());
		}
	}

	protected void Break(string message)
	{
		IsBreak = true;
		if (!string.IsNullOrWhiteSpace(message))
		{
			Log($"BREAK: {message} at {DateTime.UtcNow}");
		}
	}

	protected void Log(string message)
	{
		LogAction?.Invoke(message);
	}

	protected bool ProcessNestedScript(AutoPlayScript nestedScript)
	{
		_timeStop = float.MaxValue;
		ScriptResult scriptResult = nestedScript.Update();
		if (scriptResult != null)
		{
			if (scriptResult.IsSuccess)
			{
				return true;
			}
			Fail(scriptResult.Failure.Message, scriptResult.Failure);
		}
		return false;
	}

	public string GetLineText()
	{
		StringBuilder stringBuilder = new StringBuilder(Parameters.Sum((string p) => p.Length + 1));
		for (int num = 0; num < Parameters.Length; num++)
		{
			stringBuilder.Append(Parameters[num]);
			if (num != Parameters.Length - 1)
			{
				stringBuilder.Append("|");
			}
		}
		return stringBuilder.ToString();
	}

	protected string GetTraceInfo()
	{
		return $"\"{GetLineText()}\" at {FileName}:{LineNumber}";
	}
}
