using System;
using TMPro;
using UnityEngine;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class FriendInvitePanel : MonoBehaviour
{
	[SerializeField]
	private TMP_InputField _inputField;

	[SerializeField]
	private Localize _errorText;

	[SerializeField]
	private TextMeshProUGUI _placeholderText;

	public Action<string> Callback_OnSubmitInput { get; set; }

	public Action Callback_OnClose { get; set; }

	private void Awake()
	{
		_inputField.onSubmit.AddListener(HandleSubmitInput);
	}

	private void HandleSubmitInput(string potentialFriend)
	{
		if (!ErrorCheck(potentialFriend))
		{
			SendInput(potentialFriend);
		}
	}

	private bool ErrorCheck(string potentialFriend)
	{
		if (!string.IsNullOrEmpty(potentialFriend) && !potentialFriend.Contains("@") && !potentialFriend.Contains("#"))
		{
			_errorText.SetText("Social/Friends/UI/Errors/InvitePrompt/InvalidRequest");
			_errorText.gameObject.UpdateActive(active: true);
			_inputField.Select();
			return true;
		}
		_errorText.gameObject.UpdateActive(active: false);
		return false;
	}

	private void SendInput(string potentialFriend)
	{
		_errorText.SetText("Social/Friends/UI/Confirmation/AddFriendRequest_Sent");
		_errorText.gameObject.UpdateActive(active: true);
		AudioManager.PlayAudio("sfx_ui_friends_send_invite", base.gameObject);
		Callback_OnSubmitInput?.Invoke(potentialFriend);
	}

	public void SendInputButton()
	{
		if (!ErrorCheck(_inputField.text))
		{
			SendInput(_inputField.text);
		}
	}

	public void Show(string placeHolderText)
	{
		base.gameObject.SetActive(value: true);
		_inputField.text = string.Empty;
		_errorText.gameObject.UpdateActive(active: false);
		_inputField.Select();
		_placeholderText.text = placeHolderText;
	}

	public void Close()
	{
		Callback_OnClose?.Invoke();
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
