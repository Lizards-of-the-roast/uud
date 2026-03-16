using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Core.Code.Promises;
using Core.Shared.Code;
using Newtonsoft.Json;
using Wizards.Arena.Client.Logging;
using Wizards.Arena.Promises;
using Wizards.Models.ClientBusinessEvents;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Wizards.Mtga;

public class BILogger : IDisposable, IMutableBILogger, IBILogger
{
	public enum LogDestination
	{
		None = -1,
		WOTC = 1
	}

	public readonly struct BILogItem
	{
		public readonly ClientBusinessEventType Type;

		public readonly IClientBusinessEventReq Payload;

		public readonly LogDestination LogDestination;

		public BILogItem(ClientBusinessEventType type, IClientBusinessEventReq payload, LogDestination logDestination)
		{
			Type = type;
			Payload = payload;
			LogDestination = logDestination;
		}
	}

	private readonly IPlatformLoggerProvider _platformLoggerProvider;

	private IFrontDoorConnectionServiceWrapper _connectionServiceWrapper;

	private readonly ILogger _crossThreadlogger;

	private readonly BILogFactory _biLogFactory;

	private Queue<BILogItem> _logQueue = new Queue<BILogItem>();

	private bool _disposed;

	private readonly float _biBatchTime = 1.2f;

	private CancellationTokenSource _flushCancellationToken;

	public BILogger(ILogger crossThreadLogger = null)
	{
		_crossThreadlogger = crossThreadLogger ?? new ConsoleLogger();
		_biLogFactory = new BILogFactory(_crossThreadlogger);
		_platformLoggerProvider = new NullPlatformLoggerFactory();
	}

	~BILogger()
	{
		OnDispose();
	}

	public void Dispose()
	{
		OnDispose();
		_disposed = true;
		GC.SuppressFinalize(this);
	}

	private void OnDispose()
	{
		if (!_disposed)
		{
			if (_flushCancellationToken != null)
			{
				_flushCancellationToken.Cancel();
				_flushCancellationToken = null;
			}
			Flush();
		}
	}

	public void SetFrontDoorServiceWrapper(IFrontDoorConnectionServiceWrapper fdcWrapper)
	{
		_connectionServiceWrapper = fdcWrapper;
		StartPump();
	}

	public void ClearFdServiceWrapper()
	{
		IFrontDoorConnectionServiceWrapper connectionServiceWrapper = _connectionServiceWrapper;
		if (connectionServiceWrapper != null && connectionServiceWrapper.Connected)
		{
			Flush();
		}
		_connectionServiceWrapper = null;
	}

	private void StartPump()
	{
		MainThreadDispatcher.Dispatch(delegate
		{
			if (_flushCancellationToken == null)
			{
				_flushCancellationToken = new CancellationTokenSource();
				Pantry.Get<GlobalCoroutineExecutor>()?.StartIntervalCoroutine(PreFlush, _biBatchTime, _flushCancellationToken.Token);
			}
		});
	}

	private void PreFlush()
	{
		IFrontDoorConnectionServiceWrapper connectionServiceWrapper = _connectionServiceWrapper;
		if (connectionServiceWrapper != null && connectionServiceWrapper.Connected)
		{
			Flush();
		}
	}

	private void Flush()
	{
		List<IClientBusinessEventReq> list = new List<IClientBusinessEventReq>();
		while (_logQueue.Count > 0)
		{
			BILogItem bILogItem = _logQueue.Dequeue();
			if (bILogItem.LogDestination == LogDestination.WOTC)
			{
				list.Add(bILogItem.Payload);
			}
		}
		if (list.Count > 0)
		{
			EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
			if (currentEnvironment != null && currentEnvironment.HostPlatform == HostPlatform.AWS)
			{
				_connectionServiceWrapper?.FDCAWS?.LogBusinessEvents(list);
			}
		}
	}

	private List<BILogItem> Generate<T>(ClientBusinessEventType businessType) where T : IClientBusinessEventReq
	{
		List<BILogItem> list = new List<BILogItem>();
		T val = _biLogFactory.Generate<T>(businessType);
		BILogItem item = new BILogItem(businessType, val, LogDestination.WOTC);
		list.Add(item);
		T val2 = _platformLoggerProvider.Generate<T>(businessType);
		if (!object.Equals(val2, default(T)))
		{
			list.Add(new BILogItem(businessType, val2, LogDestination.None));
		}
		return list;
	}

	private List<BILogItem> BuildLogItemsCollection<T>(ClientBusinessEventType businessType, T payload) where T : IClientBusinessEventReq
	{
		List<BILogItem> list = new List<BILogItem>();
		if (payload == null)
		{
			list = Generate<T>(businessType);
		}
		else
		{
			BILogItem item = new BILogItem(businessType, payload, LogDestination.WOTC);
			list.Add(item);
		}
		return list;
	}

	public void SendViaFrontdoor<T>(ClientBusinessEventType businessType, T payload) where T : IClientBusinessEventReq
	{
		List<BILogItem> logItems = BuildLogItemsCollection(businessType, payload);
		Send(logItems, isFrontdoorExclusive: true);
	}

	public void Send<T>(ClientBusinessEventType businessType, T payload) where T : IClientBusinessEventReq
	{
		List<BILogItem> logItems = BuildLogItemsCollection(businessType, payload);
		Send(logItems);
	}

	private void Send(BILogItem biLogItem, bool isFrontdoorExclusive = false)
	{
		bool num = _connectionServiceWrapper?.Connected ?? false;
		bool flag = !isFrontdoorExclusive && biLogItem.LogDestination == LogDestination.WOTC && !string.IsNullOrEmpty(Pantry.CurrentEnvironment.bikeUri);
		if (!num && flag)
		{
			TrySendViaBike(biLogItem);
		}
		else
		{
			_logQueue.Enqueue(biLogItem);
		}
	}

	private string GetDiagnosticBreadcrumbMessage()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((_connectionServiceWrapper == null) ? "_connectionServiceWrapper is NULL." : $"_connectionServiceWrapper exists, Connected status is {_connectionServiceWrapper.Connected}.");
		stringBuilder.Append((_connectionServiceWrapper?.FDCAWS == null) ? "FDCAWS is NULL" : $"FDCAWS exists, ConnectionState is {_connectionServiceWrapper?.FDCAWS.ConnectionState}");
		return stringBuilder.ToString();
	}

	private void Send(List<BILogItem> logItems, bool isFrontdoorExclusive = false)
	{
		foreach (BILogItem logItem in logItems)
		{
			Send(logItem, isFrontdoorExclusive);
		}
	}

	private void TrySendViaBike(BILogItem biLogItem)
	{
		try
		{
			BikeMessage value = new BikeMessage
			{
				EventType = biLogItem.Type,
				EventData = JsonConvert.SerializeObject(biLogItem.Payload)
			};
			WebPromise.PostJson(Pantry.CurrentEnvironment.bikeUri, new Dictionary<string, string>(), JsonConvert.SerializeObject(value)).IfError(delegate(Promise<string> p)
			{
				PromiseExtensions.Logger.Warn("[BIKE message] promise error:\n" + p.Error);
				_logQueue.Enqueue(biLogItem);
			});
		}
		catch (Exception ex)
		{
			PromiseExtensions.Logger.Warn("[BIKE message]: exception:\n" + ex);
			_logQueue.Enqueue(biLogItem);
		}
	}
}
