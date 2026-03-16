using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UI;

public class PresetManaWheel : ManaColorSelector
{
	private enum SelectorMode
	{
		None,
		TwoColor,
		FiveColor,
		SixColor
	}

	[Header("Preset UI")]
	[SerializeField]
	private Transform TwoOptionRoot;

	[SerializeField]
	private Transform FiveOptionRoot;

	[SerializeField]
	private Transform SixOptionRoot;

	[SerializeField]
	private float MinimumDistance = 5f;

	[SerializeField]
	private int FiveColorOffset = 36;

	[SerializeField]
	private int SixColorOffset;

	private readonly Dictionary<ManaColor, Button> _twoOptionButtonsLeft = new Dictionary<ManaColor, Button>();

	private readonly Dictionary<ManaColor, Button> _twoOptionButtonsRight = new Dictionary<ManaColor, Button>();

	private readonly Dictionary<ManaColor, Button> _fiveOptionButtons = new Dictionary<ManaColor, Button>();

	private readonly Dictionary<ManaColor, Button> _sixOptionButtons = new Dictionary<ManaColor, Button>();

	private readonly Dictionary<ManaColor, TextMeshProUGUI> _twoOptionLabelsLeft = new Dictionary<ManaColor, TextMeshProUGUI>();

	private readonly Dictionary<ManaColor, TextMeshProUGUI> _twoOptionLabelsRight = new Dictionary<ManaColor, TextMeshProUGUI>();

	private readonly Dictionary<ManaColor, TextMeshProUGUI> _fiveOptionLabels = new Dictionary<ManaColor, TextMeshProUGUI>();

	private readonly Dictionary<ManaColor, TextMeshProUGUI> _sixOptionLabels = new Dictionary<ManaColor, TextMeshProUGUI>();

	private const string MANA_COLOR = "ManaColor_{0}";

	private const string NUMBER_FORMAT = "x{0}";

	private SelectorMode _mode;

	private bool _readyToClick;

	protected override void Start()
	{
		base.Start();
		for (int i = 0; i < TwoOptionRoot.childCount; i++)
		{
			Transform child = TwoOptionRoot.GetChild(i);
			Button component = child.GetComponent<Button>();
			TextMeshProUGUI componentInChildren = child.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
			string[] array = child.name.Split('_');
			ManaColor key = (ManaColor)Enum.Parse(typeof(ManaColor), $"ManaColor_{array[1]}", ignoreCase: true);
			if (array[2] == "Left")
			{
				_twoOptionButtonsLeft.Add(key, component);
				_twoOptionLabelsLeft.Add(key, componentInChildren);
			}
			else
			{
				_twoOptionButtonsRight.Add(key, component);
				_twoOptionLabelsRight.Add(key, componentInChildren);
			}
		}
		for (int j = 0; j < FiveOptionRoot.childCount; j++)
		{
			Transform child2 = FiveOptionRoot.GetChild(j);
			Button component2 = child2.GetComponent<Button>();
			TextMeshProUGUI componentInChildren2 = child2.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
			string[] array2 = child2.name.Split('_');
			ManaColor key2 = (ManaColor)Enum.Parse(typeof(ManaColor), $"ManaColor_{array2[1]}", ignoreCase: true);
			_fiveOptionButtons.Add(key2, component2);
			_fiveOptionLabels.Add(key2, componentInChildren2);
		}
		for (int k = 0; k < SixOptionRoot.childCount; k++)
		{
			Transform child3 = SixOptionRoot.GetChild(k);
			Button component3 = child3.GetComponent<Button>();
			TextMeshProUGUI componentInChildren3 = child3.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
			string[] array3 = child3.name.Split('_');
			ManaColor key3 = (ManaColor)Enum.Parse(typeof(ManaColor), $"ManaColor_{array3[1]}", ignoreCase: true);
			_sixOptionButtons.Add(key3, component3);
			_sixOptionLabels.Add(key3, componentInChildren3);
		}
	}

