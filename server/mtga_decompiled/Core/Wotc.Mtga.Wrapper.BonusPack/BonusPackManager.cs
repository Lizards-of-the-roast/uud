using System;
using System.Collections.Generic;
using Core.Code.Promises;
using Wizards.Arena.Enums.System;
using Wizards.Arena.Promises;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Wotc.Mtga.Wrapper.BonusPack;

public class BonusPackManager
{
	private class BonusPackInfo
	{
		public StoreItem Listing;

		public Client_PurchaseOption Option;
	}

	private IBILogger _biLogger;

	private InventoryManager _inventory;

	private StoreManager _store;

	private IMercantileServiceWrapper _mercantile;

	private BonusPackInfo _bonusOption;

	private const string BPP_TOKEN_ID = "BonusPackProgress";

	private Dictionary<string, int> _packsToHide;

	public int ProgressCurrent
	{
		get
		{
			_inventory.Inventory.CustomTokens.TryGetValue("BonusPackProgress", out var value);
			return value;
		}
	}

	public int ProgressMax { get; private set; } = int.MaxValue;

	public bool CanRedeem => RedeemablePackCount > 0;

	public int RedeemablePackCount => ProgressCurrent / ProgressMax;

	public int RemainingPackCountToReward => Math.Max(0, ProgressMax - ProgressCurrent);

	public event Action<int, int> OnProgressCurrentChanged;

	public event Action<Dictionary<string, int>> BeforePacksPurchased;

	public event Action<InventoryInfoShared> OnPacksPurchased;

	public BonusPackManager(IBILogger biLogger, InventoryManager inventory, StoreManager store)
	{
		_biLogger = biLogger;
		_inventory = inventory;
		_store = store;
		_mercantile = Pantry.Get<IMercantileServiceWrapper>();
		_inventory.SubscribeToAll(OnInventoryUpdate);
		_store.OnDataRefreshed += OnStoreRefreshed;
	}

	public void OnDestroy()
	{
		_inventory.UnsubscribeFromAll(OnInventoryUpdate);
		_store.OnDataRefreshed -= OnStoreRefreshed;
	}

	public void Redeem()
	{
		if (!CanRedeem)
		{
			return;
		}
		int quantity = RedeemablePackCount;
		StoreItem listing = _bonusOption.Listing;
		Dictionary<string, int> obj = ExpectedGoldenPacks(listing, quantity);
		_packsToHide = null;
		this.BeforePacksPurchased?.Invoke(obj);
		_mercantile.PurchaseProduct(listing, quantity, Client_PurchaseCurrencyType.CustomToken, _bonusOption.Option.CurrencyId).ThenOnMainThread(delegate(Promise<InventoryInfoShared> promise)
		{
			if (promise.Successful)
			{
				this.OnPacksPurchased?.Invoke(promise.Result);
			}
			else
			{
				_biLogger.Send(ClientBusinessEventType.StoreError, new StoreError
				{
					EventTime = DateTime.UtcNow,
					Code = (ServerErrors)promise.Error.Code,
					ErrorMessage = $"BonusPackManager error trying to auto-redeem {quantity} bonus packs: {promise.Error.Message}",
					ElapsedMilliseconds = promise.ElapsedMilliseconds,
					PromiseState = promise.State.ToString(),
					ProductId = listing.Id
				});
			}
		});
	}

	private static Dictionary<string, int> ExpectedGoldenPacks(StoreItem listing, int quantity)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (Sku sku in listing.Skus)
		{
			if (sku.TreasureItem.TreasureType == TreasureType.Booster)
			{
				dictionary.TryGetValue(sku.TreasureItem.ReferenceId, out var value);
				value += sku.TreasureItem.Quantity * quantity;
				dictionary[sku.TreasureItem.ReferenceId] = value;
			}
		}
		return dictionary;
	}

	public void HidePacks(Dictionary<string, int> packsToHide)
	{
		_packsToHide = packsToHide;
	}

	public bool ConsumeHiddenPacks(string collationId, int quantity)
	{
		if (_packsToHide == null)
		{
			return false;
		}
		_packsToHide.TryGetValue(collationId, out var value);
		if (value == quantity)
		{
			_packsToHide = null;
			return true;
		}
		return false;
	}

	private void OnInventoryUpdate(ClientInventoryUpdateReportItem reportItem)
	{
		int num = 0;
		CustomTokenDeltaInfo[] customTokenDelta = reportItem.delta.customTokenDelta;
		foreach (CustomTokenDeltaInfo customTokenDeltaInfo in customTokenDelta)
		{
			if (!(customTokenDeltaInfo.id != "BonusPackProgress"))
			{
				num += customTokenDeltaInfo.delta;
			}
		}
		if (num != 0)
		{
			this.OnProgressCurrentChanged?.Invoke(ProgressCurrent, num);
			if (CanRedeem)
			{
				Redeem();
			}
		}
	}

	private void OnStoreRefreshed()
	{
		foreach (StoreItem value in _store.StoreListings.Values)
		{
			foreach (Client_PurchaseOption purchaseOption in value.PurchaseOptions)
			{
				if (purchaseOption.CurrencyType == Client_PurchaseCurrencyType.CustomToken && purchaseOption.CurrencyId == "BonusPackProgress")
				{
					_bonusOption = new BonusPackInfo
					{
						Listing = value,
						Option = purchaseOption
					};
					ProgressMax = purchaseOption.Price;
					return;
				}
			}
		}
		_bonusOption = null;
		ProgressMax = int.MaxValue;
	}
}
