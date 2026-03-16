using System.Collections.Generic;
using Core.Meta.NewPlayerExperience.Graph;
using EventPage.Components.NetworkModels;
using Wizards.Mtga;

namespace Wotc.Mtga.Events;

public class ColorChallengeUxInfo : BasicEventUXInfo
{
	public override int DisplayPriority
	{
		get
		{
			if (!Pantry.Get<NewPlayerExperienceStrategy>().GraduatedSparkQueue.Result)
			{
				return 93;
			}
			foreach (KeyValuePair<string, IColorChallengeTrack> track in Pantry.Get<IColorChallengeStrategy>().Tracks)
			{
				track.Deconstruct(out var _, out var value);
				if (!value.Completed)
				{
					return 93;
				}
			}
			return -1;
		}
	}

	public ColorChallengeUxInfo(EventUXInfo uxInfo)
		: base(uxInfo)
	{
	}
}
