using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Wotc.Mtga.Extensions;

public class RewardDisplay : MonoBehaviour
{
	[FormerlySerializedAs("CountText")]
	[SerializeField]
	private TMP_Text _countText;

	[SerializeField]
	private GameObject _countShadow;

	[SerializeField]
	private GameObject _nonCountObject;

	public void SetCountText(int amount)
	{
		if (_countShadow != null)
		{
			_countShadow.UpdateActive(amount >= 1);
		}
		if (_nonCountObject != null)
		{
			_nonCountObject.UpdateActive(amount < 1);
		}
		if (_countText != null && amount >= 1)
		{
			_countText.text = amount.ToString("N0");
		}
	}

	public void TapCoins()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rewards_coins_tap, base.gameObject);
	}
}
