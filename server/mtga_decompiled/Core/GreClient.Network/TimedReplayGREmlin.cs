using System;
using System.Collections.Generic;
using AssetLookupTree;
using UnityEngine;
using Wotc.Mtga.TimedReplays;
using Wotc.Mtgo.Gre.External.Messaging;

namespace GreClient.Network;

internal class TimedReplayGREmlin : GremlinBase
{
	private readonly ReplayReader _replay;

	private readonly ReplayMessageFilter _filter = new ReplayMessageFilter((IReadOnlyCollection<GREMessageType>)(object)Array.Empty<GREMessageType>(), ReplayMessageFilter.CommonClientMessagesToFilter);

	private float _elapsedTimeMS = -1000f;

	private bool _completed;

	public TimedReplayGREmlin(string replayPath)
	{
		ReplayReader.Result result = ReplayReader.TryCreateReplayFromPath(replayPath, out _replay, _filter);
		if (result.ResultType == ReplayReader.Result.ResultTypes.DeserializeException)
		{
			OnError(result.FailMessage);
		}
	}

	private void OnError(string message)
	{
		SimpleLog.LogError($"[Gremlin] Replay parsing error at line {_replay.LineCount}:\n {message}");
	}

	public void SetUpCosmetics(MatchManager manager, AssetLookupSystem assetLookupSystem)
	{
		BattlefieldUtil.SetBattlefieldById(assetLookupSystem, _replay.GetBattlefield());
		var (cosmetics, cosmetics2) = _replay.GetPlayerInfo();
		manager.LocalPlayerInfo.SetCosmetics(cosmetics);
		manager.OpponentInfo.SetCosmetics(cosmetics2);
	}

	public override void ProcessMessages()
	{
		_elapsedTimeMS += Time.deltaTime * 1000f;
		if (!TryUpdate())
		{
			return;
		}
		if (!_replay.IsPassedEnd())
		{
			ReplayEntry currentMessage = _replay.GetCurrentMessage();
			if (currentMessage?.GREToClient != null && (float?)currentMessage?.Timestamp < _elapsedTimeMS)
			{
				InvokeMessageReceived(currentMessage.GREToClient);
				MoveToNextMessage();
				_elapsedTimeMS = currentMessage.Timestamp;
			}
		}
		else if (!_completed)
		{
			_completed = true;
		}
	}

	public override void SendMessage(ClientToGREMessage receivedMessage)
	{
		if (_filter.ShouldIgnore(receivedMessage))
		{
			return;
		}
		if (_replay.IsPassedEnd())
		{
			SimpleLog.LogError($"[Gremlin] Unexpected additional message received: {receivedMessage}");
			return;
		}
		ReplayEntry currentMessage = _replay.GetCurrentMessage();
		ClientToGREMessage clientToGRE = currentMessage.ClientToGRE;
		if (clientToGRE == null)
		{
			SimpleLog.LogError($"[Gremlin] Unexpected GRE message received. Received message: {receivedMessage}");
			return;
		}
		if (!clientToGRE.Equals(receivedMessage))
		{
			SimpleLog.LogError($"[Gremlin] Unexpected GRE contents received: {receivedMessage}.\nExpected: {clientToGRE}");
			return;
		}
		MoveToNextMessage();
		_elapsedTimeMS = currentMessage.Timestamp;
	}

	private void MoveToNextMessage()
	{
		ReplayReader.Result result = _replay.TryMoveToNextMessage();
		if (result.ResultType == ReplayReader.Result.ResultTypes.DeserializeException)
		{
			OnError(result.FailMessage);
		}
	}
}
