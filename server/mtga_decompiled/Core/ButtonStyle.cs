using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class ButtonStyle : AnimationTrigger
{
	public enum StyleType
	{
		None = 0,
		Main = 1,
		Outlined = 2,
		OutlinedSmall = 12,
		MultiZone = 13,
		Tepid = 9,
		Tepid_NoGlow = 14,
		Secondary = 5,
		Escalated = 6,
		Waiting = 7,
		OpponentsTurn = 8,
		ToggleOn = 10,
		ToggleOff = 11
	}

	private struct CachedRectTransformProperties
	{
		public Vector2 AnchorMin;

		public Vector2 AnchorMax;

		public Vector2 Pivot;

		public Vector2 AnchoredPosition;

		public Vector2 OffsetMin;

		public Vector2 OffsetMax;

		public CachedRectTransformProperties(RectTransform rectTransform)
		{
			AnchorMin = rectTransform.anchorMin;
			AnchorMax = rectTransform.anchorMax;
			Pivot = rectTransform.pivot;
			AnchoredPosition = rectTransform.anchoredPosition;
			OffsetMin = rectTransform.offsetMin;
			OffsetMax = rectTransform.offsetMax;
		}
	}

	[SerializeField]
	private Localize _primaryText;

	private RectTransform _textRect;

	private CachedRectTransformProperties _cachedTextRectProperties;

	private RectTransform _childViewTransform;

	private Image _iconImage;

	private Animator _animator;

	private bool? _enabled;

	private void Awake()
	{
		_textRect = _primaryText?.GetComponent<RectTransform>();
		if (_textRect != null)
		{
			_cachedTextRectProperties = new CachedRectTransformProperties(_textRect);
		}
		_animator = GetComponent<Animator>();
	}

	public void ResetAnimator()
	{
		if (_animator != null)
		{
			_animator.Rebind();
		}
		_enabled = null;
	}

	public void SetText(MTGALocalizedString key, Sprite sprite)
	{
		if (sprite != null)
		{
			if (_iconImage == null)
			{
				GameObject gameObject = new GameObject("Icon");
				gameObject.transform.SetParent(base.transform);
				gameObject.transform.ZeroOut();
				_iconImage = gameObject.AddComponent<Image>();
				float num = _textRect.rect.height - _textRect.rect.height * 0.2f;
				_iconImage.rectTransform.sizeDelta = new Vector2(num, num);
			}
			_iconImage.sprite = sprite;
		}
		else if (_iconImage != null)
		{
			Object.Destroy(_iconImage.gameObject);
		}
		SetText(key, (_iconImage == null) ? null : _iconImage.rectTransform);
	}

	public void SetText(MTGALocalizedString key, RectTransform childViewTransform)
	{
		if (_childViewTransform != null && _childViewTransform != childViewTransform)
		{
			Object.Destroy(_childViewTransform.gameObject);
		}
		_childViewTransform = childViewTransform;
		if (_childViewTransform != null)
		{
			_childViewTransform.SetParent(base.transform);
			_childViewTransform.ZeroOut();
		}
		if (_primaryText != null)
		{
			_primaryText.SetText(key);
		}
		bool flag = _primaryText != null;
		bool flag2 = childViewTransform != null;
		if (flag && flag2)
		{
			childViewTransform.anchorMin = new Vector2(0f, 0.5f);
			childViewTransform.anchorMax = new Vector2(0f, 0.5f);
			childViewTransform.pivot = new Vector2(0f, 0.5f);
			childViewTransform.anchoredPosition = new Vector2(_cachedTextRectProperties.OffsetMin.x, 0f);
			RectTransform component = _primaryText.GetComponent<RectTransform>();
			component.anchorMin = new Vector2(0f, 0f);
			component.anchorMax = new Vector2(1f, 1f);
			component.pivot = new Vector2(0.5f, 0.5f);
			component.anchoredPosition = new Vector2(0f, 0f);
			component.offsetMin = new Vector2(_cachedTextRectProperties.OffsetMin.x + childViewTransform.rect.width, _cachedTextRectProperties.OffsetMin.y);
			component.offsetMax = new Vector2(_cachedTextRectProperties.OffsetMax.x, _cachedTextRectProperties.OffsetMax.y);
		}
		else if (flag)
		{
			RectTransform component2 = _primaryText.GetComponent<RectTransform>();
			component2.anchorMin = _cachedTextRectProperties.AnchorMin;
			component2.anchorMax = _cachedTextRectProperties.AnchorMax;
			component2.pivot = _cachedTextRectProperties.Pivot;
			component2.anchoredPosition = _cachedTextRectProperties.AnchoredPosition;
			component2.offsetMin = _cachedTextRectProperties.OffsetMin;
			component2.offsetMax = _cachedTextRectProperties.OffsetMax;
		}
		else if (flag2)
		{
			childViewTransform.anchorMin = new Vector2(0.5f, 0.5f);
			childViewTransform.anchorMax = new Vector2(0.5f, 0.5f);
			childViewTransform.pivot = new Vector2(0.5f, 0.5f);
			childViewTransform.anchoredPosition = new Vector2(0f, 0f);
		}
	}

	public void SetIsEnabled(bool isEnabled)
	{
		if (!_enabled.HasValue || _enabled.Value != isEnabled)
		{
			_enabled = isEnabled;
			TriggerType triggerType = (isEnabled ? TriggerType.OnEnable : TriggerType.OnDisable);
			if (delegates.Exists((Entry x) => x.type == triggerType))
			{
				Execute(triggerType);
			}
			else if (_animator != null && _animator.isActiveAndEnabled)
			{
				_animator.SetBool("Disabled", !isEnabled);
			}
		}
	}
}
