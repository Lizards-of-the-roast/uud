using System.Collections.Generic;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Assets.Core.DuelScene;

public class DuelScene_CounterPool : DuelScene_ResourcePool
{
	private Dictionary<CounterType, int> _counters = new Dictionary<CounterType, int>();

	private Dictionary<CounterType, ManaPoolButton> _counterTypeToButton = new Dictionary<CounterType, ManaPoolButton>();

	private List<CounterType> _highlights = new List<CounterType>();

	private List<CounterType> _prvHighlights = new List<CounterType>();

	private List<CounterType> _sortedKeys = new List<CounterType>();

	public Dictionary<CounterType, int> Counters
	{
		get
		{
			return _counters;
		}
		set
		{
			_counters = value;
			_isDirty = true;
		}
	}

	public CounterType[] Highlights
	{
		set
		{
			_prvHighlights.Clear();
			_prvHighlights.AddRange(_highlights);
			_highlights.Clear();
			int num = 0;
			while (value != null && num < value.Length)
			{
				_highlights.Add(value[num]);
				num++;
			}
			_isDirty |= _prvHighlights.ContainSame(_highlights);
		}
	}

	protected override void OnDestroy()
	{
		if (_objectPool != null)
		{
			foreach (ManaPoolButton value in _counterTypeToButton.Values)
			{
				if ((bool)value)
				{
					_objectPool.PushObject(value.gameObject);
				}
			}
			_counterTypeToButton.Clear();
		}
		base.OnDestroy();
	}

	protected override void Layout()
	{
		_counterTypeToButton.Clear();
		while (_buttons.Count > 0)
		{
			int index = _buttons.Count - 1;
			ManaPoolButton manaPoolButton = _buttons[index];
			_buttons.RemoveAt(index);
			_objectPool.PushObject(manaPoolButton.gameObject);
		}
		_sortedKeys.Clear();
		_sortedKeys.AddRange(_counters.Keys);
		_sortedKeys.Sort((CounterType a, CounterType b) => a - b);
		for (int num = 0; num < _sortedKeys.Count; num++)
		{
			CounterType counterType = _sortedKeys[num];
			ManaPoolButton component = _objectPool.PopObject(_buttonPrefab.gameObject, _contentParent.transform).GetComponent<ManaPoolButton>();
			component.Init(_tooltipDisplay, _promptEngine);
			component.transform.ZeroOut();
			_buttons.Add(component);
			_counterTypeToButton[counterType] = component;
			ManaPoolSpriteTable.Sprites spritesForCounter = _spriteTable.GetSpritesForCounter(counterType);
			HighlightType highlightType = (_highlights.Contains(counterType) ? HighlightType.Hot : HighlightType.None);
			component.SetVisuals(spritesForCounter.Default(highlightType), spritesForCounter.Hover(highlightType), _counters[counterType]);
		}
	}
}
