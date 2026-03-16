using Assets.Core.Meta.Utilities;
using TMPro;
using UnityEngine;
using Wotc.Mtga.Loc;

public class RewardDisplayQualifierBadge : RewardDisplay
{
	[SerializeField]
	private TextMeshProUGUI globalText;

	[SerializeField]
	private float defaultLineSpacing;

	[SerializeField]
	private float asianLineSpacing;

	public void UnityEvent_OpenEventDetails()
	{
		UrlOpener.OpenURL("https://magic.wizards.com/en/content/esports/mythicchampionshipqualifiers");
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
	}

	public void Awake()
	{
		if (Languages.CurrentLanguage == "ko-KR" || Languages.CurrentLanguage == "ja-JP")
		{
			globalText.lineSpacing = asianLineSpacing;
		}
		else
		{
			globalText.lineSpacing = defaultLineSpacing;
		}
	}
}
