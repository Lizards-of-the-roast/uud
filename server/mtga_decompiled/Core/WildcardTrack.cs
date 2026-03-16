using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class WildcardTrack : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem _uncommonRewardParticles;

	[SerializeField]
	private ParticleSystem _rareRewardParticles;

	[SerializeField]
	private Sprite _mythicRareIcon;

	[SerializeField]
	private Sprite _rareIcon;

	[SerializeField]
	private Image _rareRadialImage;

	[SerializeField]
	private Image uncommonProgressFillImage;

	[SerializeField]
	private Image rareProgressFillImage;

	[SerializeField]
	private Localize uncommonTooltipDetailText;

	[SerializeField]
	private Localize rareTooltipDetailText;

	[SerializeField]
	private Localize rareTooltipTitleText;

	[SerializeField]
	private GameObject _rareTooltipRareCard;

	[SerializeField]
	private GameObject _rareTooltipMythicCard;

	[SerializeField]
	private ObjectiveBubble _rareObjectiveBubble;

	[SerializeField]
	private ObjectiveBubble _uncommonObjectiveBubble;

	private bool mythicIsNextRare;

	private int _trackPosition;

	public void Init(CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		_rareObjectiveBubble.Init(cardDatabase, cardViewBuilder);
		_uncommonObjectiveBubble.Init(cardDatabase, cardViewBuilder);
	}

	public void SetTrackPosition(int newTrackPosition)
	{
		int num = (newTrackPosition + 4) % 6;
		int num2 = (newTrackPosition + 1) % 6;
		mythicIsNextRare = newTrackPosition >= 11 && newTrackPosition < 17;
		int packsToUncommon = 6 - num;
		int num3 = ((newTrackPosition >= 17) ? (30 - newTrackPosition + 17) : (17 - newTrackPosition));
		int packsToRare = ((!mythicIsNextRare) ? (6 - num2) : (num3 + 6));
		_rareTooltipRareCard.UpdateActive(!mythicIsNextRare);
		_rareTooltipMythicCard.UpdateActive(mythicIsNextRare);
		_rareRadialImage.sprite = (mythicIsNextRare ? _mythicRareIcon : _rareIcon);
		uncommonProgressFillImage.fillAmount = (float)num / 6f;
		rareProgressFillImage.fillAmount = (float)num2 / 6f;
		updateText(packsToUncommon, packsToRare, num3, mythicIsNextRare);
		_trackPosition = newTrackPosition;
	}

	public void UpdateTrackPosition(int newTrackPosition)
	{
		int num = ((newTrackPosition < _trackPosition) ? (newTrackPosition + 30) : newTrackPosition) - _trackPosition;
		float timePerPip = 2f / (float)num;
		StartCoroutine(Coroutine_UpdateUncommonTrackPosition(_trackPosition, newTrackPosition, timePerPip));
		StartCoroutine(Coroutine_UpdateRareTrackPosition(_trackPosition, newTrackPosition, timePerPip));
		_trackPosition = newTrackPosition;
		int num2 = (newTrackPosition + 4) % 6;
		int num3 = (newTrackPosition + 1) % 6;
		mythicIsNextRare = newTrackPosition >= 11 && newTrackPosition < 17;
		int packsToUncommon = 6 - num2;
		int num4 = ((newTrackPosition >= 17) ? (30 - newTrackPosition + 17) : (17 - newTrackPosition));
		int packsToRare = ((!mythicIsNextRare) ? (6 - num3) : (num4 + 6));
		updateText(packsToUncommon, packsToRare, num4, mythicIsNextRare);
	}

	public IEnumerator Coroutine_UpdateUncommonTrackPosition(int trackPosition, int targetTrackPosition, float timePerPip)
	{
		int uncommonProgress = (trackPosition + 4) % 6;
		while (trackPosition != targetTrackPosition)
		{
			int num = 6 - (trackPosition + 4) % 6;
			int num2 = trackPosition + num;
			int num3 = trackPosition;
			if (trackPosition < targetTrackPosition)
			{
				trackPosition = Math.Min(num2, targetTrackPosition);
			}
			else if (num2 < 30)
			{
				trackPosition = num2;
			}
			else
			{
				trackPosition = Math.Min(num2 - 30, targetTrackPosition);
				num3 -= 30;
			}
			if (uncommonProgress == 6)
			{
				uncommonProgressFillImage.fillAmount = 0f;
			}
			uncommonProgress = (trackPosition + 4) % 6;
			if (uncommonProgress % 6 == 0)
			{
				uncommonProgress = 6;
			}
			float num4 = (float)(trackPosition - num3) * timePerPip;
			if ((float)uncommonProgress / 6f > uncommonProgressFillImage.fillAmount)
			{
				DOTween.To(() => uncommonProgressFillImage.fillAmount, delegate(float x)
				{
					uncommonProgressFillImage.fillAmount = x;
				}, (float)uncommonProgress / 6f, num4);
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_quest_main_bar_start, uncommonProgressFillImage.gameObject);
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_quest_main_bar_stop, uncommonProgressFillImage.gameObject, num4);
			}
			else
			{
				uncommonProgressFillImage.fillAmount = (float)uncommonProgress / 6f;
			}
			yield return new WaitForSeconds(num4);
		}
		if (uncommonProgress == 6)
		{
			uncommonProgressFillImage.fillAmount = 0f;
		}
	}

	public IEnumerator Coroutine_UpdateRareTrackPosition(int trackPosition, int targetTrackPosition, float timePerPip)
	{
		int rareProgress = (trackPosition + 1) % 6;
		while (trackPosition != targetTrackPosition)
		{
			int num = 6 - (trackPosition + 1) % 6;
			int num2 = trackPosition + num;
			int num3 = trackPosition;
			mythicIsNextRare = num3 >= 11 && num3 < 17;
			_rareTooltipRareCard.UpdateActive(!mythicIsNextRare);
			_rareTooltipMythicCard.UpdateActive(mythicIsNextRare);
			_rareRadialImage.sprite = (mythicIsNextRare ? _mythicRareIcon : _rareIcon);
			if (trackPosition < targetTrackPosition)
			{
				trackPosition = Math.Min(num2, targetTrackPosition);
			}
			else if (num2 < 30)
			{
				trackPosition = num2;
			}
			else
			{
				trackPosition = Math.Min(num2 - 30, targetTrackPosition);
				num3 -= 30;
			}
			if (rareProgress == 6)
			{
				rareProgressFillImage.fillAmount = 0f;
			}
			rareProgress = (trackPosition + 1) % 6;
			if (rareProgress % 6 == 0)
			{
				rareProgress = 6;
			}
			float num4 = (float)(trackPosition - num3) * timePerPip;
			if ((float)rareProgress / 6f > rareProgressFillImage.fillAmount)
			{
				DOTween.To(() => rareProgressFillImage.fillAmount, delegate(float x)
				{
					rareProgressFillImage.fillAmount = x;
				}, (float)rareProgress / 6f, num4);
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_quest_main_bar_start, rareProgressFillImage.gameObject);
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_quest_main_bar_stop, rareProgressFillImage.gameObject, num4);
			}
			else
			{
				rareProgressFillImage.fillAmount = (float)rareProgress / 6f;
			}
			yield return new WaitForSeconds(num4);
		}
		if (rareProgress == 6)
		{
			rareProgressFillImage.fillAmount = 0f;
			mythicIsNextRare = trackPosition >= 11 && trackPosition < 17;
			_rareTooltipRareCard.UpdateActive(!mythicIsNextRare);
			_rareTooltipMythicCard.UpdateActive(mythicIsNextRare);
			_rareRadialImage.sprite = (mythicIsNextRare ? _mythicRareIcon : _rareIcon);
		}
	}

	private void updateText(int packsToUncommon, int packsToRare, int packsToMythic, bool mythicIsNextRare)
	{
		uncommonTooltipDetailText.SetText("MainNav/BoosterChamber/WildcardRewards_UncommonTooltipDetails", new Dictionary<string, string>
		{
			{
				"packsToUncommon",
				packsToUncommon.ToString()
			},
			{
				"packs",
				Translate((packsToUncommon == 1) ? "MainNav/BoosterChamber/WildcardRewards_Pack" : "MainNav/BoosterChamber/WildcardRewards_Packs")
			}
		});
		rareTooltipDetailText.SetText("MainNav/BoosterChamber/WildcardRewards_RareTooltipDetails", new Dictionary<string, string>
		{
			{
				"packsToRare",
				packsToRare.ToString()
			},
			{
				"rPacks",
				Translate((packsToRare == 1) ? "MainNav/BoosterChamber/WildcardRewards_Pack" : "MainNav/BoosterChamber/WildcardRewards_Packs")
			},
			{
				"packsToMythic",
				packsToMythic.ToString()
			},
			{
				"mPacks",
				Translate((packsToMythic == 1) ? "MainNav/BoosterChamber/WildcardRewards_Pack" : "MainNav/BoosterChamber/WildcardRewards_Packs")
			}
		});
		rareTooltipTitleText.SetText(mythicIsNextRare ? "MainNav/General/MythicRareWildcard" : "MainNav/General/RareWildcard");
		static string Translate(string key)
		{
			return Languages.ActiveLocProvider.GetLocalizedText(key);
		}
	}

	private void OnEnable()
	{
		Languages.LanguageChangedSignal.Listeners += LocalizeEvent;
		LocalizeEvent();
	}

	private void OnDisable()
	{
		Languages.LanguageChangedSignal.Listeners -= LocalizeEvent;
	}

	private void LocalizeEvent()
	{
		int num = (_trackPosition + 4) % 6;
		int num2 = (_trackPosition + 1) % 6;
		mythicIsNextRare = _trackPosition >= 11 && _trackPosition < 17;
		int packsToUncommon = 6 - num;
		int num3 = ((_trackPosition >= 17) ? (30 - _trackPosition + 17) : (17 - _trackPosition));
		int packsToRare = ((!mythicIsNextRare) ? (6 - num2) : (num3 + 6));
		updateText(packsToUncommon, packsToRare, num3, mythicIsNextRare);
	}

	public void RewardWildcard()
	{
		WrapperController.Instance.NavBarController.RewardWildcard();
	}

	public void RewardUncommon()
	{
		_uncommonRewardParticles.Play();
	}

	public void RewardRare()
	{
		_rareRewardParticles.Play();
	}

	public void StopTrackSounds(float delay = 0f)
	{
		AudioManager.PostStopEvent(WwiseEvents.sfx_ui_main_quest_main_bar_start.EventName, _uncommonRewardParticles.gameObject, delay);
		AudioManager.PostStopEvent(WwiseEvents.sfx_ui_main_quest_main_bar_start.EventName, _rareRewardParticles.gameObject, delay);
	}
}
