using System.Collections.Generic;
using AK.Wwise;
using MTGA.Social;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga;
using Wotc.Mtga.Loc;

namespace Wizards.Arena.Gathering;

public class GatheringInviteOutgoingTile : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _username;

	[SerializeField]
	private Localize _labelDateSent;

	[SerializeField]
	private Button _buttonCancel;

	[SerializeField]
	private AK.Wwise.Event _cancelRequestAudioEvent;

	private ISocialManager _socialManager;

	public Invite Invite { get; private set; }

	private void Awake()
	{
		_socialManager = Pantry.Get<ISocialManager>();
		_buttonCancel.onClick.AddListener(OnCancel);
	}

	public void Init(Invite invite)
	{
		Invite = invite;
		_username.text = invite.PotentialFriend.DisplayName;
		MTGALocalizedString text = new MTGALocalizedString
		{
			Key = "Social/Friends/UI/Details/TimeSent",
			Parameters = new Dictionary<string, string> { 
			{
				"timeStamp",
				Invite.CreatedAt.ToString("d")
			} }
		};
		_labelDateSent.SetText(text);
	}

	private void OnCancel()
	{
		AudioManager.PlayAudio(_cancelRequestAudioEvent.Name, base.gameObject);
		_socialManager.RevokeFriendInviteOutgoing(Invite);
	}

	private void OnDestroy()
	{
		Invite = null;
		_buttonCancel.onClick.RemoveListener(OnCancel);
	}
}
