using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public class AutoPlayAction_HangingRefs : AutoPlayAction
{
	private string _marker;

	private bool _complete;

	private AsyncOperation _asyncOp;

	protected override void OnInitialize(in string[] parameters, int index)
	{
		_marker = AutoPlayAction.FromParameter(in parameters, index + 1);
	}

	protected override void OnExecute()
	{
		PAPA.ClearCaches();
		_asyncOp = Resources.UnloadUnusedAssets();
		_asyncOp.completed += OnComplete;
		void OnComplete(AsyncOperation _)
		{
			GC.Collect();
			_complete = true;
		}
	}

	protected override void OnUpdate()
	{
		if (!_complete || base.IsComplete)
		{
			return;
		}
		int num = 0;
		string text = DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture);
		foreach (KeyValuePair<string, int> loadedPath in AssetLoader.loadedPaths)
		{
			Log($"HangingRefs (ref)|{text}|{_marker}|{loadedPath.Value}|{loadedPath.Key}|");
			num += loadedPath.Value;
		}
		Complete($"HangingRefs (r/i)|{text}|{_marker}|{num}|{AssetLoader.loadedPaths.Count}|");
	}
}
