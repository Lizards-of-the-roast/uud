using TMPro;
using UnityEngine;
using Wotc.Mtga.Loc;

public class RewardDisplayFlavorText : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _flavorText;

	public void SetText(string locKey)
	{
		_flavorText.text = Languages.ActiveLocProvider.GetLocalizedText(locKey);
	}
}
