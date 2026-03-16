using System;
using System.Collections.Generic;
using Core.Code.Input;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class IncomingChallengeRequestTile : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _senderName;

	[SerializeField]
	private Localize _challengeTitle;

	[SerializeField]
	private CustomButton _contextClickButton;

	[SerializeField]
	private Button _buttonAccept;

	[SerializeField]
	private Button _buttonReject;

	[SerializeField]
	private Button _buttonBlock;

	[SerializeField]
	private Button _buttonAddFriend;

	private Transform _dropdownRoot;

	private bool _contextMenuActive;

	public Action<Guid> Callback_Accept;

	public Action<Guid> Callback_Reject;

	public Action<string> Callback_Block;

	public Action<string> Callback_AddFriend;

	private IActionSystem _actionSystem;

	public Guid IncomingChallengeId { get; private set; }

	public string ChallengeSenderFullDisplayName { get; private set; }

	public string ChallengeSenderPlayerId { get; private set; }

	private void Awake()
	{
		_contextClickButton.OnRightClick.AddListener(OnContextClick);
		_buttonAccept.onClick.AddListener(delegate
		{
			AudioManager.PlayAudio("sfx_ui_friends_click", base.gameObject);
			Callback_Accept?.Invoke(IncomingChallengeId);
		});
		_buttonReject.onClick.AddListener(delegate
		{
			AudioManager.PlayAudio("sfx_ui_friends_negate", base.gameObject);
			Callback_Reject?.Invoke(IncomingChallengeId);
		});
		_buttonBlock.onClick.AddListener(delegate
		{
			AudioManager.PlayAudio("sfx_ui_friends_negate", base.gameObject);
			Callback_Block?.Invoke(ChallengeSenderFullDisplayName);
		});
		_buttonAddFriend.onClick.AddListener(delegate
		{
			AudioManager.PlayAudio("sfx_ui_friends_negate", base.gameObject);
			Callback_AddFriend?.Invoke(ChallengeSenderFullDisplayName);
		});
	}

	public void OnDestroy()
	{
		Callback_Accept = null;
		Callback_Reject = null;
		Callback_Block = null;
		Callback_AddFriend = null;
		_contextClickButton.OnRightClick.RemoveAllListeners();
		_buttonAccept.onClick.RemoveAllListeners();
		_buttonReject.onClick.RemoveAllListeners();
		_buttonBlock.onClick.RemoveAllListeners();
		_buttonAddFriend.onClick.RemoveAllListeners();
	}

	public void Init(PVPChallengeData incomingChallenge, Transform dropdownSortRoot, bool bInteractable)
	{
		base.gameObject.UpdateActive(active: true);
		IncomingChallengeId = incomingChallenge.ChallengeId;
		ChallengeSenderFullDisplayName = incomingChallenge?.Invites[incomingChallenge.LocalPlayerId]?.Sender?.FullDisplayName;
		ChallengeSenderPlayerId = incomingChallenge?.Invites[incomingChallenge.LocalPlayerId]?.Sender?.PlayerId;
		_senderName.text = SharedUtilities.FormatDisplayName(ChallengeSenderFullDisplayName, Color.white, 0u);
		_challengeTitle.SetText("MainNav/Challenges/Title", new Dictionary<string, string> { 
		{
			"username",
			SharedUtilities.FormatDisplayName(ChallengeSenderFullDisplayName, 0u)
		} });
		_dropdownRoot = dropdownSortRoot;
		_buttonAccept.interactable = bInteractable;
		_buttonAddFriend.interactable = bInteractable;
		_buttonBlock.interactable = bInteractable;
		_buttonReject.interactable = bInteractable;
		_contextClickButton.Interactable = bInteractable;
	}

	public void OnContextClick()
	{
		AudioManager.PlayAudio("sfx_ui_friends_click", base.gameObject);
	}
}
