using System;
using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.Emotes;

public class NullEmoteControllerProvider : IEmoteControllerProvider
{
	public static readonly NullEmoteControllerProvider Default = new NullEmoteControllerProvider();

	public IEmoteController GetEmoteControllerByPlayerType(GREPlayerNum playerType)
	{
		return null;
	}

	public IEmoteController GetEmoteControllerById(uint id)
	{
		return null;
	}

	public IEnumerable<IEmoteController> GetAllEmoteControllers()
	{
		return Array.Empty<IEmoteController>();
	}
}
