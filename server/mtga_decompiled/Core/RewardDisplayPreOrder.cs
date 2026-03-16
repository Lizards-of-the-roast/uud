using AssetLookupTree;
using Core.Meta.MainNavigation.EventPageV2;
using UnityEngine;

public class RewardDisplayPreOrder : MonoBehaviour
{
	[SerializeField]
	private Transform _anchor;

	[SerializeField]
	private ContainerProxy _container;

	public BoosterVoucherView SetSku(AltAssetReference<BoosterVoucherView> preorderBundleRef)
	{
		return _container.SetInstance(preorderBundleRef);
	}
}
