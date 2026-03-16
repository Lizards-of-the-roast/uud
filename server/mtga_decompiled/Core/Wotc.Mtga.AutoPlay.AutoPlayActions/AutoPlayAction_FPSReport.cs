using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public class AutoPlayAction_FPSReport : AutoPlayAction
{
	private const float FPSMeasurePeriod = 1f;

	private int _fpsAccumulatorCur;

	private int _fpsAccumulatorTotal;

	private float _fpsNextPeriod;

	private int _fpsMin = int.MaxValue;

	private int _fpsMax = int.MinValue;

	private string _marker;

	private float _startTime;

	private float _endTime;

	private List<int> _fullTrack;

	protected override void OnInitialize(in string[] parameters, int index)
	{
		_marker = AutoPlayAction.FromParameter(in parameters, index + 1);
		_endTime = AutoPlayAction.FromParameter(in parameters, index + 2).IntoFloat();
		_fullTrack = new List<int>((int)(_endTime / 1f) + 1);
	}

	protected override void OnExecute()
	{
		_startTime = Time.realtimeSinceStartup;
		_endTime += _startTime;
		_fpsNextPeriod = _startTime + 1f;
	}

	protected override void OnUpdate()
	{
		_fpsAccumulatorCur++;
		_fpsAccumulatorTotal++;
		if (Time.realtimeSinceStartup > _fpsNextPeriod)
		{
			int num = (int)((float)_fpsAccumulatorCur / 1f);
			_fullTrack.Add(num);
			_fpsMin = Math.Min(_fpsMin, num);
			_fpsMax = Math.Max(_fpsMax, num);
			_fpsAccumulatorCur = 0;
			_fpsNextPeriod += 1f;
		}
		if (Time.realtimeSinceStartup > _endTime)
		{
			float num2 = Time.realtimeSinceStartup - _startTime;
			int num3 = (int)((float)_fpsAccumulatorTotal / num2);
			string text = DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture);
			Complete($"FPS (m/a/M)|{text}|{_marker}|{_fpsMin}|{num3}|{_fpsMax}|");
			Log("FPS (graph)|" + text + "|" + _marker + "|" + string.Join(",", _fullTrack.ConvertAll((int o) => o.ToString())) + "|");
		}
	}
}
