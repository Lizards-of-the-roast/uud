using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;

public class ConfirmWidgetButton : MonoBehaviour
{
	[SerializeField]
	private Button _button;

	[SerializeField]
	private TextMeshProUGUI _text;

	[SerializeField]
	private Image _img;

	private AssetLoader.AssetTracker<Sprite> _imageSpriteTracker;

	public event Action<ConfirmWidgetButton> Clicked;

	private void Awake()
	{
		_button.onClick.AddListener(OnButtonClicked);
	}

	private void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(_img, _imageSpriteTracker);
		_button.onClick.RemoveAllListeners();
		this.Clicked = null;
	}

	private void OnButtonClicked()
	{
		this.Clicked?.Invoke(this);
	}

	public void SetText(string text)
	{
		_text.SetText(text);
	}

	public void SetSprite(string spritePath)
	{
		if (_imageSpriteTracker == null)
		{
			_imageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("ConfirmWidgetButtonImageSprite");
		}
		AssetLoaderUtils.TrySetSprite(_img, _imageSpriteTracker, spritePath);
		_img.gameObject.UpdateActive(!string.IsNullOrEmpty(spritePath));
	}
}
