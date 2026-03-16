using System;
using System.Collections.Generic;
using Assets.Core.Shared.Code;
using Core.Code.Promises;
using Wizards.Arena.Promises;
using Wizards.Models.Renewal;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Network.ServiceWrappers;

public class RenewalManager
{
	private class RenewalDefinitiion
	{
		public readonly string id;

		public readonly DateTime initialPayoutsStartTime;

		public readonly DateTime initialPayoutsEndTime;

		public readonly bool? ready;

		public RenewalDefinitiion(DTO_CurrentRenewalResponse response)
		{
			id = response?.definitionId;
			initialPayoutsStartTime = response?.initialPayoutsStartTime ?? DateTime.MinValue;
			initialPayoutsEndTime = response?.initialPayoutsEndTime ?? DateTime.MinValue;
			ready = response?.ready;
		}
	}

	private RenewalDefinitiion _renewalDef;

	private string _mostRecentRenewal;

	private IRenewalServiceWrapper _renewalService;

	private IInventoryServiceWrapper _inventoryService;

	public void Init(IRenewalServiceWrapper renewalServiceWrapper, IInventoryServiceWrapper inventoryServiceWrapper, DTO_CurrentRenewalResponse currentRenewalResponse)
	{
		_renewalService = renewalServiceWrapper;
		_inventoryService = inventoryServiceWrapper;
		_renewalDef = new RenewalDefinitiion(currentRenewalResponse);
		_mostRecentRenewal = currentRenewalResponse?.currentId;
	}

	public bool IsCurrentRenewalUpcoming()
	{
		if (_renewalDef?.id != null)
		{
			RenewalDefinitiion renewalDef = _renewalDef;
			if (renewalDef == null || renewalDef.ready != false)
			{
				DateTime gameTime = ServerGameTime.GameTime;
				DateTime? obj = _renewalDef?.initialPayoutsStartTime;
				return gameTime < obj;
			}
			return true;
		}
		return false;
	}

	public bool IsUpcomingRenewalAvailable()
	{
		if (IsCurrentRenewalUpcoming())
		{
			return _mostRecentRenewal != _renewalDef?.id;
		}
		return false;
	}

	private bool IsCurrentRenewalActive()
	{
		if (_renewalDef?.id != null)
		{
			RenewalDefinitiion renewalDef = _renewalDef;
			if (renewalDef != null && renewalDef.ready == true)
			{
				DateTime gameTime = ServerGameTime.GameTime;
				DateTime? obj = _renewalDef?.initialPayoutsStartTime;
				if (gameTime > obj)
				{
					gameTime = ServerGameTime.GameTime;
					DateTime? obj2 = _renewalDef?.initialPayoutsEndTime;
					return gameTime < obj2;
				}
			}
		}
		return false;
	}

	public bool IsActiveRenewalAvailable()
	{
		if (_renewalDef?.id != null && _mostRecentRenewal != _renewalDef?.id)
		{
			return IsCurrentRenewalActive();
		}
		return false;
	}

	public string GetCurrentRenewalId()
	{
		return _renewalDef?.id;
	}

	public DateTime GetCurrentRenewalStartDate()
	{
		return _renewalDef?.initialPayoutsStartTime ?? DateTime.MaxValue;
	}

	private void UpdateMostRecentRenewalId()
	{
		_mostRecentRenewal = _renewalDef?.id;
	}

	public void RedeemRenewalRewards(Action<IEnumerable<ClientInventoryUpdateReportItem>> onSuccess)
	{
		_renewalService.RedeemRenewalRewards().ThenOnMainThread(delegate(Promise<DTO_RedeemRenewalRewardsResponse> promise)
		{
			if (promise.Successful)
			{
				UpdateMostRecentRenewalId();
				InventoryInfoShared inventoryInfoShared = AWSInventoryConversions.ConvertInventoryInfo(promise.Result?.inventoryInfo);
				IEnumerable<ClientInventoryUpdateReportItem> obj = inventoryInfoShared.ToUpdateReportItem();
				_inventoryService.OnInventoryInfoUpdated_AWS(inventoryInfoShared);
				onSuccess?.Invoke(obj);
			}
		});
	}
}
