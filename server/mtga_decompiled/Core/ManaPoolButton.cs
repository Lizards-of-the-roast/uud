using System;
using System.Collections.Generic;
using GreClient.CardData;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

public class ManaPoolButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private Button _button;

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private TextMeshProUGUI _resourceCountText;

	[SerializeField]
	private GameObject _manaRiderObj;

	[SerializeField]
	private GameObject _manaSnowObj;

	[SerializeField]
	private GameObject _manaTreasureObj;

	private TooltipData _tooltipData;

	private TooltipProperties _tooltipProperties;

	private int _count;

	private List<RiderPromptData> _riders;

	private List<ManaSpecType> _specs;

	private ITooltipDisplay _tooltipDisplay = new NullTooltipDisplay();

	private IPromptEngine _promptEngine;

	public event Action<ManaPoolButton> Clicked;

	private void Awake()
	{
		_button.onClick.AddListener(delegate
		{
			this.Clicked?.Invoke(this);
		});
		_tooltipData = new TooltipData
		{
			RelativePosition = TooltipSystem.TooltipPositionAnchor.TopRight,
			TooltipStyle = TooltipSystem.TooltipStyle.DefaultLeftAlignedText
		};
		_tooltipProperties = new TooltipProperties
		{
			Padding = new Vector2(80f, 10f)
		};
	}

	public void Init(ITooltipDisplay tooltipDisplay, IPromptEngine promptEngine)
	{
		_tooltipDisplay = tooltipDisplay ?? new NullTooltipDisplay();
		_promptEngine = promptEngine;
	}

	public void OnDestroy()
	{
		this.Clicked = null;
		_button.onClick.RemoveAllListeners();
	}

	public void SetVisuals(Sprite iconSprite, Sprite highlightSprite, List<RiderPromptData> riders, List<ManaSpecType> manaSpecs, int count)
	{
		_count = count;
		_icon.sprite = iconSprite;
		_button.spriteState = new SpriteState
		{
			highlightedSprite = highlightSprite
		};
		_riders = riders;
		_specs = manaSpecs;
		_resourceCountText.text = _count.ToString();
		ManaIcons.IconType iconType = ManaIcons.CalculateIconType(riders, manaSpecs);
		_manaRiderObj.UpdateActive(iconType == ManaIcons.IconType.Riders);
		_manaSnowObj.UpdateActive(iconType == ManaIcons.IconType.Snow);
		_manaTreasureObj.UpdateActive(iconType == ManaIcons.IconType.Treasure);
	}

	public void SetVisuals(Sprite iconSprite, Sprite highlightSprite, int count = 1)
	{
		_count = count;
		_icon.sprite = iconSprite;
		_button.spriteState = new SpriteState
		{
			highlightedSprite = highlightSprite
		};
		_riders = null;
		_manaRiderObj.UpdateActive(active: false);
		_manaSnowObj.UpdateActive(active: false);
		_manaTreasureObj.UpdateActive(active: false);
		_resourceCountText.text = _count.ToString();
	}

	public void AddCount(int value)
	{
		_count += value;
		_resourceCountText.text = _count.ToString();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (_promptEngine != null)
		{
			string text = TooltipTextFromManaRiders(_promptEngine, _riders);
			string text2 = TooltipTextFromManaSpecs(_specs);
			if (!string.IsNullOrEmpty(text) || !string.IsNullOrEmpty(text2))
			{
				string text3 = text + ((!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text2)) ? "\n" : string.Empty) + text2;
				_tooltipData.Text = text3;
				_tooltipDisplay.DisplayTooltip(base.gameObject, _tooltipData, _tooltipProperties);
			}
		}
	}

	private string TooltipTextFromManaRiders(IPromptEngine promptEngine, List<RiderPromptData> riders)
	{
		if (riders == null || riders.Count == 0)
		{
			return string.Empty;
		}
		string text = string.Empty;
		foreach (RiderPromptData rider in riders)
		{
			string promptText = promptEngine.GetPromptText(rider);
			promptText = ManaUtilities.ConvertManaSymbols(promptText);
			text = text + ((text.Length > 0) ? "\n" : string.Empty) + promptText;
		}
		return text;
	}

	private string TooltipTextFromManaSpecs(List<ManaSpecType> specs)
	{
		if (specs == null || specs.Count == 0)
		{
			return string.Empty;
		}
		string text = string.Empty;
		using List<ManaSpecType>.Enumerator enumerator = specs.GetEnumerator();
		while (enumerator.MoveNext())
		{
			switch (enumerator.Current)
			{
			case ManaSpecType.FromSnow:
				text += Languages.ActiveLocProvider.GetLocalizedText("DuelScene/ScreenSpace/ManaPool/FromSnowSourceTooltip");
				break;
			case ManaSpecType.FromTreasure:
				text += Languages.ActiveLocProvider.GetLocalizedText("DuelScene/ScreenSpace/ManaPool/FromTreasureSourceTooltip");
				break;
			}
		}
		return text;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_tooltipData.Text = string.Empty;
	}
}
