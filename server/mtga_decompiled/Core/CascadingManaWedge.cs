using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;

public class CascadingManaWedge : MonoBehaviour
{
	[SerializeField]
	private RectTransform _anchor;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private TextMeshProUGUI quantityText;

	[SerializeField]
	private Image colorBacking;

	[SerializeField]
	private CustomButton button;

	private AssetLoader.AssetTracker<Sprite> _iconImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("ManaWergeIconImageSprite");

	private Action<int> _onClick;

	private Action<int> _onHover;

	private Action _onExit;

	private int _index;

	private void OnClickInternal()
	{
		_onClick?.Invoke(_index);
	}

	private void OnHoverInternal()
	{
		_onHover?.Invoke(_index);
	}

	private void OnExitInternal()
	{
		_onExit?.Invoke();
	}

	private void Awake()
	{
		button.OnClick.AddListener(OnClickInternal);
		button.OnMouseover.AddListener(OnHoverInternal);
		button.OnMouseoff.AddListener(OnExitInternal);
	}

	public void SetVisuals(string spritePath, Color tint, Vector3 angle)
	{
		AssetLoaderUtils.TrySetSprite(iconImage, _iconImageSpriteTracker, spritePath);
		colorBacking.color = tint;
		Position(angle);
	}

	public void SetIndexAndEvents(int index, Action<int> onClick, Action<int> onHover, Action onExit)
	{
		_index = index;
		_onClick = onClick;
		_onHover = onHover;
		_onExit = onExit;
	}

	private void Position(Vector3 angle)
	{
		base.transform.localEulerAngles = angle;
		iconImage.transform.localEulerAngles = angle * -1f;
	}

	public void HideQuantity()
	{
		quantityText.gameObject.UpdateActive(active: false);
	}

	public void SetQuantity(uint value)
	{
		quantityText.gameObject.UpdateActive(active: true);
		quantityText.text = value.ToString();
	}

	public void Cleanup()
	{
		AssetLoaderUtils.CleanupImage(iconImage, _iconImageSpriteTracker);
		base.transform.ZeroOut();
		_anchor.transform.ZeroOut();
	}
}
