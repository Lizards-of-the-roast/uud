using System.Collections;
using Wizards.Arena.Client.Logging;
using Wizards.Mtga;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Loc;

public class InactiveStoreManager : StoreManager
{
	private readonly IClientLocProvider _locProvider;

	public InactiveStoreManager(IAccountClient accountClient, ILogger logger, IBILogger biLogger, IClientLocProvider locProvider)
		: base(accountClient, logger, biLogger)
	{
		_locProvider = locProvider;
	}

	public override IEnumerator ProcessRMTListingsYield()
	{
		yield return null;
	}

	public override IEnumerator PurchaseRMTItemYield(StoreItem item)
	{
		SceneLoader.GetSceneLoader().SystemMessages.ShowOk(_locProvider.GetLocalizedText("MainNav/Store/Store_Unavailable_Title"), _locProvider.GetLocalizedText("MainNav/Store/Store_Unavailable"));
		yield return null;
	}
}
