using TMPro;
using UnityEngine;
using Wotc.Mtga.Loc;

namespace EventPage.CampaignGraph;

public class LossHintModule : OverlayModule
{
	[SerializeField]
	private TextMeshProUGUI _lossHintTitle;

	[SerializeField]
	private TextMeshProUGUI _lossHintDescription;

	[SerializeField]
	private Transform _lossHintTooltipParent;

	[SerializeField]
	private CustomButton _doneButton;

	private LossHintDeluxeTooltip _lossHintTooltip;

	private void Awake()
	{
		_doneButton.OnClick.AddListener(DoneButtonClicked);
	}

	public override void Show()
	{
		if (!base.gameObject.activeSelf)
		{
			PostMatchContext postMatchContext = base.EventContext.PostMatchContext;
			if (postMatchContext != null && !postMatchContext.WonGame)
			{
				ShowLossHint();
			}
		}
	}

	public override void UpdateModule()
	{
	}

	public override void Hide()
	{
		if (_lossHintTooltip != null)
		{
			Object.Destroy(_lossHintTooltip.gameObject);
		}
		base.gameObject.SetActive(value: false);
	}

	private void ShowLossHint()
	{
		string lossHintPrefabPath = ClientEventDefinitionList.GetLossHintPrefabPath(_assetLookupSystem, _parentTemplate.EventContext);
		if (!string.IsNullOrEmpty(lossHintPrefabPath))
		{
			_lossHintTooltip = AssetLoader.Instantiate<LossHintDeluxeTooltip>(lossHintPrefabPath, _lossHintTooltipParent);
			_lossHintTooltip.SetCards(_parentTemplate.EventContext, SceneLoader.GetSceneLoader().GetCardZoomView());
			_lossHintTitle.text = Languages.ActiveLocProvider.GetLocalizedText(_lossHintTooltip.DefaultLossHintTitle);
			_lossHintDescription.text = Languages.ActiveLocProvider.GetLocalizedText(_lossHintTooltip.DefaultLossHintDescription);
			base.gameObject.SetActive(value: true);
		}
	}

	private void DoneButtonClicked()
	{
		Hide();
	}
}
