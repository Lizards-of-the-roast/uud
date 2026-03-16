using System;
using AK.Wwise;
using MTGA.Social;
using UnityEngine;
using Wizards.GeneralUtilities.AdvancedButton;
using Wizards.Mtga;

namespace Wizards.Arena.Gathering;

public class GatheringUserStatusDropdown : MonoBehaviour
{
	[Header("Button References")]
	[SerializeField]
	private AdvancedButton _onlineStatusButton;

	[SerializeField]
	private AdvancedButton _busyStatusButton;

	[SerializeField]
	private AdvancedButton _offlineStatusButton;

	[Header("Audio References")]
	[SerializeField]
	private AK.Wwise.Event _onlineStatusButtonClickedAudioEvent;

	[SerializeField]
	private AK.Wwise.Event _busyStatusButtonClickedAudioEvent;

	[SerializeField]
	private AK.Wwise.Event _offlineStatusButtonClickedAudioEvent;

	[SerializeField]
	private AK.Wwise.Event _buttonRolloverAudioEvent;

	private ISocialManager _socialManager;

	private void Awake()
	{
		_socialManager = Pantry.Get<ISocialManager>();
	}

	private void Start()
	{
		_onlineStatusButton.OnMouseHoverEnterEvent.AddListener(ButtonRolloverAudioEvent);
		_busyStatusButton.OnMouseHoverEnterEvent.AddListener(ButtonRolloverAudioEvent);
		_offlineStatusButton.OnMouseHoverEnterEvent.AddListener(ButtonRolloverAudioEvent);
		_onlineStatusButton.onClick.AddListener(delegate
		{
			SetPresence(PresenceStatus.Available);
		});
		_busyStatusButton.onClick.AddListener(delegate
		{
			SetPresence(PresenceStatus.Busy);
		});
		_offlineStatusButton.onClick.AddListener(delegate
		{
			SetPresence(PresenceStatus.Offline);
		});
	}

	private void OnDestroy()
	{
		_onlineStatusButton.OnMouseHoverEnterEvent.RemoveAllListeners();
		_busyStatusButton.OnMouseHoverEnterEvent.RemoveAllListeners();
		_offlineStatusButton.OnMouseHoverEnterEvent.RemoveAllListeners();
		_onlineStatusButton.onClick.RemoveAllListeners();
		_busyStatusButton.onClick.RemoveAllListeners();
		_offlineStatusButton.onClick.RemoveAllListeners();
	}

	private void SetPresence(PresenceStatus presenceStatus)
	{
		switch (presenceStatus)
		{
		case PresenceStatus.Offline:
			AudioManager.PlayAudio(_offlineStatusButtonClickedAudioEvent.Name, base.gameObject);
			break;
		case PresenceStatus.Busy:
			AudioManager.PlayAudio(_busyStatusButtonClickedAudioEvent.Name, base.gameObject);
			break;
		case PresenceStatus.Available:
			AudioManager.PlayAudio(_onlineStatusButtonClickedAudioEvent.Name, base.gameObject);
			break;
		default:
			throw new ArgumentOutOfRangeException("presenceStatus", presenceStatus, null);
		case PresenceStatus.Away:
			break;
		}
		_socialManager.SetUserPresenceStatus(presenceStatus);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void ButtonRolloverAudioEvent()
	{
		AudioManager.PlayAudio(_buttonRolloverAudioEvent.Name, base.gameObject);
	}
}
