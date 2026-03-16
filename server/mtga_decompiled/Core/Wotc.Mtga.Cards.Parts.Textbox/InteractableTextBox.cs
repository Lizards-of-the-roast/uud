using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.Cards.Parts.Textbox;

public class InteractableTextBox : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler
{
	public enum TextBoxHighlight
	{
		Default,
		Dimmed,
		Selectable,
		Selected
	}

	[SerializeField]
	private Collider _collider;

	[SerializeField]
	private TMP_Text _titleLabel;

	[SerializeField]
	private TMP_Text _abilityLabel;

	[SerializeField]
	private GameObject _darkenedMask;

	[SerializeField]
	private GameObject _selectableMask;

	[SerializeField]
	private GameObject _selectedMask;

	[SerializeField]
	private GameObject _hoveredMask;

	private Action _pressed;

	public void SetFont(TMP_FontAsset titleFont, TMP_FontAsset abilityFont)
	{
		_titleLabel.font = titleFont;
		_titleLabel.fontStyle = FontStyles.Bold;
		_abilityLabel.font = abilityFont;
	}

	public void SetLabels(string title, string ability)
	{
		_titleLabel.text = title;
		_abilityLabel.text = ability;
	}

	public void FitTextInTextbox(IReadOnlyList<float> supportedFontSizes)
	{
		_titleLabel.enableAutoSizing = false;
		_abilityLabel.enableAutoSizing = false;
		float num = Mathf.Abs(_titleLabel.rectTransform.rect.y);
		foreach (float supportedFontSize in supportedFontSizes)
		{
			_titleLabel.fontSize = supportedFontSize;
			_titleLabel.ForceMeshUpdate();
			if (_titleLabel.preferredHeight <= num)
			{
				break;
			}
		}
		num = Mathf.Abs(_abilityLabel.rectTransform.rect.y);
		foreach (float supportedFontSize2 in supportedFontSizes)
		{
			_abilityLabel.fontSize = supportedFontSize2;
			_abilityLabel.ForceMeshUpdate();
			if (_abilityLabel.preferredHeight <= num)
			{
				break;
			}
		}
		_titleLabel.SetAllDirty();
		_abilityLabel.SetAllDirty();
	}

	public void SetInteraction(TextBoxHighlight highlight, Action onPressed)
	{
		_pressed = onPressed;
		_collider.enabled = onPressed != null;
		_darkenedMask.UpdateActive(highlight == TextBoxHighlight.Dimmed);
		_selectableMask.UpdateActive(highlight == TextBoxHighlight.Selectable);
		_selectedMask.UpdateActive(highlight == TextBoxHighlight.Selected);
		_hoveredMask.UpdateActive(active: false);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_hoveredMask.UpdateActive(active: true);
		AudioManager.PlayAudio("sfx_ui_main_rollover", base.gameObject);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_hoveredMask.UpdateActive(active: false);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		_pressed?.Invoke();
		AudioManager.PlayAudio("sfx_UI_generic_click", base.gameObject);
	}
}
