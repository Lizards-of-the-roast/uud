using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

[RequireComponent(typeof(KeyboardToggleButton))]
public class FullControl : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private TooltipData _tooltipData;

	[SerializeField]
	private TooltipProperties _tooltipProperties;

	protected KeyboardToggleButton _keyboardToggleButton;

	protected UnlocalizedMTGAString _unlocalizedLabelString = new UnlocalizedMTGAString();

	protected UnlocalizedMTGAString _unlocalizedKeyString = new UnlocalizedMTGAString();

	protected bool _isHovered;

	public bool forceLocked;

	private TooltipSystem _tooltipSystem;

	public event System.Action Clicked;

	public void Init(TooltipSystem tooltipSystem)
	{
		_tooltipSystem = tooltipSystem;
	}

	protected void Awake()
	{
		_keyboardToggleButton = GetComponent<KeyboardToggleButton>();
	}

	protected virtual void Start()
	{
		_unlocalizedLabelString.Key = Languages.ActiveLocProvider.GetLocalizedText("DuelScene/SettingsMenu/Gameplay/FullControlTemp");
		_keyboardToggleButton.SetLabelText(_unlocalizedLabelString);
	}

	private void OnDestroy()
	{
		this.Clicked = null;
	}

	public void ToggleAutoPass()
	{
		_isHovered = true;
		this.Clicked?.Invoke();
	}

	public void OnSettingsUpdated(SettingsMessage settingsMessage)
	{
		UpdateVisuals(settingsMessage.FullControlEnabled(), settingsMessage.FullControlLocked());
	}

	protected virtual void UpdateVisuals(bool fullControlEnabled, bool locked)
	{
		_unlocalizedLabelString.Key = (locked ? Languages.ActiveLocProvider.GetLocalizedText("DuelScene/SettingsMenu/Gameplay/FullControlLock") : Languages.ActiveLocProvider.GetLocalizedText("DuelScene/SettingsMenu/Gameplay/FullControlTemp"));
		_keyboardToggleButton.SetLabelText(_unlocalizedLabelString);
		_keyboardToggleButton.SetToggled(fullControlEnabled);
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		if (!(CardDragController.DraggedCard != null))
		{
			_isHovered = true;
			_tooltipData.Text = "DuelScene/ScreenSpace/Prompts/FullControlToolTip";
			_tooltipSystem.DisplayTooltip(base.gameObject, _tooltipData, _tooltipProperties);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_isHovered = false;
	}
}
