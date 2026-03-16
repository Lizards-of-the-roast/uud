using System;
using System.Diagnostics;
using System.Threading;
using Core.Meta.MainNavigation.SystemMessage;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wizards.Arena.Promises;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Shared.Code.Store;

public class EntitlementPoll
{
	private readonly IMercantileServiceWrapper _mercantile;

	private readonly ISystemMessageManager _systemMessageServiceWrapper;

	private readonly CancellationToken _token;

	private readonly float _basePollingIncrement;

	private readonly float _pollingBeginRampTime;

	private readonly float _pollingEndRampTime;

	private readonly float _totalPollingTime;

	private readonly uint _maxPollAttempts;

	private uint _currentAttempt;

	private readonly float _minPollingPercentage;

	private Stopwatch _pollTime;

	public bool IsPolling { get; private set; }

	public EntitlementPoll(IMercantileServiceWrapper mercantile, ISystemMessageManager systemMessageServiceWrapper, CancellationToken token, float basePollingIncrement = 1f, float pollingBeginRampTime = 10f, float pollingEndRampTime = 30f, float totalPollingTime = 120f, float minPollingPercentage = 0.1f, uint maxPollAttempts = 0u)
	{
		_mercantile = mercantile;
		_systemMessageServiceWrapper = systemMessageServiceWrapper;
		_token = token;
		_basePollingIncrement = basePollingIncrement;
		_pollingBeginRampTime = pollingBeginRampTime;
		_pollingEndRampTime = pollingEndRampTime;
		_totalPollingTime = totalPollingTime;
		_minPollingPercentage = minPollingPercentage;
		_maxPollAttempts = maxPollAttempts;
	}

	public void StartPolling()
	{
		_pollTime = Stopwatch.StartNew();
		_currentAttempt = 0u;
		if (!IsPolling)
		{
			IsPolling = true;
			Poll(_token).Forget();
		}
	}

	private async UniTaskVoid Poll(CancellationToken token)
	{
		Promise<Client_EntitlementsResponse> response = null;
		try
		{
			while (!token.IsCancellationRequested)
			{
				response = await CheckEntitlements();
				if (token.IsCancellationRequested || response.Successful)
				{
					break;
				}
				if (_maxPollAttempts != 0)
				{
					if (_currentAttempt >= _maxPollAttempts)
					{
						break;
					}
					_currentAttempt++;
				}
				float num = (float)_pollTime.Elapsed.TotalSeconds;
				float rampedPercentage = GetRampedPercentage(num, _pollingBeginRampTime, _pollingEndRampTime, _minPollingPercentage);
				float a = _basePollingIncrement / rampedPercentage;
				float b = Mathf.Max(0f, _totalPollingTime - num);
				float num2 = Mathf.Min(a, b);
				if (num2 <= 0f)
				{
					break;
				}
				await UniTask.Delay(TimeSpan.FromSeconds(num2), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
			}
			if (response != null && !response.Successful && response.State != PromiseState.Timeout)
			{
				_systemMessageServiceWrapper.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Store_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Payment_Removal_Error_Text"));
			}
		}
		finally
		{
			IsPolling = false;
			_currentAttempt = 0u;
			_pollTime.Stop();
		}
	}

	private async UniTask<Promise<Client_EntitlementsResponse>> CheckEntitlements()
	{
		Promise<Client_EntitlementsResponse> response = _mercantile.CheckEntitlements(shouldRetry: false);
		await response.AsTask;
		return response;
	}

	public static float GetRampedPercentage(float elapsed, float beginRampTime, float endRampTime, float minPercentage)
	{
		if (elapsed < beginRampTime)
		{
			return 1f;
		}
		if (elapsed <= endRampTime)
		{
			float t = ((endRampTime > beginRampTime) ? Mathf.InverseLerp(beginRampTime, endRampTime, elapsed) : 1f);
			return Mathf.Lerp(1f, minPercentage, t);
		}
		return minPercentage;
	}
}
