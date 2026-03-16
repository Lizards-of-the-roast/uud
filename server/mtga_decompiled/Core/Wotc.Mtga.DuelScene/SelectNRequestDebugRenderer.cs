using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class SelectNRequestDebugRenderer : BaseUserRequestDebugRenderer<SelectNRequest>
{
	private List<uint> _selections;

	private int? weight;

	private CancelRequestDebugRenderer _cancelRequestDebugRenderer;

	private MtgGameState _gameState;

	private ICardDatabaseAdapter _cardDatabase;

	public SelectNRequestDebugRenderer(SelectNRequest request, MtgGameState gameState, ICardDatabaseAdapter cardDatabase)
		: base(request)
	{
		_gameState = gameState;
		_cardDatabase = cardDatabase;
		_selections = new List<uint>();
		_cancelRequestDebugRenderer = new CancelRequestDebugRenderer(_request);
		if (request.Weights != null && request.Weights.Count > 0)
		{
			weight = 0;
		}
	}

	public override void Render()
	{
		GUI.enabled = _request.CanCancel;
		_cancelRequestDebugRenderer.Render();
		GUI.enabled = true;
		if (_request.IsStackingDecision)
		{
			if (GUILayout.Button("Submit Stacking Response"))
			{
				if (_request.Context == SelectionContext.TriggeredAbility)
				{
					_request.SubmitArbitrary();
				}
				else
				{
					_request.SubmitSelection(_request.Ids[0]);
				}
			}
			return;
		}
		GUI.enabled = _selections.Count >= _request.MinSel && _selections.Count <= _request.MaxSel;
		if (weight.HasValue)
		{
			GUI.enabled = weight >= _request.MinSel && weight <= _request.MaxSel;
		}
		if (GUILayout.Button("Submit"))
		{
			_request.SubmitSelection(_selections);
		}
		GUI.enabled = true;
		foreach (uint id in _request.Ids)
		{
			bool num = _selections.Contains(id);
			GUI.color = (num ? UnityEngine.Color.green : UnityEngine.Color.white);
			GUI.enabled = num || _selections.Count < _request.MaxSel;
			if (_gameState.TryGetEntity(id, out var mtgEntity))
			{
				if (mtgEntity is MtgCardInstance mtgCardInstance)
				{
					string text = _cardDatabase.GreLocProvider.GetLocalizedText(mtgCardInstance.TitleId);
					int num2 = _request.Ids.IndexOf(id);
					int? num3 = null;
					if (num2 != -1 && _request.Weights.Count > num2)
					{
						num3 = _request.Weights[num2];
						text += $"W:{num3}";
					}
					if (GUILayout.Button(text))
					{
						if (_selections.Contains(id))
						{
							if (num3.HasValue)
							{
								weight -= num3;
							}
							_selections.Remove(id);
						}
						else
						{
							if (num3.HasValue)
							{
								weight += num3;
							}
							_selections.Add(id);
						}
					}
				}
				else if (mtgEntity is MtgPlayer mtgPlayer && GUILayout.Button(mtgPlayer.IsLocalPlayer ? "LocalPlayer" : "Opponent"))
				{
					if (_selections.Contains(id))
					{
						_selections.Remove(id);
					}
					else
					{
						_selections.Add(id);
					}
				}
			}
			GUI.enabled = true;
			GUI.color = UnityEngine.Color.white;
		}
	}
}
