using System;
using System.Collections;
using System.Collections.Generic;
using AssetLookupTree;
using Core.Meta.MainNavigation.Store;
using Wizards.Mtga.FrontDoorModels;

namespace Core.Rewards;

public interface IRewardBase
{
	int InstancesCount { get; }

	int ToAddCount { get; }

	IEnumerable<Func<RewardDisplayContext, IEnumerator>> DisplayRewards(ContentControllerRewards ccr);

	void AddFromInventoryUpdate(ClientInventoryUpdateReportItem inventoryUpdate, PrecalculatedRewardUpdateInfo cache);

	void ClearAdded();

	void ClearInstances();

	float GetWidth(AssetLookupSystem als);
}
