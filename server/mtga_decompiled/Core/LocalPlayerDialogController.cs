using AssetLookupTree;
using Wotc.Mtga.DuelScene.UXEvents;

public class LocalPlayerDialogController : EntityDialogController
{
	private EmoteOptionsController _emoteOptionsController;

	private DuelSceneLogger _duelSceneLogger;

	public LocalPlayerDialogController(IEmoteDataProvider emoteDataProvider, UIMessageHandler uiMessageHandler, EmoteViewPresenter emoteViewPresenter, AssetLookupSystem assetLookupSystem, EmoteOptionsController emoteController, UXEventQueue uxEventQueue, DuelSceneLogger duelSceneLogger)
		: base(emoteDataProvider, uiMessageHandler, emoteViewPresenter, assetLookupSystem)
	{
		_emoteOptionsController = emoteController;
		uxEventQueue.EventExecutionCommenced += _checkIfGameStateUpdated;
		_duelSceneLogger = duelSceneLogger;
		_emoteOptionsController.OnEmoteOptionClicked += _sendEmoteDataPacket;
		_uiMessageHandler.EmoteRecievedCallback += _receiveEmoteDataPacket;
	}

	private void _checkIfGameStateUpdated(UXEvent uxEvent)
	{
		if (uxEvent is GameStatePlaybackCommencedUXEvent { GameState: not null } gameStatePlaybackCommencedUXEvent)
		{
			_emoteOptionsController.OnGameStateUpdated(gameStatePlaybackCommencedUXEvent.GameState);
		}
	}

	private void _sendEmoteDataPacket(EmoteData emoteData)
	{
		if (_uiMessageHandler.TrySendEmote(emoteData.Id))
		{
			_duelSceneLogger?.EmoteUsed(emoteData.Id, emoteData.Entry.Category);
		}
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.GREPlayerNum = GREPlayerNum.LocalPlayer;
		EmoteView emoteView = EmoteUtils.InstantiateEmoteView(emoteData, _assetLookupSystem);
		if (!(emoteView == null))
		{
			emoteView.Init(emoteData.Id, EmoteUtils.GetFullLocKey(emoteData.Id, _assetLookupSystem), EmoteUtils.GetEmoteSfxData(emoteData.Id, _assetLookupSystem));
			_emoteViewPresenter.PresentQueuedEmote(emoteView, emoteData);
		}
	}

	private void _receiveEmoteDataPacket(string emoteId)
	{
		_emoteOptionsController.OnEmoteRecieved(emoteId);
	}

	public override void UpdateIsMuted(bool isMuted)
	{
	}
}
