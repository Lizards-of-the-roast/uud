using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class RequestDurationRenderer : IDisposable
{
	private readonly HeadlessClient _hc;

	private readonly Stopwatch _requestDurationStopwatch = new Stopwatch();

	private readonly Stopwatch _requestProcessStopwatch = new Stopwatch();

	private readonly List<string> _durationOutput = new List<string>();

	private bool _show;

	private BaseUserRequest _trackedRequest;

	private readonly StringBuilder _sb = new StringBuilder();

	public RequestDurationRenderer(HeadlessClient hc)
	{
		_hc = hc;
		_hc.RequestSet += SetTrackedRequest;
		_hc.RequestProcessed += OnRequestProcessed;
	}

	public void Render()
	{
		GUILayout.BeginVertical(GUI.skin.box);
		GUILayout.BeginHorizontal();
		if (GUILayout.Button(_show ? "Hide Request Stats" : "Show Request Stats"))
		{
			_show = !_show;
		}
		if (GUILayout.Button("Copy Request Stats"))
		{
			CopyRequestStats();
		}
		GUILayout.EndHorizontal();
		if (_show)
		{
			GUILayout.BeginVertical(GUI.skin.box);
			foreach (string item in _durationOutput)
			{
				GUILayout.Label(item);
			}
			GUILayout.EndVertical();
		}
		GUILayout.EndVertical();
	}

	private void SetTrackedRequest(BaseUserRequest request)
	{
		if (request != null)
		{
			if (_trackedRequest != null)
			{
				_requestProcessStopwatch.Stop();
				_requestDurationStopwatch.Stop();
				_durationOutput.Add($"(INCOMPLETE) {_trackedRequest.Type} | Process Time {_requestProcessStopwatch.ElapsedMilliseconds} | Total time {_requestDurationStopwatch.ElapsedMilliseconds}");
				BaseUserRequest trackedRequest = _trackedRequest;
				trackedRequest.OnSubmit = (Action<ClientToGREMessage>)Delegate.Remove(trackedRequest.OnSubmit, new Action<ClientToGREMessage>(OnTrackedRequestSubmitted));
				_trackedRequest = null;
			}
			_trackedRequest = request;
			_requestDurationStopwatch.Restart();
			BaseUserRequest trackedRequest2 = _trackedRequest;
			trackedRequest2.OnSubmit = (Action<ClientToGREMessage>)Delegate.Combine(trackedRequest2.OnSubmit, new Action<ClientToGREMessage>(OnTrackedRequestSubmitted));
		}
	}

	private void OnRequestProcessed(BaseUserRequest request)
	{
		if (request != null)
		{
			if (_trackedRequest == null)
			{
				SetTrackedRequest(request);
			}
			else if (_trackedRequest != request)
			{
				_requestProcessStopwatch.Stop();
				_requestDurationStopwatch.Stop();
				_durationOutput.Add($"(REQ MISMATCH) {_trackedRequest.Type} | Process Time {_requestProcessStopwatch.ElapsedMilliseconds} | Total time {_requestDurationStopwatch.ElapsedMilliseconds}");
				BaseUserRequest trackedRequest = _trackedRequest;
				trackedRequest.OnSubmit = (Action<ClientToGREMessage>)Delegate.Remove(trackedRequest.OnSubmit, new Action<ClientToGREMessage>(OnTrackedRequestSubmitted));
				SetTrackedRequest(request);
			}
			_requestProcessStopwatch.Restart();
		}
	}

	private void OnTrackedRequestSubmitted(ClientToGREMessage outMsg)
	{
		_requestDurationStopwatch.Stop();
		_requestProcessStopwatch.Stop();
		_durationOutput.Add($"{_trackedRequest.Type} | Process Time {_requestProcessStopwatch.ElapsedMilliseconds} | Total time {_requestDurationStopwatch.ElapsedMilliseconds}");
		BaseUserRequest trackedRequest = _trackedRequest;
		trackedRequest.OnSubmit = (Action<ClientToGREMessage>)Delegate.Remove(trackedRequest.OnSubmit, new Action<ClientToGREMessage>(OnTrackedRequestSubmitted));
		_trackedRequest = null;
	}

	private void CopyRequestStats()
	{
		_sb.Clear();
		foreach (string item in _durationOutput)
		{
			_sb.AppendLine(item);
		}
		GUIUtility.systemCopyBuffer = _sb.ToString().TrimEnd();
		_sb.Clear();
	}

	public void Dispose()
	{
		_hc.RequestSet -= SetTrackedRequest;
		_hc.RequestProcessed -= OnRequestProcessed;
		_durationOutput.Clear();
		if (_trackedRequest != null)
		{
			BaseUserRequest trackedRequest = _trackedRequest;
			trackedRequest.OnSubmit = (Action<ClientToGREMessage>)Delegate.Remove(trackedRequest.OnSubmit, new Action<ClientToGREMessage>(OnTrackedRequestSubmitted));
			_trackedRequest = null;
		}
	}
}
