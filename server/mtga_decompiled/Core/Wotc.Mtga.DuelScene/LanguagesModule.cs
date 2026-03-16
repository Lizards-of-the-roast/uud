using System;
using System.Collections.Generic;
using UnityEngine;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.DuelScene;

public class LanguagesModule : DebugModule, IDisposable
{
	private int _currentIdx;

	private readonly List<(string humanReadable, string langCode)> _options = new List<(string, string)>();

	public override string Name => "Languages";

	public override string Description => string.Empty;

	public LanguagesModule()
	{
		foreach (KeyValuePair<string, string> item in Languages.Converter)
		{
			_options.Add((item.Key, item.Value));
		}
		SetCurrentIdx();
		Languages.LanguageChangedSignal.Listeners += SetCurrentIdx;
	}

	public override void Render()
	{
		int num = _currentIdx;
		for (int i = 0; i < _options.Count; i++)
		{
			if (_currentIdx == i)
			{
				GUI.backgroundColor = Color.green;
			}
			if (GUILayout.Button(_options[i].humanReadable))
			{
				num = i;
			}
			GUI.backgroundColor = Color.white;
		}
		if (num != _currentIdx)
		{
			Languages.CurrentLanguage = _options[num].langCode;
		}
	}

	private void SetCurrentIdx()
	{
		_currentIdx = _options.FindIndex(((string humanReadable, string langCode) x) => x.langCode == Languages.CurrentLanguage);
		_currentIdx = Mathf.Clamp(_currentIdx, 0, _options.Count);
	}

	public void Dispose()
	{
		Languages.LanguageChangedSignal.Listeners -= SetCurrentIdx;
	}
}
