using System;
using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class ManaPool : DuelScene_ResourcePool
{
	public class MtgManaComparer : IComparer<MtgMana>
	{
		public Dictionary<uint, HighlightType> Highlights;

		public int Compare(MtgMana lhs, MtgMana rhs)
		{
			uint manaId = lhs.ManaId;
			uint manaId2 = rhs.ManaId;
			if (Highlights != null)
			{
				bool flag = Highlights.ContainsKey(manaId);
				bool flag2 = Highlights.ContainsKey(manaId2);
				if (flag != flag2)
				{
					return flag2.CompareTo(flag);
				}
			}
			ManaColor color = lhs.Color;
			ManaColor color2 = rhs.Color;
			if (color != color2)
			{
				return color.CompareTo(color2);
			}
			int count = lhs.Riders.Count;
			int count2 = rhs.Riders.Count;
			if (count != count2)
			{
				return count2.CompareTo(count);
			}
			for (int i = 0; i < count; i++)
			{
				int num = lhs.Riders[i].PromptId - rhs.Riders[i].PromptId;
				if (num != 0)
				{
					return num;
				}
			}
			int count3 = lhs.ManaSpecs.Count;
			int count4 = rhs.ManaSpecs.Count;
			if (count3 != count4)
			{
				return count4.CompareTo(count3);
			}
			for (int j = 0; j < count3; j++)
			{
				int num2 = lhs.ManaSpecs[j] - rhs.ManaSpecs[j];
				if (num2 != 0)
				{
					return num2;
				}
			}
			return manaId.CompareTo(manaId2);
		}
	}

	[SerializeField]
	private Image _poolBackgroundImage;

	[SerializeField]
	private Button _pageLeftButton;

	[SerializeField]
	private Button _pageRightButton;

	private List<MtgMana> _manaPool = new List<MtgMana>();

	private Dictionary<uint, ManaPoolButton> _idToButtons = new Dictionary<uint, ManaPoolButton>();

	private Dictionary<uint, HighlightType> _highlights = new Dictionary<uint, HighlightType>();

	private Dictionary<uint, HighlightType> _prvHighlights = new Dictionary<uint, HighlightType>();

	private int _pageIndex;

	private static MtgManaComparer manaPoolComparer = new MtgManaComparer();

	public List<MtgMana> Mana
	{
		set
		{
			_pageIndex = 0;
			_manaPool = value;
			_isDirty = true;
		}
	}

	public Dictionary<uint, HighlightType> Highlights
	{
		set
		{
			_pageIndex = 0;
			_highlights.Clear();
			foreach (uint key in value.Keys)
			{
				if (_idToButtons.ContainsKey(key))
				{
					_highlights[key] = value[key];
				}
			}
			_isDirty |= HighlightsAreDirty(_prvHighlights, _highlights);
			_prvHighlights.Clear();
			foreach (uint key2 in _highlights.Keys)
			{
				_prvHighlights[key2] = _highlights[key2];
			}
		}
	}

	public event Action<uint> ManaSelected;

	protected override void Awake()
	{
		base.Awake();
		_pageLeftButton.onClick.AddListener(OnPageLeft);
		_pageRightButton.onClick.AddListener(OnPageRight);
	}

	protected override void OnDestroy()
	{
		if ((bool)_pageLeftButton)
		{
			_pageLeftButton.onClick.RemoveListener(OnPageLeft);
		}
		if ((bool)_pageRightButton)
		{
			_pageRightButton.onClick.RemoveListener(OnPageRight);
		}
		if (_objectPool != null)
		{
			foreach (ManaPoolButton value in _idToButtons.Values)
			{
				if ((bool)value)
				{
					_objectPool.PushObject(value.gameObject);
				}
			}
			_idToButtons.Clear();
		}
		this.ManaSelected = null;
		base.OnDestroy();
	}

	protected virtual void OnPageLeft()
	{
		_pageIndex--;
		_isDirty = true;
	}

	protected virtual void OnPageRight()
	{
		_pageIndex++;
		_isDirty = true;
	}

	private bool HighlightsAreDirty(Dictionary<uint, HighlightType> previous, Dictionary<uint, HighlightType> current)
	{
		foreach (uint key in previous.Keys)
		{
			if (!current.ContainsKey(key))
			{
				return true;
			}
		}
		foreach (uint key2 in current.Keys)
		{
			if (!previous.ContainsKey(key2) || (previous.TryGetValue(key2, out var value) && current[key2] != value))
			{
				return true;
			}
		}
		return false;
	}

	public Transform GetResourceTransform(MtgMana mana)
	{
		if (!_idToButtons.ContainsKey(mana.ManaId))
		{
			_isDirty = false;
			Layout();
		}
		if (_idToButtons.TryGetValue(mana.ManaId, out var value))
		{
			return value.transform;
		}
		return null;
	}

	protected override void Layout()
	{
		_idToButtons.Clear();
		manaPoolComparer.Highlights = _highlights;
		_manaPool.Sort(manaPoolComparer);
		int num = 0;
		for (int i = 0; i < _manaPool.Count; i++)
		{
			MtgMana mtgMana = _manaPool[i];
			if (i > 0 && collapseIntoPrvMana(mtgMana, _manaPool[i - 1], _highlights))
			{
				ManaPoolButton manaPoolButton = _buttons[num - 1];
				_idToButtons[mtgMana.ManaId] = manaPoolButton;
				manaPoolButton.AddCount((int)mtgMana.Count);
				continue;
			}
			ManaPoolButton orCreateManaButton = GetOrCreateManaButton(num);
			_idToButtons[mtgMana.ManaId] = orCreateManaButton;
			num++;
			ManaPoolSpriteTable.Sprites spritesForColor = _spriteTable.GetSpritesForColor(mtgMana.Color);
			HighlightType highlightType = ManaHighlightType(mtgMana.ManaId);
			orCreateManaButton.SetVisuals(spritesForColor.Default(highlightType), spritesForColor.Hover(highlightType), mtgMana.Riders, mtgMana.ManaSpecs, (int)mtgMana.Count);
		}
		while (num < _buttons.Count)
		{
			ManaPoolButton manaPoolButton2 = _buttons[num];
			manaPoolButton2.Clicked -= OnButtonClicked;
			_buttons.RemoveAt(num);
			UnityEngine.Object.Destroy(manaPoolButton2.gameObject);
		}
		while (_pageIndex + _maxDisplayedIcons > _buttons.Count && _pageIndex != 0)
		{
			_pageIndex--;
		}
		bool flag = _highlights.Count > 0;
		int num2 = 0;
		_maxDisplayedIcons = (flag ? _buttons.Count : _maxDisplayedIconsCache);
		for (int j = 0; j < _buttons.Count; j++)
		{
			bool flag2 = j >= _pageIndex && j < _pageIndex + _maxDisplayedIcons;
			if (flag2)
			{
				num2++;
			}
			_buttons[j].gameObject.UpdateActive(flag2);
		}
		bool active = !flag && _pageIndex > 0;
		_pageLeftButton.gameObject.UpdateActive(active);
		bool active2 = !flag && _pageIndex + _maxDisplayedIcons < _buttons.Count;
		_pageRightButton.gameObject.UpdateActive(active2);
		Vector3 vector = _rect.sizeDelta;
		vector.x = (float)num2 * _elementWidth + (float)(num2 - 1) * _elementSpacing;
		_rect.sizeDelta = vector;
		bool flag3 = _manaPool.Count > 0;
		if (_poolBackgroundImage.enabled != flag3)
		{
			_poolBackgroundImage.enabled = flag3;
		}
		static bool collapseIntoPrvMana(MtgMana curMana, MtgMana prvMana, Dictionary<uint, HighlightType> highlights)
		{
			HighlightType value;
			bool num3 = highlights.TryGetValue(curMana.ManaId, out value);
			HighlightType value2;
			bool flag4 = highlights.TryGetValue(prvMana.ManaId, out value2);
			if (num3 != flag4)
			{
				return false;
			}
			if (value != value2)
			{
				return false;
			}
			if (curMana.Color != prvMana.Color)
			{
				return false;
			}
			if (!curMana.Riders.ContainSame(prvMana.Riders))
			{
				return false;
			}
			if (!curMana.ManaSpecs.ContainSame(prvMana.ManaSpecs))
			{
				return false;
			}
			return true;
		}
	}

	private ManaPoolButton GetOrCreateManaButton(int buttonCount)
	{
		if (buttonCount > _buttons.Count - 1)
		{
			ManaPoolButton manaPoolButton = UnityEngine.Object.Instantiate(_buttonPrefab, _contentParent.transform);
			manaPoolButton.Clicked += OnButtonClicked;
			manaPoolButton.Init(_tooltipDisplay, _promptEngine);
			_buttons.Insert(buttonCount, manaPoolButton);
			return manaPoolButton;
		}
		return _buttons[buttonCount];
	}

	private void OnButtonClicked(ManaPoolButton btn)
	{
		foreach (uint key in _idToButtons.Keys)
		{
			if (_idToButtons[key] == btn)
			{
				this.ManaSelected?.Invoke(key);
				break;
			}
		}
	}

	private HighlightType ManaHighlightType(uint manaId)
	{
		if (_highlights.TryGetValue(manaId, out var value))
		{
			return value;
		}
		return HighlightType.None;
	}
}
