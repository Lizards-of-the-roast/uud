using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using Core.Code.Promises;
using Google.Protobuf;
using Newtonsoft.Json;
using Test.Scenes.NetworkMessagingHarness;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.MessageSerialization;
using Wizards.Arena.Models.Network;
using Wizards.Arena.Promises;
using Wizards.Arena.Protocol;
using Wizards.Arena.TcpConnection;
using Wizards.Mtga;
using Wizards.Unification.Models.Event;
using Wizards.Unification.Models.FrontDoor;
using Wizards.Unification.Models.Mercantile;
using Wizards.Unification.Models.Rewards;
using Wotc.Mtga.Network.ServiceWrappers;

public class NetworkMessagingHarnessRequestPanel : MonoBehaviour
{
	[SerializeField]
	public Dropdown _messageTypeDropdown;

	[SerializeField]
	public Dropdown _serializationFormatDropdown;

	[SerializeField]
	public Text _numIterationsText;

	[SerializeField]
	public Slider _numIterationsSlider;

	[SerializeField]
	public Text _resultsText;

	private FrontDoorConnectionAWS _frontDoorConnection;

	public void Awake()
	{
		StartCoroutine(GetFrontDoorConnectionCoroutine());
	}

	private IEnumerator GetFrontDoorConnectionCoroutine()
	{
		yield return new Until(() => Pantry.CurrentEnvironment != null).AsCoroutine();
		_frontDoorConnection = Pantry.Get<IFrontDoorConnectionServiceWrapper>().FDCAWS;
	}

	public void Start()
	{
		InitializeFormControls();
	}

	private void InitializeFormControls()
	{
		_messageTypeDropdown.options = (from enumName in Enum.GetNames(typeof(TestMessageType))
			select new Dropdown.OptionData(enumName)).ToList();
		_serializationFormatDropdown.options = (from enumName in Enum.GetNames(typeof(SerializationFormat))
			select new Dropdown.OptionData(enumName)).ToList();
		SetIterationsText(_numIterationsSlider.value);
		_numIterationsSlider.onValueChanged.AddListener(SetIterationsText);
	}

	public void SetIterationsText(float value)
	{
		_numIterationsText.text = $"Number of Iterations: {Convert.ToInt32(value)}";
	}

	public void OnClickSendRequest()
	{
		FrontDoorConnectionAWS frontDoorConnection = _frontDoorConnection;
		if (frontDoorConnection == null || frontDoorConnection.ConnectionState != FrontDoorConnectionAWS.FrontDoorConnectionState.Connected)
		{
			_resultsText.text = "Not quite logged in yet; wait a sec or manually log in, then retry.";
			return;
		}
		TestMessageType value = (TestMessageType)_messageTypeDropdown.value;
		SerializationFormat value2 = (SerializationFormat)_serializationFormatDropdown.value;
		int numIterations = Convert.ToInt32(_numIterationsSlider.value);
		SendRequest(value, value2, numIterations).ThenOnMainThread(delegate(MessageResults results)
		{
			_resultsText.text = results.ToString();
		});
	}

	private Promise<MessageResults> SendRequest(TestMessageType messageType, SerializationFormat serializationFormat, int numIterations)
	{
		return messageType switch
		{
			TestMessageType.StartHook => MeasuredJson<StartHookReq, StartHookResponse>(new StartHookReq(), CmdType.StartHook, numIterations), 
			TestMessageType.GetListings => MeasuredJson<GetListingsReqV2, GetListingsV4Response>(new GetListingsReqV2
			{
				TypesAndCacheVersions = AwsMercantileServiceWrapper.ListingCatalogTypes.ToDictionary((EListingType t) => t, (EListingType _) => 0)
			}, CmdType.MercGetListingsV5, numIterations), 
			TestMessageType.GetSkus => MeasuredJson<GetSkusReq, GetSkusResponseV2>(new GetSkusReq(), CmdType.MercGetSkusV3, numIterations), 
			TestMessageType.GetSkusAndListings => MeasuredJson<GetSkusAndListingsReq, GetSkusAndListingsResponse>(new GetSkusAndListingsReq
			{
				SkusCacheVersionHash = "",
				ListingCacheVersionsHash = AwsMercantileServiceWrapper.ListingCatalogTypes.ToDictionary((EListingType t) => t, (EListingType _) => "")
			}, CmdType.MercGetSkusAndListings, numIterations), 
			TestMessageType.GetCourses => MeasuredJson<GetCoursesReq, PlayerCoursesResponseV2>(new GetCoursesReq(), CmdType.EventGetCoursesV2, numIterations), 
			TestMessageType.GetPlayerCards => MeasuredJson<GetAllCardsReq, CardsAndCacheVersion>(new GetAllCardsReq
			{
				CacheVersion = 0
			}, CmdType.CardGetAllCards, numIterations), 
			TestMessageType.GetDailyWeeklyQuests => MeasuredJson<GetPlayerQuestsReq, ClientPeriodicRewards>(new GetPlayerQuestsReq(), CmdType.PeriodicRewardsGetStatus, numIterations), 
			_ => throw new ArgumentOutOfRangeException("messageType", messageType, null), 
		};
	}

	private Promise<MessageResults> MeasuredJson<TIn, TOut>(TIn request, CmdType type, int numIterations) where TOut : new()
	{
		return Measured(request, numIterations, (TIn t) => JsonConvert.SerializeObject(t), new UTF8StringSize(), new ResponsePayloadSize(), new ByteArraySize(), (string stringReq) => _frontDoorConnection.SendMetaRequestMessage((Wizards.Arena.Protocol.Cmd cmd) => new ProtobufMsg(cmd.ToByteArray(), cmd.TransId), (Response resp) => resp, (Response resp) => resp, type, stringReq), (Response resp) => _frontDoorConnection.DecompressJsonPayload(resp), (byte[] str) => _frontDoorConnection.AutoDeserialize<TOut>(str));
	}

