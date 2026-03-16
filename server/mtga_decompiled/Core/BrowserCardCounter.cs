using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BrowserCardCounter : MonoBehaviour
{
	[SerializeField]
	public Button IncrementButton;

	[SerializeField]
	public Button DecrementButton;

	[SerializeField]
	private TMP_Text _displayText;

	[SerializeField]
	private Image _onImage;

	[SerializeField]
	private Image _offImage;

	private int _count;

	public void SetCardCount(int count)
	{
		_count = count;
		_displayText.text = count.ToString();
		_onImage.gameObject.SetActive(count > 0);
		_offImage.gameObject.SetActive(count <= 0);
	}

	public void IncrementCardCount()
	{
		SetCardCount(_count + 1);
	}

	public Vector2 GetScreenPosition()
	{
		return CurrentCamera.Value.WorldToScreenPoint(base.transform.position);
	}
}
