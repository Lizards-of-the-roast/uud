using System;
using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

[Serializable]
public class PromptButtonData
{
	public MTGALocalizedString ButtonText;

	public System.Action ButtonCallback;

	public System.Action PointerEnter;

	public System.Action PointerExit;

	public string ButtonSFX = WwiseEvents.sfx_ui_accept.EventName;

	public bool ClearsInteractions = true;

	public ButtonTag Tag = ButtonTag.Secondary;

	public ButtonStyle.StyleType Style;

	public bool Enabled = true;

	public Sprite ButtonIcon;

	public RectTransform ChildView;

	public bool ShowWarningIcon;

	public TooltipData TooltipData;

	[NonSerialized]
	public Phase NextPhase;

	[NonSerialized]
	public Step NextStep;
}
