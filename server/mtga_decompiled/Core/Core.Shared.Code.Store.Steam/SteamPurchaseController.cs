using System.Linq;
using UnityEngine.Purchasing;
using Wizards.Arena.Client.Logging;
using Wizards.Mtga.Store;
using _3rdParty.Steam;

namespace Core.Shared.Code.Store.Steam;

public class SteamPurchaseController : IAPPurchaseController
{
	public static string Currency
	{
		get
		{
			string isoCurrencyCode = _3rdParty.Steam.Steam.GetIsoCurrencyCode();
			if (!string.IsNullOrWhiteSpace(isoCurrencyCode) && StoreUtils.XsollaSupportedCurrencyCodes.Contains(isoCurrencyCode))
			{
				return isoCurrencyCode;
			}
			return StoreUtils.CurrencyCodeForCountry(_3rdParty.Steam.Steam.GetIsoRegionCode());
		}
	}

	public SteamPurchaseController(ILogger logger)
		: base(logger)
	{
		logger.Info("Creating SteamPurchaseController");
		UnityIAPServices.AddNewCustomStore(new SteamStoreWrapper(_logger));
		UnityIAPServices.SetStoreAsDefault("Steam");
	}
}
