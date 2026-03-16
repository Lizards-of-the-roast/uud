using System.Collections.Generic;
using System.Linq;
using Wizards.Arena.Promises;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Wizards.Mtga;

public class VoucherDataProvider
{
	private readonly IMercantileServiceWrapper _mercantileServiceWrapper;

	private Dictionary<string, Client_VoucherDefinition> _voucherDefinitions;

	private bool _initialized;

	public static VoucherDataProvider Create()
	{
		return new VoucherDataProvider(Pantry.Get<IMercantileServiceWrapper>());
	}

	public VoucherDataProvider(IMercantileServiceWrapper mercantileServiceWrapper)
	{
		_mercantileServiceWrapper = mercantileServiceWrapper;
	}

	public Promise<List<Client_VoucherDefinition>> Initialize()
	{
		return _mercantileServiceWrapper.GetVoucherDefinitions().Then(delegate(Promise<List<Client_VoucherDefinition>> promise)
		{
			if (promise.Successful)
			{
				_voucherDefinitions = promise.Result.ToDictionary((Client_VoucherDefinition voucherDef) => voucherDef.VoucherId);
				_initialized = true;
			}
			else
			{
				PromiseExtensions.Logger.Error($"Failed to get Voucher Definitions: {promise.Error}");
			}
		});
	}

	public Client_VoucherDefinition VoucherDefinitionForId(string voucherId)
	{
		if (!_initialized)
		{
			SimpleLog.LogError("Attempting to get Voucher Definitions before VoucherDataProvider is initialized.");
			return null;
		}
		if (_voucherDefinitions.TryGetValue(voucherId, out var value))
		{
			return value;
		}
		return null;
	}
}
