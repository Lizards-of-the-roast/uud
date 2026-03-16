using System;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class SettingsMessageController : ISettingsMessageController, ISettingsMessageProvider, IDisposable
{
	public ObservableReference<SettingsMessage> Settings { get; } = new ObservableReference<SettingsMessage>();

	public void SetSettings(SettingsMessage settings)
	{
		Settings.Value = settings;
	}

	public void Dispose()
	{
		Settings?.Dispose();
	}
}
