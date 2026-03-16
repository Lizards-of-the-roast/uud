using System;
using System.Globalization;
using System.IO;
using UnityEngine;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public class AutoPlayAction_Screenshot : AutoPlayAction
{
	private const string SUBDIR = "Screenshots";

	private string _filename;

	private string _datetime;

	private int _countMax;

	private int _countCur;

	protected override void OnInitialize(in string[] parameters, int index)
	{
		_filename = AutoPlayAction.FromParameter(in parameters, index + 1);
		_countMax = Math.Max(AutoPlayAction.FromParameter(in parameters, index + 2).IntoInt(), 0);
		string path = Path.Combine(Application.persistentDataPath, "Screenshots");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
	}

	private string Filename()
	{
		return Path.Combine("Screenshots", _filename + "_" + _datetime + ".png");
	}

	protected override void OnExecute()
	{
		_datetime = DateTime.UtcNow.ToString("u", CultureInfo.InvariantCulture).Replace(":", "-").Replace(" ", "_");
		if (_countMax == 0)
		{
			string text = Filename();
			ScreenCapture.CaptureScreenshot(text);
			Complete("Captured screenshot: " + text);
		}
	}

	protected override void OnUpdate()
	{
		if (_countCur < _countMax)
		{
			ScreenCapture.CaptureScreenshot(Filename());
			_countCur++;
		}
		else
		{
			Complete($"Captured {_countMax} screenshots: {_filename}");
		}
	}
}
