using AssetLookupTree;

namespace Wotc.Mtga.DuelScene.Emotes;

public class OpponentDialogController : EntityDialogController
{
	public CommunicationOptionsController _communicationOptionsController;

	private DuelSceneLogger _duelSceneLogger;

	public OpponentDialogController(IEmoteDataProvider emoteDataProvider, UIMessageHandler uiMessageHandler, AssetLookupSystem assetLookupSystem, CommunicationOptionsController emoteController, EmoteViewPresenter emoteViewPresenter, DuelSceneLogger duelSceneLogger)
		: base(emoteDataProvider, uiMessageHandler, emoteViewPresenter, assetLookupSystem)
	{
		_uiMessageHandler.EmoteRecievedCallback += _receiveEmoteDataPacket;
		_communicationOptionsController = emoteController;
		_duelSceneLogger = duelSceneLogger;
		_communicationOptionsController.OnMuteEmoteClicked += delegate
		{
			UserSetIsMuted(isMuted: true);
		};
		_communicationOptionsController.OnUnmuteEmoteClicked += delegate
		{
			UserSetIsMuted(isMuted: false);
		};
	}

	public override void UpdateIsMuted(bool isMuted)
	{
		if (_isMuted != isMuted)
		{
			if (isMuted)
			{
				_emoteViewPresenter.ClearQueue();
			}
			_isMuted = isMuted;
			base.UpdateIsMuted(isMuted);
			_communicationOptionsController.UpdateIsMuted(isMuted);
		}
	}

	private void UserSetIsMuted(bool isMuted)
	{
		UpdateIsMuted(isMuted);
		_duelSceneLogger.OpponentMutedChanged(isMuted);
	}

	private void _receiveEmoteDataPacket(string emoteId)
	{
		if (!_isMuted && _emoteDataProvider.TryGetEmoteData(emoteId, out var emoteData) && TryInstantiateEmoteView(emoteData, out var outView))
		{
			outView.Init(emoteId, EmoteUtils.GetFullLocKey(emoteId, _assetLookupSystem), EmoteUtils.GetEmoteSfxData(emoteData.Id, _assetLookupSystem));
			_emoteViewPresenter.PresentQueuedEmote(outView, emoteData);
		}
	}

	private bool TryInstantiateEmoteView(EmoteData emoteData, out EmoteView outView)
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.GREPlayerNum = GREPlayerNum.Opponent;
		outView = EmoteUtils.InstantiateEmoteView(emoteData, _assetLookupSystem);
		return outView != null;
	}
}
