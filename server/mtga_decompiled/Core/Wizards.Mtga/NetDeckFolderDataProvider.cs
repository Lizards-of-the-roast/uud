using System.Collections.Generic;
using System.Linq;
using Core.Meta.Shared;
using Wizards.Arena.Promises;
using Wizards.Unification.Models.NetDeck;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Wizards.Mtga;

public class NetDeckFolderDataProvider
{
	private readonly INetDeckFolderServiceWrapper _netDeckFolderServiceWrapperWrapper;

	private List<NetDeckFolder>? _netDeckFolders;

	private bool _initialized;

	public static NetDeckFolderDataProvider Create()
	{
		return new NetDeckFolderDataProvider(Pantry.Get<INetDeckFolderServiceWrapper>());
	}

	public NetDeckFolderDataProvider(INetDeckFolderServiceWrapper netDeckFolderServiceWrapper)
	{
		_netDeckFolderServiceWrapperWrapper = netDeckFolderServiceWrapper;
	}

	public Promise<List<DTO_NetDeckFolder>> Initialize()
	{
		return _netDeckFolderServiceWrapperWrapper.GetNetDeckFolders().Then(delegate(Promise<List<DTO_NetDeckFolder>> promise)
		{
			if (promise.Successful)
			{
				_netDeckFolders = promise.Result?.Select((DTO_NetDeckFolder ndf) => new NetDeckFolder
				{
					Id = ndf.Id,
					FolderNameLocKey = ndf.FolderNameLocKey,
					FolderDescLocKey = ndf.FolderDescLocKey,
					StartDate = ndf.StartDate,
					EndDate = ndf.EndDate
				}).ToList() ?? new List<NetDeckFolder>();
				_initialized = true;
			}
			else
			{
				PromiseExtensions.Logger.Error($"Failed to get NetDeckFolders: {promise.Error}");
			}
		});
	}

	public IEnumerable<NetDeckFolder> GetNetDeckFolders()
	{
		if (!_initialized)
		{
			SimpleLog.LogError("Attempting to get NetDeckFolders before NetDeckFolderDataProvider is initialized.");
		}
		IEnumerable<NetDeckFolder> netDeckFolders = _netDeckFolders;
		return netDeckFolders ?? Enumerable.Empty<NetDeckFolder>();
	}
}
