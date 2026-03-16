using System;
using AssetLookupTree;

public abstract class EntityDialogController : IDisposable
{
	protected IEmoteDataProvider _emoteDataProvider;

	protected UIMessageHandler _uiMessageHandler;

	protected AssetLookupSystem _assetLookupSystem;

	protected EmoteViewPresenter _emoteViewPresenter;

	protected bool _isMuted;

	private bool _showingPermanentEmote;

	public bool AllowSparkyChatter
	{
		get
		{
			if (!_isMuted)
			{
				return !_showingPermanentEmote;
			}
			return false;
		}
	}

	public event Action<bool> IsMutedUpdated;

	public event Action EmotePresented;

	protected EntityDialogController(IEmoteDataProvider emoteDataProvider, UIMessageHandler uiMessageHandler, EmoteViewPresenter emoteViewPresenter, AssetLookupSystem assetLookupSystem)
	{
		_emoteDataProvider = emoteDataProvider;
		_uiMessageHandler = uiMessageHandler;
		_assetLookupSystem = assetLookupSystem;
		_emoteViewPresenter = emoteViewPresenter;
		if (_emoteViewPresenter != null)
		{
			_emoteViewPresenter.EmotePresented += OnEmotePresented;
		}
	}

	public void Dispose()
	{
		if (_emoteViewPresenter != null)
		{
			_emoteViewPresenter.EmotePresented -= OnEmotePresented;
		}
		this.IsMutedUpdated = null;
		this.EmotePresented = null;
	}

	private void OnEmotePresented()
	{
		this.EmotePresented?.Invoke();
	}

	public void ShowPlayerNumericAid(string localizedText)
	{
		_showingPermanentEmote = true;
		_emoteViewPresenter.PresentPermanentEmote(CreateEmoteView(localizedText));
	}

	public void UpdatePlayerNumericAid(string localizedText)
	{
		_emoteViewPresenter.SetDisplayedEmoteText(localizedText);
	}

	public void ClearPlayerNumericAid()
	{
		_showingPermanentEmote = false;
		_emoteViewPresenter.ClearDisplayedEmote();
	}

	public void ShowPlayerChoice(string localizedText)
	{
		DisplayQueuedDialog(localizedText, frontOfQueue: true);
	}

	public void ShowSparkyChatter(string localizedText)
	{
		DisplayQueuedDialog(localizedText, frontOfQueue: false);
	}

	public void PlaySparkyAudio(AudioEvent audioEvent)
	{
		AudioManager.PlayAudio(audioEvent, _emoteViewPresenter.gameObject);
	}

	private void DisplayQueuedDialog(string localizedText, bool frontOfQueue)
	{
		_emoteViewPresenter.PresentQueuedDialog(CreateEmoteView(localizedText), frontOfQueue);
	}

	private EmoteView CreateEmoteView(string localizedString)
	{
		EmoteView emoteView = EmoteUtils.InstantiateDefaultEmoteView("", _assetLookupSystem);
		emoteView.SetLocalizedText(localizedString);
		return emoteView;
	}

	public bool IsMuted()
	{
		return _isMuted;
	}

	public virtual void UpdateIsMuted(bool isMuted)
	{
		this.IsMutedUpdated?.Invoke(isMuted);
	}

	public bool IsAudioPlaying()
	{
		if (_emoteViewPresenter != null)
		{
			return AudioManager.IsSoundsPlayingOnObject(_emoteViewPresenter.gameObject);
		}
		return false;
	}
}
