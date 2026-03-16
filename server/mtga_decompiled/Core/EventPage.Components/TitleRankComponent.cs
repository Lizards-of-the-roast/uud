using System;
using AssetLookupTree;
using UnityEngine;
using Wizards.Mtga.Format;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Extensions;

namespace EventPage.Components;

public class TitleRankComponent : TextComponent
{
	[SerializeField]
	private RankDisplay _rankDisplay;

	[SerializeField]
	private CustomButton _backButton;

	public Action<bool> BackButtonAction;

	public void Awake()
	{
		_backButton.OnClick.AddListener(BackButtonClicked);
	}

	public void OnDestroy()
	{
		_backButton.OnClick.RemoveListener(BackButtonClicked);
	}

	private void BackButtonClicked()
	{
		BackButtonAction(obj: true);
	}

	public void ShowBackButton(bool showButton)
	{
		_backButton.transform.parent.gameObject.SetActive(showButton);
	}

	public void ShowRank(MDNEFormatType eventType, AssetLookupSystem assetLookupSystem)
	{
		if (_rankDisplay != null)
		{
			_rankDisplay.gameObject.UpdateActive(active: true);
			_rankDisplay.IsLimited = FormatUtilities.IsLimited(eventType);
			_rankDisplay.RefreshRank(assetLookupSystem);
		}
	}

	public void HideRank()
	{
		if (_rankDisplay != null)
		{
			_rankDisplay.gameObject.UpdateActive(active: false);
		}
	}
}
