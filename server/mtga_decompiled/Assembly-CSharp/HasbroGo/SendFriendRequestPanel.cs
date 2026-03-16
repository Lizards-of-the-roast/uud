using System;
using TMPro;
using UnityEngine;

namespace HasbroGo;

public class SendFriendRequestPanel : MonoBehaviour
{
	[SerializeField]
	private TMP_InputField friendInfoInputField;

	[SerializeField]
	private TextMeshProUGUI sendFriendRequestOuput;

	private readonly string friendRequestSendSucceededOutput = "Friend Request Successfully Sent";

	private readonly string friendRequestSendFailedOutput = "Friend request failed to send. Please check your spelling and try again.";

	private readonly string friendRequestSendInvalidInputOutput = "Invalid Display Name, Email Address, Account Id, or Persona Id. Please check your spelling and try again.";

	private void OnEnable()
	{
		friendInfoInputField.text = string.Empty;
		sendFriendRequestOuput.text = string.Empty;
		SocialManager.Instance.FriendInviteSentEvent += OnFriendRequestSuccess;
		SocialManager.Instance.FriendActionErrorEvent += OnFriendRequestFailure;
	}

	private void OnDisable()
	{
		SocialManager.Instance.FriendInviteSentEvent -= OnFriendRequestSuccess;
		SocialManager.Instance.FriendActionErrorEvent -= OnFriendRequestFailure;
	}

	public void SendFriendRequestButtonClicked()
	{
		SocialManager.Instance.SendFriendInvite(friendInfoInputField.text);
	}

	private void OnFriendRequestSuccess(object sender, EventArgs e)
	{
		sendFriendRequestOuput.text = friendRequestSendSucceededOutput;
	}

	private void OnFriendRequestFailure(object sender, EventArgs e)
	{
		bool flag = (bool)(e as ErrorEventArgs).ExtraData;
		sendFriendRequestOuput.text = (flag ? friendRequestSendInvalidInputOutput : friendRequestSendFailedOutput);
	}
}
