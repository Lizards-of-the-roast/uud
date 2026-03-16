using TMPro;
using UnityEngine;

public class BrowserCardHeader : MonoBehaviour
{
	public class BrowserCardHeaderData
	{
		public string HeaderText { get; private set; }

		public string SubheaderText { get; private set; }

		public BrowserCardHeaderData(string headerText, string subheaderText)
		{
			HeaderText = headerText;
			SubheaderText = subheaderText;
		}
	}

	[SerializeField]
	private TextMeshProUGUI _topText;

	[SerializeField]
	private TextMeshProUGUI _bottomText;

	[SerializeField]
	private RectTransform _canvasRectTransform;

	private float _originalCanvasWidth;

	private void Awake()
	{
		_originalCanvasWidth = _canvasRectTransform.rect.width;
	}

	public void SetText(BrowserCardHeaderData cardInfoData)
	{
		if (string.IsNullOrEmpty(cardInfoData.SubheaderText))
		{
			_topText.gameObject.SetActive(value: false);
			_bottomText.text = cardInfoData.HeaderText;
		}
		else
		{
			_topText.gameObject.SetActive(value: true);
			_topText.text = cardInfoData.HeaderText;
			_bottomText.text = cardInfoData.SubheaderText;
		}
	}

	public void SetCanvasWidth(float width)
	{
		_canvasRectTransform.sizeDelta = new Vector2(width, _canvasRectTransform.rect.height);
	}

	public void RestoreOriginalCanvasWidth()
	{
		SetCanvasWidth(_originalCanvasWidth);
	}
}
