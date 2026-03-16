using System;
using AssetLookupTree;
using UnityEngine;
using Wotc.Mtga.Extensions;

public class CommunicationOptionsView : MonoBehaviour
{
	[SerializeField]
	private Animator _containerAnimator;

	[SerializeField]
	private Animator _emoteAnimator;

	[SerializeField]
	private Transform _muteEmoteParent;

	private readonly int ISSHOWING_ANIM_FLAG = Animator.StringToHash("shouldBeShowing");

	private EmoteView _muteEmoteView;

	private EmoteView _unmuteEmoteView;

	public event Action OnMuteEmoteClicked;

	public event Action OnUnmuteEmoteClicked;

	private void Awake()
	{
		Close();
	}

	public void Init(AssetLookupSystem assetLookupSystem)
	{
		_muteEmoteView = EmoteUtils.InstantiateDefaultEmoteView("DuelScene/Emotes/MessagingToggle/Mute", assetLookupSystem);
		_muteEmoteView.transform.SetParent(_muteEmoteParent);
		_muteEmoteView.transform.ZeroOut();
		_muteEmoteView.SetEquipped(isEquipped: true);
		_muteEmoteView.SetClickable(isClickable: true);
		_muteEmoteView.SetHoverable(isHoverable: true);
		_muteEmoteView.OnClick += _muteEmoteClicked;
		_unmuteEmoteView = EmoteUtils.InstantiateDefaultEmoteView("DuelScene/Emotes/MessagingToggle/Unmute", assetLookupSystem);
		_unmuteEmoteView.transform.SetParent(_muteEmoteParent);
		_unmuteEmoteView.transform.ZeroOut();
		_unmuteEmoteView.SetEquipped(isEquipped: true);
		_unmuteEmoteView.SetClickable(isClickable: true);
		_unmuteEmoteView.SetHoverable(isHoverable: true);
		_unmuteEmoteView.OnClick += _unmuteEmoteClicked;
		UpdateIsMuted(isMuted: false);
	}

	public void Open()
	{
		_emoteAnimator.SetTrigger("Pressed");
		_emoteAnimator.ResetTrigger("Highlighted");
		_containerAnimator.SetBool(ISSHOWING_ANIM_FLAG, value: true);
	}

	public void Close()
	{
		_containerAnimator.SetBool(ISSHOWING_ANIM_FLAG, value: false);
	}

	public void UpdateIsMuted(bool isMuted)
	{
		_muteEmoteView.gameObject.SetActive(!isMuted);
		_unmuteEmoteView.gameObject.SetActive(isMuted);
	}

	private void _muteEmoteClicked(string emoteId)
	{
		Close();
		UpdateIsMuted(isMuted: true);
		this.OnMuteEmoteClicked?.Invoke();
	}

	private void _unmuteEmoteClicked(string emoteId)
	{
		Close();
		UpdateIsMuted(isMuted: false);
		this.OnUnmuteEmoteClicked?.Invoke();
	}

	private void OnDestroy()
	{
		this.OnMuteEmoteClicked = null;
		this.OnUnmuteEmoteClicked = null;
	}
}