	private Promise<MessageResults> MeasuredProtobuf<TIn, TOut>(TIn request, CmdType type, int numIterations) where TIn : IMessage where TOut : IMessage, new()
	{
		return Measured(request, numIterations, (TIn t) => t, new ProtobufMessageSize(), new ResponsePayloadSize(), new ProtobufMessageSize(), (IMessage protoReq) => _frontDoorConnection.SendMetaRequestMessage((Wizards.Arena.Protocol.Cmd cmd) => new ProtobufMsg(cmd.ToByteArray(), cmd.TransId), (Response resp) => resp, (Response resp) => resp, type, protoReq), (Response resp) => resp, (IMessage resp) => ((IMessageEnvelope)resp).UnpackPayload<TOut>());
	}

	private Promise<MessageResults> Measured<TIn, TWireIn, TWireOut, TDecompressed, TOut>(TIn request, int numIterations, Func<TIn, TWireIn> serializer, IByteSizer<TWireIn> wireSizerIn, IByteSizer<TWireOut> wireSizerOut, IByteSizer<TDecompressed> wireSizerDecompressed, Func<TWireIn, Promise<TWireOut>> networkPromiseGen, Func<TWireOut, TDecompressed> decompressor, Func<TDecompressed, TOut> deserializer)
	{
		MessageResults results = new MessageResults
		{
			Iterations = 0
		};
		bool complete = false;
		StartCoroutine(MeasuredCoroutineIterations(results, numIterations, request, serializer, wireSizerIn, wireSizerOut, wireSizerDecompressed, networkPromiseGen, decompressor, deserializer, delegate
		{
			complete = true;
		}));
		return new Until(() => complete).Then((Promise<Unit> _) => new SimplePromise<MessageResults>(results));
	}

	private IEnumerator MeasuredCoroutineIterations<TIn, TWireIn, TWireOut, TDecompressed, TOut>(MessageResults results, int numIterations, TIn request, Func<TIn, TWireIn> serializer, IByteSizer<TWireIn> wireSizerIn, IByteSizer<TWireOut> wireSizerOut, IByteSizer<TDecompressed> wireSizerDecompressed, Func<TWireIn, Promise<TWireOut>> networkPromiseGen, Func<TWireOut, TDecompressed> decompressor, Func<TDecompressed, TOut> deserializer, Action onComplete)
	{
		foreach (int item in Enumerable.Range(0, numIterations))
		{
			_ = item;
			MessageResults iterationResults = new MessageResults();
			yield return MeasuredCoroutine(iterationResults, request, serializer, wireSizerIn, wireSizerOut, wireSizerDecompressed, networkPromiseGen, decompressor, deserializer);
			results.AddIteration(iterationResults);
		}
		onComplete();
	}

	private IEnumerator MeasuredCoroutine<TIn, TWireIn, TWireOut, TDecompressed, TOut>(MessageResults results, TIn request, Func<TIn, TWireIn> serializer, IByteSizer<TWireIn> wireSizerIn, IByteSizer<TWireOut> wireSizerOut, IByteSizer<TDecompressed> wireSizerDecompressed, Func<TWireIn, Promise<TWireOut>> networkPromiseGen, Func<TWireOut, TDecompressed> decompressor, Func<TDecompressed, TOut> deserializer)
	{
		results.Name = typeof(TIn).Name + "->" + typeof(TOut).Name;
		Stopwatch watch = new Stopwatch();
		watch.Start();
		TWireIn val = serializer(request);
		results.SerTime = watch.Elapsed;
		UpdateStats(results);
		results.OutgoingRawMsgSize = wireSizerIn.SizeInBytes(val);
		watch.Restart();
		Promise<TWireOut> promise = networkPromiseGen(val);
		IEnumerator promiseCoroutine = promise.AsCoroutine();
		while (promiseCoroutine.MoveNext())
		{
			UpdateStats(results);
			yield return promiseCoroutine.Current;
		}
		if (!promise.Successful)
		{
			_resultsText.text = promise.Error.ToString();
			yield break;
		}
		TWireOut result = promise.Result;
		results.NetworkTime = watch.Elapsed;
		results.IncomingRawMsgSize = wireSizerOut.SizeInBytes(result);
		watch.Restart();
		TDecompressed val2 = decompressor(result);
		results.DecompressTime = watch.Elapsed;
		results.DecompressedMsgSize = wireSizerDecompressed.SizeInBytes(val2);
		watch.Restart();
		deserializer(val2);
		results.DeserTime = watch.Elapsed;
		UpdateStats(results);
		watch.Stop();
	}

	private void UpdateStats(MessageResults results)
	{
		TimeSpan timeSpan = TimeSpan.FromMilliseconds(_frontDoorConnection.RoundTripTicksRollingAverage);
		if (timeSpan < results.MinPing)
		{
			results.MinPing = timeSpan;
		}
		if (timeSpan > results.MaxPing)
		{
			results.MaxPing = timeSpan;
		}
		long totalMemory = GC.GetTotalMemory(forceFullCollection: false);
		if (totalMemory < results.MinMem)
		{
			results.MinMem = totalMemory;
		}
		if (totalMemory > results.MaxMem)
		{
			results.MaxMem = totalMemory;
		}
	}
}
