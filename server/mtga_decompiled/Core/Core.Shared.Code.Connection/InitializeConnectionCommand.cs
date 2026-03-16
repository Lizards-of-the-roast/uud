using AssetLookupTree;
using Core.Code.AssetLookupTree.AssetLookup;
using Core.Code.Input;
using MTGA.KeyboardManager;
using Wizards.Arena.Client.Logging;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Mtga.Logging;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Shared.Code.Connection;

public class InitializeConnectionCommand
{
	public void ExecuteFromPantry()
	{
		Execute(Pantry.Get<AssetLookupManager>().AssetLookupSystem, Pantry.Get<FrontDoorConnectionManager>(), Pantry.Get<IFrontDoorConnectionServiceWrapper>(), Pantry.Get<MatchManager>(), Pantry.Get<Matchmaking>(), Pantry.Get<EventManager>(), Pantry.Get<KeyboardManager>(), Pantry.Get<IActionSystem>(), Pantry.Get<SettingsMenuHost>());
	}

	public void Execute(AssetLookupSystem assetLookupSystem, FrontDoorConnectionManager fdConnectionManager, IFrontDoorConnectionServiceWrapper connectionServiceWrapper, MatchManager matchManager, Matchmaking matchmaking, EventManager eventManager, KeyboardManager keyboardManager, IActionSystem actions, SettingsMenuHost settingsMenuHost)
	{
		ConnectionManager connectionManager = Pantry.Get<ConnectionManager>();
		ConnectionStatusResponder connectionStatusResponder = Pantry.Get<ConnectionStatusResponder>();
		IActiveMatchesServiceWrapper activeMatchesServiceWrapper = Pantry.Get<IActiveMatchesServiceWrapper>();
		connectionStatusResponder.Initialize(connectionManager, fdConnectionManager, Languages.ActiveLocProvider, SystemMessageManager.Instance);
		ConnectionIndicator connectionIndicator = Pantry.Get<ConnectionIndicator>();
		connectionIndicator.Initialize(assetLookupSystem, new UnityLogger("ConnectionIndicator", LoggerLevel.Error));
		IAccountClient accountClient = Pantry.Get<IAccountClient>();
		IBILogger biLogger = Pantry.Get<IBILogger>();
		connectionManager.Init(fdConnectionManager, connectionServiceWrapper, activeMatchesServiceWrapper, accountClient, matchManager, matchmaking, eventManager, assetLookupSystem, keyboardManager, actions, biLogger, settingsMenuHost, connectionStatusResponder, connectionIndicator);
		connectionStatusResponder.transform.SetParent(connectionManager.transform);
		connectionIndicator.transform.SetParent(connectionManager.transform);
	}
}
