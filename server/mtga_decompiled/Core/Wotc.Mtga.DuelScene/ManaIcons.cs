using System.Collections.Generic;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public static class ManaIcons
{
	public enum IconType
	{
		None,
		Riders,
		Snow,
		Treasure
	}

	public static IconType CalculateIconType(IReadOnlyCollection<RiderPromptData> riders, IReadOnlyCollection<ManaSpecType> specs)
	{
		if (specs.Contains(ManaSpecType.FromSnow))
		{
			return IconType.Snow;
		}
		if (specs.Contains(ManaSpecType.FromTreasure))
		{
			return IconType.Treasure;
		}
		if (riders.Count > 0)
		{
			return IconType.Riders;
		}
		return IconType.None;
	}
}
