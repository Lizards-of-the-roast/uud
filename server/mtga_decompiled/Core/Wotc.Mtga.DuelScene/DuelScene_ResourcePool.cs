using System.Collections.Generic;
using Pooling;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.DuelScene;

public abstract class DuelScene_ResourcePool : MonoBehaviour
{
	[SerializeField]
	protected RectTransform _rect;

	[SerializeField]
	protected int _maxDisplayedIcons = 4;

	protected int _maxDisplayedIconsCache;

	[SerializeField]
	protected float _elementSpacing = 0.15f;

	protected float _elementWidth;

	[SerializeField]
	protected RectTransform _contentParent;

	[SerializeField]
	protected ManaPoolButton _buttonPrefab;

	[SerializeField]
	protected ManaPoolSpriteTable _spriteTable;

	protected ITooltipDisplay _tooltipDisplay;

	protected IPromptEngine _promptEngine;

	protected IUnityObjectPool _objectPool;

	protected List<ManaPoolButton> _buttons = new List<ManaPoolButton>();

	protected bool _isDirty;

	public RectTransform Rect => _rect;

	public void Init(ITooltipDisplay tooltipDisplay, IPromptEngine promptEngine, IUnityObjectPool objectPool)
	{
		_tooltipDisplay = tooltipDisplay;
		_promptEngine = promptEngine;
		_objectPool = objectPool;
	}

	protected virtual void Awake()
	{
		_contentParent.GetComponent<HorizontalOrVerticalLayoutGroup>().spacing = _elementSpacing;
		_elementWidth = _buttonPrefab.GetComponent<LayoutElement>().preferredWidth;
		_maxDisplayedIconsCache = _maxDisplayedIcons;
		_isDirty = true;
	}

	protected virtual void OnDestroy()
	{
	}

	protected virtual void LateUpdate()
	{
		if (_isDirty)
		{
			_isDirty = false;
			Layout();
		}
	}

	protected abstract void Layout();
}
