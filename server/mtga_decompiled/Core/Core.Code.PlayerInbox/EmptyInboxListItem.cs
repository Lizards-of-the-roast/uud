using System.Collections.Generic;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Loc;

namespace Core.Code.PlayerInbox;

public class EmptyInboxListItem : MonoBehaviour
{
	[SerializeField]
	private Localize text;

	private PlayerInboxDataProvider _inboxDataProvider;

	private bool _cachedInError;

	private PlayerInboxDataProvider InboxDataProvider => _inboxDataProvider ?? Pantry.Get<PlayerInboxDataProvider>();

	private void Start()
	{
		InboxDataProvider.RegisterForLetterChanges(LetterChanges);
	}

	private void OnDestroy()
	{
		InboxDataProvider.UnRegisterForLetterChanges(LetterChanges);
	}

	private void LetterChanges(PlayerInboxDataProvider.LetterDataChange changeType, List<Client_Letter> letter)
	{
		bool flag = changeType == PlayerInboxDataProvider.LetterDataChange.Error;
		if (flag != _cachedInError)
		{
			string key = (flag ? "PlayerInbox/Messages_In_Error" : "PlayerInbox/NoMessages");
			text.SetText(key);
			_cachedInError = flag;
		}
	}
}
