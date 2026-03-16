using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using Wizards.Arena.Client.Logging;

namespace Core.Shared.Code.Store.Steam;

public class SteamStoreWrapper : IStoreWrapper
{
	private readonly SteamCustomStore _store;

	public UnityEngine.Purchasing.Extension.Store instance => _store;

	public string name => "Steam";

	public SteamStoreWrapper(ILogger logger)
	{
		_store = new SteamCustomStore(logger);
	}

	public ConnectionState GetStoreConnectionState()
	{
		return _store.ConnectionState;
	}
}
