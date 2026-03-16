using System;
using AssetLookupTree;
using UnityEngine.EventSystems;

public class CommunicationOptionsController : IEmoteController, IDisposable
{
	private CommunicationOptionsView _communicationOptionsView;

	private bool _isMuted;

	private bool _isOpen;

	public bool Hovered { get; set; }

	public bool Disposed { get; private set; }

	public event Action OnMuteEmoteClicked;

	public event Action OnUnmuteEmoteClicked;

	public CommunicationOptionsController(AssetLookupSystem assetLookupSystem, CommunicationOptionsView communicationOptionsView)
	{
		_communicationOptionsView = communicationOptionsView;
		_communicationOptionsView.Init(assetLookupSystem);
		_communicationOptionsView.OnMuteEmoteClicked += InternalOnMuteEmoteClicked;
		_communicationOptionsView.OnUnmuteEmoteClicked += InternalOnUnmuteEmoteClicked;
	}

	public void UpdateIsMuted(bool isMuted)
	{
		_isMuted = isMuted;
		_communicationOptionsView.UpdateIsMuted(_isMuted);
		EventSystem current = EventSystem.current;
		if ((bool)current)
		{
			current.SetSelectedGameObject(null);
		}
	}

	public void Open()
	{
		_isOpen = true;
		_communicationOptionsView.Open();
	}

	public void Close()
	{
		_isOpen = false;
		_communicationOptionsView.Close();
	}

	public void Toggle()
	{
		if (_isOpen)
		{
			Close();
		}
		else
		{
			Open();
		}
	}

	private void InternalOnMuteEmoteClicked()
	{
		this.OnMuteEmoteClicked?.Invoke();
	}

	private void InternalOnUnmuteEmoteClicked()
	{
		this.OnUnmuteEmoteClicked?.Invoke();
	}

	public void Dispose()
	{
		OnDisposed(manuallyDisposing: true);
		GC.SuppressFinalize(this);
	}

	~CommunicationOptionsController()
	{
		OnDisposed(manuallyDisposing: false);
	}

	protected virtual void OnDisposed(bool manuallyDisposing)
	{
		if (!Disposed && manuallyDisposing)
		{
			if ((bool)_communicationOptionsView)
			{
				_communicationOptionsView.OnUnmuteEmoteClicked -= InternalOnUnmuteEmoteClicked;
				_communicationOptionsView.OnMuteEmoteClicked -= InternalOnMuteEmoteClicked;
			}
			_communicationOptionsView = null;
			this.OnUnmuteEmoteClicked = null;
			this.OnMuteEmoteClicked = null;
			Disposed = true;
		}
	}
}
