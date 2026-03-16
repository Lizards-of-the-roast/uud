using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class ImGUIMatchView
{
	private readonly HeadlessClient _client;

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly ImGUIRequestView _requestView;

	private Vector2 _scrollPos = Vector2.zero;

	private bool _userPresenceToggled;

	private bool _emoteSelectionToggled;

	private string _emoteInput = "Phrase_Basic_Hello";

	private HashSet<string> _sentEmotes = new HashSet<string>();

	public ImGUIMatchView(HeadlessClient hc, ICardDatabaseAdapter cardDatabase)
	{
		_client = hc;
		_cardDatabase = cardDatabase ?? NullCardDatabaseAdapter.Default;
		_requestView = new ImGUIRequestView(_cardDatabase);
	}

	public void Render()
	{
		MtgGameState gameState = _client.GameState;
		GUILayout.Label(MatchConnectionState(_client));
		RenderConcedeControls();
		GUILayout.Space(5f);
		RenderSettingsControls();
		RenderStrategyControls();
		GUILayout.Space(5f);
		if (!_client.StrategyEnabled)
		{
			_scrollPos = GUILayout.BeginScrollView(_scrollPos);
			_requestView.Render(_client.PendingInteraction, gameState);
			RenderPlayerPresence(gameState);
			RenderEmoteSelection();
			GUILayout.EndScrollView();
		}
	}

	private void RenderStrategyControls()
	{
		if (GUILayout.Button(_client.StrategyEnabled ? "Take Control" : "Run AI"))
		{
			_client.StrategyEnabled = !_client.StrategyEnabled;
		}
	}

	private void RenderSettingsControls()
	{
		bool flag = false;
		if (_client.CurrentSettings != null)
		{
			flag = _client.CurrentSettings.FullControlEnabled();
		}
		if (GUILayout.Button(flag ? "Disable Full Control" : "Enable Full Control"))
		{
			_client.EnableFullControl(!flag);
		}
	}

	private void RenderConcedeControls()
	{
		if (GUILayout.Button("Familiar Concede Game"))
		{
			_client.Concede(MatchScope.Game);
		}
		if (_client.GameState.GameInfo.MatchWinCondition != MatchWinCondition.SingleElimination && GUILayout.Button("Familiar Concede Match"))
		{
			_client.Concede(MatchScope.Match);
		}
	}

	private void RenderEmoteSelection()
	{
		GUILayout.BeginVertical(GUI.skin.box);
		_emoteSelectionToggled = GUILayout.Toggle(_emoteSelectionToggled, "Show Emote Selection");
		if (_emoteSelectionToggled)
		{
			GUILayout.Label("Select an emote to dispatch.");
			_emoteInput = GUILayout.TextField(_emoteInput);
			if (GUILayout.Button("Send Emote"))
			{
				_client.Gre.SubmitUIMessage(EmoteUIMessage(_emoteInput));
				_sentEmotes.Add(_emoteInput);
			}
			if (_sentEmotes.Count > 0)
			{
				GUILayout.Space(3f);
				GUILayout.Label("Previously sent emotes");
				foreach (string sentEmote in _sentEmotes)
				{
					if (GUILayout.Button(sentEmote))
					{
						_client.Gre.SubmitUIMessage(EmoteUIMessage(sentEmote));
					}
				}
			}
		}
		GUILayout.EndVertical();
	}

	private void RenderPlayerPresence(MtgGameState gameState)
	{
		GUILayout.BeginVertical(GUI.skin.box);
		_userPresenceToggled = GUILayout.Toggle(_userPresenceToggled, "Show User Presence");
		if (_userPresenceToggled)
		{
			GUILayout.Label("Click a card to simulate a player presence hover message.");
			foreach (KeyValuePair<uint, MtgCardInstance> visibleCard in gameState.VisibleCards)
			{
				if (GUILayout.RepeatButton($"{visibleCard.Key}: {_cardDatabase.GreLocProvider.GetLocalizedText(visibleCard.Value.TitleId)}"))
				{
					_client.Gre.SubmitUIMessage(HoverUIMessage(visibleCard.Key));
				}
			}
		}
		GUILayout.EndVertical();
	}

	private static string MatchConnectionState(HeadlessClient hc)
	{
		return hc?.ConnectionState ?? "NULL CLIENT";
	}

	private static UIMessage HoverUIMessage(uint id)
	{
		return new UIMessage
		{
			OnHover = new OnHover
			{
				ObjectId = id
			}
		};
	}

	private static UIMessage EmoteUIMessage(string text)
	{
		return new UIMessage
		{
			OnChat = new OnChat
			{
				Text = text
			}
		};
	}
}
