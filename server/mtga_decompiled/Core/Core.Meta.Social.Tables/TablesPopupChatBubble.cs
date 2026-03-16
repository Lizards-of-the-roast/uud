using System;
using MTGA.Social;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.Models.Network;
using Wizards.Mtga;

namespace Core.Meta.Social.Tables;

public class TablesPopupChatBubble : MonoBehaviour
{
	private Lazy<ChallengeMessageConverter> _messageConverter = new Lazy<ChallengeMessageConverter>();

	[SerializeField]
	private TMP_Text _playerNameText;

	[SerializeField]
	private Image _playerNameBubble;

	[SerializeField]
	private TMP_Text _messageText;

	[SerializeField]
	private Image _messageBubble;

	private IAccountClient AccountClient => Pantry.Get<IAccountClient>();

	public void SetTextAndBubbles(LobbyPlayer playerInfo, string message)
	{
		string sourceText = playerInfo?.DisplayName ?? "Unknown Player";
		Color color = TableUtils.TablesColorForPlayer(AccountClient, playerInfo);
		_playerNameText.SetText(sourceText);
		_playerNameBubble.color = color;
		_messageBubble.color = color;
		_messageText.SetText(message);
	}
}
