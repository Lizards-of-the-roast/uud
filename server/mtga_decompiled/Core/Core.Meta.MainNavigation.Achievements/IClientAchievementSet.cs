using System;
using System.Collections.Generic;
using AssetLookupTree;
using UnityEngine;
using Wotc.Mtga.Wrapper;

namespace Core.Meta.MainNavigation.Achievements;

public interface IClientAchievementSet
{
	string GraphId { get; }

	int DisplayPriority { get; }

	string TitleLocalizationKey { get; }

	string Title { get; }

	CollationMapping ExpansionCode { get; }

	IList<IClientAchievementGroup> AchievementGroups { get; }

	DateTime EndTime { get; }

	DateTime EndRevealTime { get; }

	event Action SetChanged;

	Sprite GetAchievementSetIcon(AssetLookupSystem assetLookupSystem);
}
