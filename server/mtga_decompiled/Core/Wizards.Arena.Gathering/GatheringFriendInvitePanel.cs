using AK.Wwise;
using MTGA.Social;
using TMPro;
using UnityEngine;
using Wizards.Arena.Gathering.Friend_Invite;
using Wizards.GeneralUtilities.SerializeReferenceSelect;
using Wizards.Mtga;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace Wizards.Arena.Gathering;

public class GatheringFriendInvitePanel : MonoBehaviour
{
	[SerializeField]
	private TMP_InputField _friendNameInputField;

	[SerializeReference]
	[SelectImplementation(typeof(IUsernameValidator), false, false)]
	private IUsernameValidator _usernameValidator;

	[SerializeField]
	private TextMeshProUGUI _inputFieldPlaceHolderText;

	[SerializeField]
	private Localize _messageText;

	[SerializeField]
	private AK.Wwise.Event _inviteSentAudioEvent;

	[SerializeField]
	private float _successfulSubmissionDestructionWaitTime = 1f;

	private ISocialManager _socialManager;

	private void Awake()
	{
		_socialManager = Pantry.Get<ISocialManager>();
	}

	private void Start()
	{
		_inputFieldPlaceHolderText.text = _socialManager.LocalPlayer.FullName;
		_friendNameInputField.onValueChanged.AddListener(FriendInputTextValidation);
		_friendNameInputField.onSubmit.AddListener(delegate
		{
			SubmitFriendRequest();
		});
	}

	private void OnDestroy()
	{
		_friendNameInputField.onValueChanged.RemoveAllListeners();
		_friendNameInputField.onSubmit.RemoveAllListeners();
	}

	private void FriendInputTextValidation(string enteredInputValue)
	{
		if (!_usernameValidator.ValidUsername(enteredInputValue))
		{
			UpdateMessageDisplay(displayText: true, "Social/Friends/UI/Errors/InvitePrompt/InvalidRequest");
		}
		else
		{
			_messageText.gameObject.UpdateActive(active: false);
		}
	}

	private void UpdateMessageDisplay(bool displayText, string textToDisplay = null)
	{
		if (!displayText || string.IsNullOrEmpty(textToDisplay))
		{
			_messageText.gameObject.UpdateActive(active: false);
			return;
		}
		_messageText.SetText(textToDisplay);
		_messageText.gameObject.UpdateActive(active: true);
	}

	private void SubmitFriendRequest()
	{
		if (!_usernameValidator.ValidUsername(_friendNameInputField.text))
		{
			UpdateMessageDisplay(displayText: true, "Social/Friends/UI/Errors/InvitePrompt/InvalidRequest");
			_friendNameInputField.Select();
		}
		else
		{
			UpdateMessageDisplay(displayText: true, "Social/Friends/UI/Confirmation/AddFriendRequest_Sent");
			_socialManager.SubmitFriendInviteOutgoing(_friendNameInputField.text);
		}
	}

	public void CloseInvitePanel()
	{
		Object.Destroy(base.gameObject);
	}
}
