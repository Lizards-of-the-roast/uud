using System;
using System.Reflection;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class SparkyChatterUXEvent : UXEvent
{
	private const string WWISE_PCK_FIELD = "PCK";

	private const string NPE_PCK_NAME = "NPE_VO";

	private readonly IClientLocProvider _clientLocProvider;

	private readonly EntityDialogController _dialogController;

	private readonly ChatterPair _chatterPair;

	private const float MinPlayback = 0.5f;

	private float _minPlaybackTimer;

	public override bool IsBlocking { get; }

	public SparkyChatterUXEvent(IClientLocProvider clientLocProvider, EntityDialogController dialogController, ChatterPair chatterPair, bool isBlocking = false)
	{
		_clientLocProvider = clientLocProvider;
		_dialogController = dialogController;
		_chatterPair = chatterPair;
		IsBlocking = isBlocking;
	}

	public override void Execute()
	{
		if (ShouldPlayAudio(_chatterPair) && _dialogController.AllowSparkyChatter)
		{
			_dialogController.ShowSparkyChatter(_clientLocProvider.GetLocalizedText(_chatterPair.localizationTerm));
			_dialogController.PlaySparkyAudio(_chatterPair.audioEvent);
		}
		else
		{
			Complete();
		}
	}

	public override void Update(float dt)
	{
		base.Update(dt);
		if (_minPlaybackTimer < 0.5f)
		{
			_minPlaybackTimer += dt;
		}
		else if (!_dialogController.IsAudioPlaying())
		{
			Complete();
		}
	}

	private static bool ShouldPlayAudio(ChatterPair chatterPair)
	{
		if (Languages.CurrentLanguage == "en-US")
		{
			return true;
		}
		Type nestedType = typeof(WwiseMapping).GetNestedType(chatterPair.audioEvent.WwiseEventName, BindingFlags.Public);
		if (nestedType == null)
		{
			return false;
		}
		return nestedType.GetField("PCK").GetValue(null).ToString() == "NPE_VO";
	}
}