	protected override void InWheelUpdate()
	{
		base.InWheelUpdate();
		if (!_readyToClick)
		{
			if (!UnityEngine.Input.GetMouseButton(0))
			{
				_readyToClick = true;
			}
			return;
		}
		Vector2 screenPoint = UnityEngine.Input.mousePosition;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, screenPoint, _camera, out var localPoint);
		Vector2 vector = localPoint - (Vector2)_overlayRect.localPosition;
		float num = Mathf.Atan2(vector.y, vector.x) * 57.29578f - 90f;
		if (num < 0f)
		{
			num += 360f;
		}
		ManaColor manaColor = AngleToManaColor(Mathf.RoundToInt(360f - num));
		bool flag = vector.magnitude < MinimumDistance;
		Debug.DrawLine(base.transform.localPosition, localPoint, flag ? UnityEngine.Color.grey : ManaColorToColor(manaColor), 0.1f);
		if (!flag)
		{
			if (UnityEngine.Input.GetMouseButton(0))
			{
				HighlightColorClicked(manaColor);
				OnClicked();
			}
			else if (UnityEngine.Input.GetMouseButtonUp(0))
			{
				SelectColor(manaColor);
			}
			else
			{
				HighlightColorHovered(manaColor);
				OnHover(manaColor);
			}
		}
	}

	protected override void OutOfWheelUpdate()
	{
		base.OutOfWheelUpdate();
		RemoveHighlights();
	}

	protected override void Cleanup()
	{
		base.Cleanup();
		_mode = SelectorMode.None;
		_readyToClick = false;
	}

	protected override void Setup()
	{
		base.Setup();
		RemoveHighlights();
		if (_selectionProvider != null && _selectionProvider.ValidSelectionCount > 0)
		{
			if (_selectionProvider.ValidSelectionCount > 2)
			{
				if (_selectionProvider.ContainsColor(ManaColor.Colorless))
				{
					_mode = SelectorMode.SixColor;
				}
				else
				{
					_mode = SelectorMode.FiveColor;
				}
			}
			else
			{
				_mode = SelectorMode.TwoColor;
			}
			UpdateLabels();
		}
		else
		{
			_mode = SelectorMode.None;
		}
		TwoOptionRoot.gameObject.SetActive(_mode == SelectorMode.TwoColor);
		FiveOptionRoot.gameObject.SetActive(_mode == SelectorMode.FiveColor);
		SixOptionRoot.gameObject.SetActive(_mode == SelectorMode.SixColor);
		switch (_mode)
		{
		case SelectorMode.TwoColor:
		{
			ManaColor primaryColor = _selectionProvider.GetElementAt(0).PrimaryColor;
			ManaColor primaryColor2 = _selectionProvider.GetElementAt(1).PrimaryColor;
			foreach (KeyValuePair<ManaColor, Button> item in _twoOptionButtonsLeft)
			{
				item.Value.interactable = item.Key == primaryColor;
				item.Value.gameObject.UpdateActive(item.Key == primaryColor);
			}
			{
				foreach (KeyValuePair<ManaColor, Button> item2 in _twoOptionButtonsRight)
				{
					item2.Value.interactable = item2.Key == primaryColor2;
					item2.Value.gameObject.UpdateActive(item2.Key == primaryColor2);
				}
				break;
			}
		}
		case SelectorMode.FiveColor:
		{
			foreach (KeyValuePair<ManaColor, Button> fiveOptionButton in _fiveOptionButtons)
			{
				fiveOptionButton.Value.interactable = _selectionProvider.ContainsColor(fiveOptionButton.Key);
			}
			break;
		}
		case SelectorMode.SixColor:
		{
			foreach (KeyValuePair<ManaColor, Button> sixOptionButton in _sixOptionButtons)
			{
				sixOptionButton.Value.interactable = _selectionProvider.ContainsColor(sixOptionButton.Key);
			}
			break;
		}
		}
	}

	private void UpdateLabels()
	{
		Dictionary<ManaColor, string> dictionary = new Dictionary<ManaColor, string>();
		if (_selectionProvider.SelectedColors.Count() > 0)
		{
			Dictionary<ManaColor, uint> dictionary2 = new Dictionary<ManaColor, uint>();
			foreach (ManaColor selectedColor in _selectionProvider.SelectedColors)
			{
				if (!dictionary2.ContainsKey(selectedColor))
				{
					dictionary2[selectedColor] = 0u;
				}
				dictionary2[selectedColor]++;
			}
			foreach (KeyValuePair<ManaColor, uint> item in dictionary2)
			{
				dictionary[item.Key] = $"x{item.Value}";
			}
		}
		else
		{
			foreach (ManaProducedData validSelection in _selectionProvider.ValidSelections)
			{
				if (!_selectionProvider.CurrentConstantCount.HasValue)
				{
					dictionary[validSelection.PrimaryColor] = $"x{validSelection.CountOfColor}";
					dictionary[validSelection.PrimaryColor] += "+";
				}
			}
		}
		switch (_mode)
		{
		case SelectorMode.TwoColor:
			foreach (KeyValuePair<ManaColor, TextMeshProUGUI> item2 in _twoOptionLabelsLeft)
			{
				if (_twoOptionButtonsLeft[item2.Key].interactable && dictionary.TryGetValue(item2.Key, out var value3))
				{
					item2.Value.alpha = 1f;
					item2.Value.SetText(value3);
				}
				else
				{
					item2.Value.alpha = 0f;
				}
			}
			{
				foreach (KeyValuePair<ManaColor, TextMeshProUGUI> item3 in _twoOptionLabelsRight)
				{
					if (_twoOptionButtonsRight[item3.Key].interactable && dictionary.TryGetValue(item3.Key, out var value4))
					{
						item3.Value.alpha = 1f;
						item3.Value.SetText(value4);
					}
					else
					{
						item3.Value.alpha = 0f;
					}
				}
				break;
			}
		case SelectorMode.FiveColor:
		{
			foreach (KeyValuePair<ManaColor, TextMeshProUGUI> fiveOptionLabel in _fiveOptionLabels)
			{
				if (_fiveOptionButtons[fiveOptionLabel.Key].interactable && dictionary.TryGetValue(fiveOptionLabel.Key, out var value2))
				{
					fiveOptionLabel.Value.alpha = 1f;
					fiveOptionLabel.Value.SetText(value2);
				}
				else
				{
					fiveOptionLabel.Value.alpha = 0f;
				}
			}
			break;
		}
		case SelectorMode.SixColor:
		{
			foreach (KeyValuePair<ManaColor, TextMeshProUGUI> sixOptionLabel in _sixOptionLabels)
			{
				if (_sixOptionButtons[sixOptionLabel.Key].interactable && dictionary.TryGetValue(sixOptionLabel.Key, out var value))
				{
					sixOptionLabel.Value.alpha = 1f;
					sixOptionLabel.Value.SetText(value);
				}
				else
				{
					sixOptionLabel.Value.alpha = 0f;
				}
			}
			break;
		}
		}
	}

	private void RemoveHighlights()
	{
		switch (_mode)
		{
		case SelectorMode.TwoColor:
			foreach (Button value in _twoOptionButtonsLeft.Values)
			{
				value.image.overrideSprite = (value.interactable ? null : value.spriteState.disabledSprite);
			}
			{
				foreach (Button value2 in _twoOptionButtonsRight.Values)
				{
					value2.image.overrideSprite = (value2.interactable ? null : value2.spriteState.disabledSprite);
				}
				break;
			}
		case SelectorMode.FiveColor:
		{
			foreach (Button value3 in _fiveOptionButtons.Values)
			{
				value3.image.overrideSprite = (value3.interactable ? null : value3.spriteState.disabledSprite);
			}
			break;
		}
		case SelectorMode.SixColor:
		{
			foreach (Button value4 in _sixOptionButtons.Values)
			{
				value4.image.overrideSprite = (value4.interactable ? null : value4.spriteState.disabledSprite);
			}
			break;
		}
		}
	}

	private void HighlightColorHovered(ManaColor color)
	{
		switch (_mode)
		{
		case SelectorMode.TwoColor:
			foreach (KeyValuePair<ManaColor, Button> item in _twoOptionButtonsLeft)
			{
				item.Value.image.overrideSprite = (item.Value.interactable ? ((item.Key == color) ? item.Value.spriteState.highlightedSprite : null) : item.Value.spriteState.disabledSprite);
			}
			{
				foreach (KeyValuePair<ManaColor, Button> item2 in _twoOptionButtonsRight)
				{
					item2.Value.image.overrideSprite = (item2.Value.interactable ? ((item2.Key == color) ? item2.Value.spriteState.highlightedSprite : null) : item2.Value.spriteState.disabledSprite);
				}
				break;
			}
		case SelectorMode.FiveColor:
		{
			foreach (KeyValuePair<ManaColor, Button> fiveOptionButton in _fiveOptionButtons)
			{
				fiveOptionButton.Value.image.overrideSprite = (fiveOptionButton.Value.interactable ? ((fiveOptionButton.Key == color) ? fiveOptionButton.Value.spriteState.highlightedSprite : null) : fiveOptionButton.Value.spriteState.disabledSprite);
			}
			break;
		}
		case SelectorMode.SixColor:
		{
			foreach (KeyValuePair<ManaColor, Button> sixOptionButton in _sixOptionButtons)
			{
				sixOptionButton.Value.image.overrideSprite = (sixOptionButton.Value.interactable ? ((sixOptionButton.Key == color) ? sixOptionButton.Value.spriteState.highlightedSprite : null) : sixOptionButton.Value.spriteState.disabledSprite);
			}
			break;
		}
		}
	}

	private void HighlightColorClicked(ManaColor color)
	{
		switch (_mode)
		{
		case SelectorMode.TwoColor:
			foreach (KeyValuePair<ManaColor, Button> item in _twoOptionButtonsLeft)
			{
				item.Value.image.overrideSprite = (item.Value.interactable ? ((item.Key == color) ? item.Value.spriteState.pressedSprite : null) : item.Value.spriteState.disabledSprite);
			}
			{
				foreach (KeyValuePair<ManaColor, Button> item2 in _twoOptionButtonsRight)
				{
					item2.Value.image.overrideSprite = (item2.Value.interactable ? ((item2.Key == color) ? item2.Value.spriteState.pressedSprite : null) : item2.Value.spriteState.disabledSprite);
				}
				break;
			}
		case SelectorMode.FiveColor:
		{
			foreach (KeyValuePair<ManaColor, Button> fiveOptionButton in _fiveOptionButtons)
			{
				fiveOptionButton.Value.image.overrideSprite = (fiveOptionButton.Value.interactable ? ((fiveOptionButton.Key == color) ? fiveOptionButton.Value.spriteState.pressedSprite : null) : fiveOptionButton.Value.spriteState.disabledSprite);
			}
			break;
		}
		case SelectorMode.SixColor:
		{
			foreach (KeyValuePair<ManaColor, Button> sixOptionButton in _sixOptionButtons)
			{
				sixOptionButton.Value.image.overrideSprite = (sixOptionButton.Value.interactable ? ((sixOptionButton.Key == color) ? sixOptionButton.Value.spriteState.pressedSprite : null) : sixOptionButton.Value.spriteState.disabledSprite);
			}
			break;
		}
		}
	}

	private void OnDrawGizmos()
	{
		Vector3 vector = base.transform.up * MinimumDistance;
		for (int i = 0; i < 360; i++)
		{
			Vector3 vector2 = base.transform.position + Quaternion.AngleAxis(i, -base.transform.forward) * vector;
			Vector3 to = base.transform.position + Quaternion.AngleAxis(i + 1, -base.transform.forward) * vector;
			Gizmos.color = ManaColorToColor(AngleToManaColor(i));
			Gizmos.DrawLine(vector2, to);
		}
	}

	private UnityEngine.Color ManaColorToColor(ManaColor col)
	{
		return col switch
		{
			ManaColor.White => UnityEngine.Color.white, 
			ManaColor.Blue => UnityEngine.Color.blue, 
			ManaColor.Black => UnityEngine.Color.black, 
			ManaColor.Red => UnityEngine.Color.red, 
			ManaColor.Green => UnityEngine.Color.green, 
			_ => UnityEngine.Color.grey, 
		};
	}

	private ManaColor AngleToManaColor(int angle)
	{
		switch (_mode)
		{
		case SelectorMode.TwoColor:
			return _selectionProvider.GetElementAt((1 + angle / 180) % 2).PrimaryColor;
		case SelectorMode.FiveColor:
			return (ManaColor)(1 + (angle + FiveColorOffset) / 72 % 5);
		case SelectorMode.SixColor:
		{
			ManaColor manaColor = (ManaColor)((1 + (angle + SixColorOffset) / 60) % 6);
			if (manaColor == ManaColor.None)
			{
				manaColor = ManaColor.Colorless;
			}
			return manaColor;
		}
		default:
			return ManaColor.Colorless;
		}
	}
}
