using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventRewardView : MonoBehaviour
{
	[SerializeField]
	private Image _rewardImage;

	[SerializeField]
	private TMP_Text _quantityText;

	[SerializeField]
	private TMP_Text _titleText;

	public Image RewardImage => _rewardImage;

	public TMP_Text QuantityText => _quantityText;

	public TMP_Text TitleText => _titleText;
}
