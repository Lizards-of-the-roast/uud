using System.Collections.Generic;
using System.Linq;
using Core.Meta.MainNavigation.Store;
using UnityEngine;
using Wizards.Mtga.FrontDoorModels;

namespace Core.Rewards;

public abstract class VanityItemReward<T, P> : ItemReward<T, P>, IVanityItemReward where P : Component
{
	public abstract string VanityItemPrefix { get; }

	public abstract void AddVanityItem(string name);

	public override void AddFromInventoryUpdate(ClientInventoryUpdateReportItem inventoryUpdate, PrecalculatedRewardUpdateInfo cache)
	{
		IEnumerable<string> vanityItemsAdded = inventoryUpdate.delta.vanityItemsAdded;
		foreach (string item in vanityItemsAdded ?? Enumerable.Empty<string>())
		{
			VanityItemRewardUtil.AddVanityItemIfMatch(this, item);
		}
	}
}
