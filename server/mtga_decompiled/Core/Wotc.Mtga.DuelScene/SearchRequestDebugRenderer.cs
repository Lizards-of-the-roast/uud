using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.DuelScene;

public class SearchRequestDebugRenderer : BaseUserRequestDebugRenderer<SearchRequest>
{
	private ICardDatabaseAdapter _cardDatabase;

	private MtgGameState _gameState;

	private List<uint> _selections;

	private CancelRequestDebugRenderer _cancelRequestDebugRenderer;

	public SearchRequestDebugRenderer(SearchRequest request, MtgGameState gameState, ICardDatabaseAdapter cardDatabase)
		: base(request)
	{
		_gameState = gameState;
		_cardDatabase = cardDatabase;
		_selections = new List<uint>();
		_cancelRequestDebugRenderer = new CancelRequestDebugRenderer(_request);
	}

	public override void Render()
	{
		GUI.enabled = _selections.Count >= _request.Min && _selections.Count <= _request.Max;
		if (GUILayout.Button("Submit"))
		{
			_request.SubmitSelection(_selections);
		}
		GUI.enabled = true;
		GUI.enabled = _request.CanCancel;
		_cancelRequestDebugRenderer.Render();
		GUI.enabled = true;
		foreach (uint option in _request.Options)
		{
			bool flag = !_selections.Contains(option);
			if (!_gameState.TryGetEntity(option, out var mtgEntity))
			{
				continue;
			}
			if (mtgEntity is MtgCardInstance mtgCardInstance)
			{
				string text = string.Format("{0} {1}", flag ? "Select" : "UnSelect", _cardDatabase.GreLocProvider.GetLocalizedText(mtgCardInstance.TitleId));
				GUI.color = (flag ? Color.white : Color.green);
				if (GUILayout.Button(text))
				{
					if (flag)
					{
						_selections.Add(option);
					}
					else
					{
						_selections.Remove(option);
					}
				}
				GUI.color = Color.white;
			}
			else if (mtgEntity is MtgPlayer)
			{
				GUILayout.Label("WHAT WHY IS THERE A PLAYER HERE LORD HAVE MERCY WHAT DID YOU DO");
			}
		}
	}
}
