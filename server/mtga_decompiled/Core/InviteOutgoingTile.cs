using System;
using System.Collections.Generic;
using MTGA.Social;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class InviteOutgoingTile : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _labelName;

	[SerializeField]
	private Localize _labelDateSent;

	[SerializeField]
	private Button _buttonCancel;

	public Action<Invite> Callback_Reject;

	private Transform _dropdownSortRoot;

	public Invite Invite { get; private set; }

	private void Awake()
	{
		_buttonCancel.onClick.AddListener(delegate
		{
			AudioManager.PlayAudio("sfx_ui_friends_negate", base.gameObject);
			Callback_Reject?.Invoke(Invite);
		});
	}

	public void Init(Invite invite)
	{
		base.gameObject.UpdateActive(active: true);
		Invite = invite;
		_labelName.text = invite.PotentialFriend.DisplayName;
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

	public void Cleanup()
	{
		Invite = null;
		Callback_Reject = null;
	}
}
