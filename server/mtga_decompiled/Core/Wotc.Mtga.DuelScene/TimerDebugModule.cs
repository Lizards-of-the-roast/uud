using System;
using System.Diagnostics;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class TimerDebugModule : DebugModule, IDisposable
{
	private readonly IGameStateProvider _gameStateProvider;

	private readonly Stopwatch _stopwatch = new Stopwatch();

	private static Texture2D backgroundTexture;

	private static GUIStyle textureStyle;

	public override string Name => "Timers";

	public override string Description => "Detailed insights into match timers and timer visuals";

	public TimerDebugModule(IGameStateProvider gameStateProvider)
	{
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_gameStateProvider.CurrentGameState.ValueUpdated += OnGameStateUpdated;
	}

	public override void Render()
	{
		MtgGameState mtgGameState = _gameStateProvider.CurrentGameState;
		if (mtgGameState == null)
		{
			return;
		}
		InitializeStyles();
		foreach (MtgPlayer player in mtgGameState.Players)
		{
			DrawTimersForPlayer(player);
		}
	}

	private void InitializeStyles()
	{
		if (textureStyle == null || !(backgroundTexture != null))
		{
			backgroundTexture = Texture2D.whiteTexture;
			textureStyle = new GUIStyle
			{
				normal = new GUIStyleState
				{
					background = backgroundTexture
				}
			};
		}
	}

	private void DrawTimersForPlayer(MtgPlayer player)
	{
		GUILayout.BeginVertical(GUI.skin.box);
		GUILayout.Label(player.IsLocalPlayer ? $"Player {player.InstanceId} (LocalPlayer)" : $"Player {player.InstanceId}");
		DrawTimerForPlayer(player, TimerType.ActivePlayer);
		DrawTimerForPlayer(player, TimerType.Inactivity);
		DrawTimerForPlayer(player, TimerType.MatchClock);
		GUILayout.EndVertical();
	}

	private void DrawTimerForPlayer(MtgPlayer player, TimerType timerType)
	{
		if (player == null)
		{
			return;
		}
		MtgTimer mtgTimer = player.Timers.Find((MtgTimer x) => x.TimerType == timerType);
		if (mtgTimer == null)
		{
			GUILayout.Label("No " + EnumExtensions.EnumCleanName(timerType) + " Timer");
			return;
		}
		GUILayout.BeginVertical(GUI.skin.box);
		float num = (float)_stopwatch.ElapsedMilliseconds * 0.001f;
		LayoutBox(UnityEngine.Color.white);
		Rect lastRect = GUILayoutUtility.GetLastRect();
		Rect lastRect2 = GUILayoutUtility.GetLastRect();
		float num2 = (mtgTimer.Running ? num : 0f);
		float num3 = mtgTimer.ElapsedTime + num2;
		float num4 = mtgTimer.RemainingTime - num;
		float num5 = Mathf.Clamp01(num3 / (float)mtgTimer.TotalDuration);
		lastRect.width *= num5;
		DrawRect(color: (!mtgTimer.Running) ? UnityEngine.Color.grey : ((num4 <= (float)mtgTimer.WarningThreshold) ? UnityEngine.Color.yellow : UnityEngine.Color.green), position: lastRect);
		GUI.color = UnityEngine.Color.black;
		GUI.Label(lastRect2, mtgTimer.TimerType.ToString() + ": " + Math.Round(num3, 1));
		GUI.color = UnityEngine.Color.white;
		GUILayout.Space(2f);
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("+15"))
		{
			mtgTimer.ElapsedTime += 15f;
		}
		if (GUILayout.Button("-15"))
		{
			mtgTimer.ElapsedTime -= 15f;
		}
		GUILayout.Space(3f);
		if (mtgTimer.TimerType == TimerType.MatchClock)
		{
			uint totalDuration = mtgTimer.TotalDuration;
			if (GUILayout.Button("WarnTime"))
			{
				mtgTimer.ElapsedTime = (uint)(TimeSpan.FromSeconds(totalDuration) - TimeSpan.FromSeconds(20.0)).TotalSeconds;
				_stopwatch.Restart();
			}
			GUILayout.Space(3f);
			if (GUILayout.Button("Critical Time"))
			{
				mtgTimer.ElapsedTime = (uint)(TimeSpan.FromSeconds(totalDuration) - TimeSpan.FromSeconds(15.0)).TotalSeconds;
				_stopwatch.Restart();
			}
		}
		else if (GUILayout.Button("WarnTime"))
		{
			mtgTimer.ElapsedTime = (uint)(TimeSpan.FromSeconds(mtgTimer.TotalDuration) - TimeSpan.FromSeconds(20.0)).TotalSeconds;
			_stopwatch.Restart();
		}
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
	}

	private void DrawRect(Rect position, UnityEngine.Color color, GUIContent content = null)
	{
		UnityEngine.Color backgroundColor = GUI.backgroundColor;
		GUI.backgroundColor = color;
		GUI.Box(position, content ?? GUIContent.none, textureStyle);
		GUI.backgroundColor = backgroundColor;
	}

	private void LayoutBox(UnityEngine.Color color, GUIContent content = null)
	{
		UnityEngine.Color backgroundColor = GUI.backgroundColor;
		GUI.backgroundColor = color;
		GUILayout.Box(content ?? GUIContent.none, textureStyle, GUILayout.Height(20f));
		GUI.backgroundColor = backgroundColor;
	}

	private void OnGameStateUpdated(MtgGameState gameState)
	{
		_stopwatch.Restart();
	}

	public void Dispose()
	{
		_gameStateProvider.CurrentGameState.ValueUpdated -= OnGameStateUpdated;
	}
}
