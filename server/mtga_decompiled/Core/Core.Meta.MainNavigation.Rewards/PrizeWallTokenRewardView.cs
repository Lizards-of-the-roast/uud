using UnityEngine;
using Wotc.Mtga.Loc;

namespace Core.Meta.MainNavigation.Rewards;

public class PrizeWallTokenRewardView : MonoBehaviour
{
	[SerializeField]
	private Localize _quantityLabel;

	public void Refresh(int quantity)
	{
		if (_quantityLabel != null)
		{
			_quantityLabel.SetText(quantity.ToString());
		}
	}
}
