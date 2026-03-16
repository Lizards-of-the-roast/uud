using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.DuelScene;

public class ImGUIRequestView
{
	private readonly ICardDatabaseAdapter _cardDatabase;

	private Vector2 _scrollVal = Vector2.zero;

	private BaseUserRequest prvReq;

	private BaseUserRequestDebugRendererFactory _baseUserRequestDebugRendererFactory;

	private BaseUserRequestDebugRenderer _currentDebugRenderer;

	public ImGUIRequestView(ICardDatabaseAdapter cardDatabase)
	{
		_cardDatabase = cardDatabase ?? NullCardDatabaseAdapter.Default;
		_baseUserRequestDebugRendererFactory = new BaseUserRequestDebugRendererFactory(_cardDatabase);
	}

	public void Render(BaseUserRequest request, MtgGameState gameState)
	{
		if (request != null && gameState != null)
		{
			GUILayout.Label($"Current Request: {request}");
			GUILayout.Space(5f);
			GUILayout.BeginVertical(GUI.skin.box);
			GUILayout.BeginHorizontal();
			RenderAutoRespondButton(request);
			RenderCancelButton(request);
			GUILayout.EndHorizontal();
			GUILayout.Space(5f);
			_scrollVal = GUILayout.BeginScrollView(_scrollVal);
			SetRequest(request, gameState);
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
		}
	}

	private void SetRequest(BaseUserRequest request, MtgGameState gameState)
	{
		if (prvReq != request)
		{
			_currentDebugRenderer = _baseUserRequestDebugRendererFactory.CreateDebugRendererForRequest(request, gameState);
		}
		prvReq = request;
		if (_currentDebugRenderer != null)
		{
			_currentDebugRenderer.Render();
		}
		else
		{
			GUILayout.Label(request?.ToString() + " is not implemented. Be brave and do it. We believe in you.");
		}
	}

	private void RenderAutoRespondButton(BaseUserRequest request)
	{
		if (GUILayout.Button("Auto-Handle Request"))
		{
			request.AutoRespond();
		}
	}

	private static void RenderCancelButton(BaseUserRequest request)
	{
		bool enabled = GUI.enabled;
		GUI.enabled = request.CanCancel;
		if (GUILayout.Button("Send Cancel"))
		{
			request.Cancel();
		}
		GUI.enabled = enabled;
	}
}
