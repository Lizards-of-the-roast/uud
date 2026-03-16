using System.Linq;
using UnityEngine;
using Wizards.GeneralUtilities.ObjectCommunication;
using Wotc.Mtga.Client.Models.Mercantile;

namespace Wizards.DynamicTimelineBinding;

[CreateAssetMenu(menuName = "Beacon/Store Page Pack Price Filter", fileName = "Store Page Pack Price Filter", order = 21)]
public class StoreItemBasePriceFilter : BindingFilter
{
	[SerializeField]
	private Client_PurchaseCurrencyType _currencyType;

	[SerializeField]
	private int _packPrice;

	[SerializeField]
	private bool _checkParents;

	public override Object[] Filter(BeaconIdentifier beaconIdentifier)
	{
		return (from storeItemBase in beaconIdentifier.GetBeaconObject<StoreItemBase>(_checkParents)
			where storeItemBase._storeItem.PurchaseOptions.Exists((Client_PurchaseOption purchaseOption) => purchaseOption.CurrencyType == _currencyType && purchaseOption.Price == _packPrice)
			select storeItemBase).ToArray();
	}
}
