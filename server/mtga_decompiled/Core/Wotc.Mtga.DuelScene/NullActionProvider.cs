using System;
using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class NullActionProvider : IActionProvider
{
	public static readonly IActionProvider Default = new NullActionProvider();

	public IReadOnlyList<ActionInfo> GetGameStateActions(uint instanceId)
	{
		return Array.Empty<ActionInfo>();
	}

	public IReadOnlyList<GreInteraction> GetRequestActions(uint instanceId)
	{
		return Array.Empty<GreInteraction>();
	}
}
