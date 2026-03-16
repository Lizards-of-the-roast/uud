using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.Browsers;

public class SelectCardsBrowser_MultiZone : SelectCardsBrowser
{
	private bool _usingZoneButtons;

	private Vector3 _originalScrollbarPosition;

	private Vector3 _centeredScrollbarPosition;

	public SelectCardsBrowser_MultiZone(BrowserManager browserManager, IDuelSceneBrowserProvider provider, IHighlightController highlightController, IDimmingController dimmingController, GameManager gameManager)
		: base(browserManager, provider, highlightController, dimmingController, gameManager)
	{
		base.AllowsHoverInteractions = true;
	}

	public void OnZoneUpdated()
	{
		cardHolder.ApplyTargetOffset = selectCardsProvider.ApplyTargetOffset;
		cardHolder.ApplySourceOffset = selectCardsProvider.ApplySourceOffset;
		ReleaseCards();
		SetupCards();
		UpdateButtons();
		UpdateHighlightsAndDimming();
		scrollbar.gameObject.SetActive(cardViews.Count > scrollableLayout.FrontCount);
		scrollbar.value = 1f;
		cardHolder.OnCardHolderUpdated += base.OnNextCardHolderUpdate;
		cardHolder.LayoutNow();
	}

	protected override void InitializeUIElements()
	{
		base.InitializeUIElements();
		scrollbar.value = 1f;
		_originalScrollbarPosition = scrollbar.transform.localPosition;
		_centeredScrollbarPosition = _originalScrollbarPosition;
		_centeredScrollbarPosition.x = 0f;
		scrollbar.transform.localPosition = (_usingZoneButtons ? _originalScrollbarPosition : _centeredScrollbarPosition);
	}

	protected override void PostCardViewSelection()
	{
		if (!base.IsClosed)
		{
			UpdateButtons();
			UpdateHighlightsAndDimming();
		}
	}

	protected override void SetupCards()
	{
		cardViews = selectCardsProvider.GetCardsToDisplay();
		float scrollPosition = scrollableLayout.ScrollPosition;
		Dictionary<string, ButtonStateData> buttonStateData = selectCardsProvider.GetButtonStateData();
		_usingZoneButtons = buttonStateData.Count((KeyValuePair<string, ButtonStateData> button) => button.Key.StartsWith("ZoneButton")) > 1;
		CardLayout_ScrollableBrowser cardHolderLayoutData = GetCardHolderLayoutData(_usingZoneButtons ? "SelectCards_MultiZone" : "SelectCards_MultiZoneNoButtons", (uint)cardViews.Count);
		scrollableLayout.CopyFrom(cardHolderLayoutData);
		scrollableLayout.ScrollPosition = scrollPosition;
		MoveCardViewsToBrowser(cardViews);
	}

	public override void UpdateButtons()
	{
		if (base.IsClosed)
		{
			return;
		}
		Dictionary<string, ButtonStateData> buttonStateData = selectCardsProvider.GetButtonStateData();
		Dictionary<string, ButtonStateData> dictionary = new Dictionary<string, ButtonStateData>();
		int num = 0;
		foreach (string buttonKey in buttonStateData.Keys)
		{
			if (buttonKey.StartsWith("ZoneButton"))
			{
				GetBrowserElement(buttonStateData[buttonKey].BrowserElementKey).GetComponent<StyledButton>().SetModel(new PromptButtonData
				{
					ButtonText = buttonStateData[buttonKey].LocalizedString,
					Style = buttonStateData[buttonKey].StyleType,
					Enabled = buttonStateData[buttonKey].Enabled,
					ButtonCallback = delegate
					{
						OnButtonCallback(buttonKey);
					}
				});
				num++;
			}
			else
			{
				dictionary.Add(buttonKey, buttonStateData[buttonKey]);
			}
		}
		if (num == 1)
		{
			for (int num2 = 0; num2 < 3; num2++)
			{
				GetBrowserElement("ZoneButton" + num2).GetComponent<StyledButton>().gameObject.SetActive(value: false);
			}
		}
		else if (num < 3)
		{
			GetBrowserElement("ZoneButton2").GetComponent<StyledButton>().gameObject.SetActive(value: false);
		}
		if (dictionary.Count == 1 && num == 0 && buttonStateData.TryGetValue("DoneButton", out var value) && value.StyleType == ButtonStyle.StyleType.Secondary)
		{
			GameObject browserElement = GetBrowserElement("ButtonMarker_BottomRight1");
			GetBrowserElement("SingleButton").GetComponent<StyledButton>().transform.position = browserElement.transform.position;
		}
		UpdateButtons(dictionary);
	}
}
