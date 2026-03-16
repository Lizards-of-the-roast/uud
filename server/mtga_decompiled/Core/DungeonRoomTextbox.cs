using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Wotc.Mtga.Extensions;

public class DungeonRoomTextbox : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler
{
	public enum RoomHighlight
	{
		Default,
		Dimmed,
		Selectable,
		Selected
	}

	[SerializeField]
	private Collider _collider;

	[SerializeField]
	private TMP_Text _roomTitleLabel;

	[SerializeField]
	private TMP_Text _roomAbilityLabel;

	[SerializeField]
	private GameObject _roomDarkenedMask;

	[SerializeField]
	private GameObject _roomSelectableMask;

	[SerializeField]
	private GameObject _roomSelectedMask;

	[SerializeField]
	private GameObject _roomHoveredMask;

	private Action _roomPressed;

	public void SetFont(TMP_FontAsset titleFont, TMP_FontAsset abilityFont)
	{
		_roomTitleLabel.font = titleFont;
		_roomTitleLabel.fontStyle = FontStyles.Bold;
		_roomAbilityLabel.font = abilityFont;
	}

	public void SetLabels(string title, string ability)
	{
		_roomTitleLabel.text = title;
		_roomAbilityLabel.text = ability;
	}

	public void FitTextInTextbox(IReadOnlyList<float> supportedFontSizes)
	{
		_roomTitleLabel.enableAutoSizing = false;
		_roomAbilityLabel.enableAutoSizing = false;
		float num = Mathf.Abs(_roomTitleLabel.rectTransform.rect.y);
		foreach (float supportedFontSize in supportedFontSizes)
		{
			_roomTitleLabel.fontSize = supportedFontSize;
			_roomTitleLabel.ForceMeshUpdate();
			if (_roomTitleLabel.preferredHeight <= num)
			{
				break;
			}
		}
		num = Mathf.Abs(_roomAbilityLabel.rectTransform.rect.y);
		foreach (float supportedFontSize2 in supportedFontSizes)
		{
			_roomAbilityLabel.fontSize = supportedFontSize2;
			_roomAbilityLabel.ForceMeshUpdate();
			if (_roomAbilityLabel.preferredHeight <= num)
			{
				break;
			}
		}
		_roomTitleLabel.SetAllDirty();
		_roomAbilityLabel.SetAllDirty();
	}

	public void SetInteraction(RoomHighlight roomHighlight, Action roomPressed)
	{
		_roomPressed = roomPressed;
		_collider.enabled = roomPressed != null;
		_roomDarkenedMask.UpdateActive(roomHighlight == RoomHighlight.Dimmed);
		_roomSelectableMask.UpdateActive(roomHighlight == RoomHighlight.Selectable);
		_roomSelectedMask.UpdateActive(roomHighlight == RoomHighlight.Selected);
		_roomHoveredMask.UpdateActive(active: false);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_roomHoveredMask.UpdateActive(active: true);
		AudioManager.PlayAudio("sfx_ui_main_rollover", base.gameObject);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_roomHoveredMask.UpdateActive(active: false);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		_roomPressed?.Invoke();
		AudioManager.PlayAudio("sfx_UI_generic_click", base.gameObject);
	}
}
