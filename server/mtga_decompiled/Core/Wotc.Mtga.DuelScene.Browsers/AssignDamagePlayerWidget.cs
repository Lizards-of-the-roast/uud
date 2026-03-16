using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Wotc.Mtga.DuelScene.Browsers;

public class AssignDamagePlayerWidget : MonoBehaviour
{
	[SerializeField]
	private Image _avatarImage;

	[SerializeField]
	private TMP_Text _lifeText;

	[SerializeField]
	private Vector3 _spinnerOffset = new Vector3(60f, 0f, 0f);

	public Vector3 SpinnerOffset => _spinnerOffset;

	public void SetAvatarSprite(Sprite sprite)
	{
		_avatarImage.sprite = sprite;
	}

	public void SetLifeTotal(int lifeTotal)
	{
		_lifeText.SetText(lifeTotal.ToString("N0"));
	}
}
