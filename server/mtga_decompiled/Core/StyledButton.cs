using System;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Pooling;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wizards.Mtga;
using Wotc.Mtga.Extensions;

public class StyledButton : Button
{
	[SerializeField]
	private GameObject _warningIcon;

	private PromptButtonData _model = new PromptButtonData();

	private ButtonStyle _currentStyle;

	private bool _isActive;

	private bool _isDirty;

	public Action Rollover;

	public Action Rollout;

	private Animator _animator;

	private RectTransform _rectRoot;

	private AssetLookupSystem _assetLookupSystem;

	private IUnityObjectPool _objectPool;

	private ButtonStyle.StyleType _prefabStyleType;

	public ButtonStyle.StyleType Style => _model.Style;

	public ButtonTag Tag => _model.Tag;

	public bool HasInteraction => _model.ButtonCallback != null;

	public bool Hover { get; private set; }

	public string ButtonSFX
	{
		get
		{
			if (_model == null)
			{
				return string.Empty;
			}
			return _model.ButtonSFX;
		}
	}

	protected override void Awake()
	{
		_objectPool = Pantry.Get<IUnityObjectPool>();
		base.Awake();
		_rectRoot = base.transform as RectTransform;
		_animator = GetComponent<Animator>();
		if (_animator != null)
		{
			_animator.keepAnimatorStateOnDisable = true;
		}
		base.onClick.AddListener(delegate
		{
			if (_model.ButtonCallback != null && _model.Enabled)
			{
				_model.ButtonCallback?.Invoke();
				AudioManager.PlayAudio(_model.ButtonSFX, AudioManager.Default);
			}
		});
	}

	public void Init(AssetLookupSystem assetLookupSystem)
	{
		_assetLookupSystem = assetLookupSystem;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		_isDirty = true;
	}

	private void LateUpdate()
	{
		if (_isDirty)
		{
			Layout();
			_isDirty = false;
		}
	}

	private void Layout()
	{
		if (_currentStyle == null || _prefabStyleType != _model.Style)
		{
			_prefabStyleType = _model.Style;
			_assetLookupSystem.Blackboard.Clear();
			_assetLookupSystem.Blackboard.ButtonStyle = _model.Style;
			ButtonStylePrefab payload = _assetLookupSystem.TreeLoader.LoadTree<ButtonStylePrefab>(returnNewTree: false).GetPayload(_assetLookupSystem.Blackboard);
			if (_currentStyle != null)
			{
				_currentStyle.ResetAnimator();
				if (_objectPool != null)
				{
					_objectPool.PushObject(_currentStyle.gameObject);
				}
				else
				{
					UnityEngine.Object.Destroy(_currentStyle.gameObject);
				}
			}
			if (_objectPool != null)
			{
				_currentStyle = _objectPool.PopObject(payload.PrefabPath).GetComponent<ButtonStyle>();
			}
			else
			{
				_currentStyle = AssetLoader.Instantiate<ButtonStyle>(payload.PrefabPath);
			}
			Transform obj = _currentStyle.transform;
			obj.SetParent(_rectRoot);
			obj.SetAsFirstSibling();
			obj.ZeroOut();
			obj.localScale = Vector3.one;
			RectTransform rectTransform = obj as RectTransform;
			if (rectTransform != null && _rectRoot != null)
			{
				Vector2 vector = new Vector2(0.5f, 0.5f) - _rectRoot.pivot;
				Vector2 vector2 = new Vector2(vector.x * _rectRoot.sizeDelta.x, vector.y * _rectRoot.sizeDelta.y);
				Vector2 anchoredPosition = rectTransform.anchoredPosition + vector2;
				rectTransform.anchoredPosition = anchoredPosition;
			}
		}
		MTGALocalizedString mTGALocalizedString = _model.ButtonText;
		if (mTGALocalizedString == null || string.IsNullOrEmpty(mTGALocalizedString.Key))
		{
			mTGALocalizedString = "MainNav/General/Empty_String";
		}
		if (_model.ChildView != null)
		{
			_currentStyle.SetText(mTGALocalizedString, _model.ChildView);
		}
		else
		{
			_currentStyle.SetText(mTGALocalizedString, _model.ButtonIcon);
		}
		_warningIcon.UpdateActive(_model.ShowWarningIcon);
		Action buttonCallback = _model.ButtonCallback;
		_currentStyle.SetIsEnabled(buttonCallback != null && _model.Enabled);
		if (buttonCallback != null)
		{
			SetActive(active: true);
		}
	}

	public void SetModel(PromptButtonData buttonModel)
	{
		_model = buttonModel;
		_isDirty = true;
	}

	public void SetActive(bool active)
	{
		if (_isActive != active)
		{
			_isActive = active;
			_animator.SetBool("Active", _isActive);
		}
	}

	public void ResetButton()
	{
		SetModel(new PromptButtonData
		{
			ButtonText = _model.ButtonText,
			Style = _model.Style,
			Enabled = _model.Enabled
		});
		SetActive(active: false);
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (_model.Enabled && eventData.button == PointerEventData.InputButton.Left)
		{
			_currentStyle?.Execute(AnimationTrigger.TriggerType.OnPointerClick);
			base.OnPointerClick(eventData);
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (_model.Enabled)
		{
			_model.PointerEnter?.Invoke();
			_currentStyle?.Execute(AnimationTrigger.TriggerType.OnPointerEnter);
			Hover = true;
			Rollover?.Invoke();
			base.OnPointerEnter(eventData);
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		if (_model.Enabled)
		{
			_model.PointerExit?.Invoke();
			_currentStyle?.Execute(AnimationTrigger.TriggerType.OnPointerExit);
			Hover = false;
			Rollout?.Invoke();
			base.OnPointerExit(eventData);
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		if (_model.Enabled && eventData.button == PointerEventData.InputButton.Left)
		{
			_currentStyle?.Execute(AnimationTrigger.TriggerType.OnPointerDown);
			base.OnPointerDown(eventData);
		}
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		if (_model.Enabled && eventData.button == PointerEventData.InputButton.Left)
		{
			_currentStyle?.Execute(AnimationTrigger.TriggerType.OnPointerUp);
			base.OnPointerUp(eventData);
		}
	}

	public override void OnSubmit(BaseEventData eventData)
	{
	}

	public void SimulateClickDown()
	{
		if (!(_currentStyle == null) && _model.Enabled)
		{
			_currentStyle.Execute(AnimationTrigger.TriggerType.OnPointerEnter);
			_currentStyle.Execute(AnimationTrigger.TriggerType.OnPointerDown);
		}
	}

	public void SimulateClickRelease()
	{
		if (!(_currentStyle == null) && _model.Enabled)
		{
			_currentStyle.Execute(AnimationTrigger.TriggerType.OnPointerClick);
			base.onClick?.Invoke();
		}
	}
}
