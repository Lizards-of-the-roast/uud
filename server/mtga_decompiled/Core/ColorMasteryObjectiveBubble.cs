using System;
using System.Collections.Generic;
using UnityEngine;
using Wizards.MDN;
using Wizards.Mtga;
using Wotc.Mtga.Events;

public class ColorMasteryObjectiveBubble : ObjectiveBubble
{
	private static readonly int WhiteFinished = Animator.StringToHash("White");

	private static readonly int BlueFinished = Animator.StringToHash("Blue");

	private static readonly int BlackFinished = Animator.StringToHash("Black");

	private static readonly int RedFinished = Animator.StringToHash("Red");

	private static readonly int GreenFinished = Animator.StringToHash("Green");

	private static readonly int PulseProgress = Animator.StringToHash("PulseProgress");

	private List<string> _finishedColors = new List<string>();

	private float _radialFill;

	private EventContext ColorMasteryEvent => WrapperController.Instance.EventManager.ColorMasteryEventContext;

	private CampaignGraphManager GraphManager => Pantry.Get<CampaignGraphManager>();

	public override bool Tickable
	{
		get
		{
			if (!(_progressFillImage.fillAmount < _radialFill))
			{
				return base.Tickable;
			}
			return true;
		}
	}

	public override bool Clickable => true;

	private void Awake()
	{
		base.enabled = false;
	}

	private void Start()
	{
		SetColorMasteryData();
	}

	public void SetFinishedColor(params string[] colors)
	{
		for (int i = 0; i < colors.Length; i++)
		{
			switch (colors[i])
			{
			case "white":
				_animator.SetBool(WhiteFinished, value: true);
				break;
			case "blue":
				_animator.SetBool(BlueFinished, value: true);
				break;
			case "black":
				_animator.SetBool(BlackFinished, value: true);
				break;
			case "red":
				_animator.SetBool(RedFinished, value: true);
				break;
			case "green":
				_animator.SetBool(GreenFinished, value: true);
				break;
			}
		}
	}

	public override void Tick(Action<RewardObjectiveContext> onTickedCallback)
	{
		if (Tickable)
		{
			IColorChallengePlayerEvent obj = (IColorChallengePlayerEvent)ColorMasteryEvent.PlayerEvent;
			int completedGames = obj.CompletedGames;
			int totalGames = obj.TotalGames;
			UpdateRaidalFill();
			UpdateFinishedColors();
			bool num = Reference_endProgress >= Reference_questData.Goal && completedGames >= totalGames;
			MTGALocalizedString xOfYLocString = ContentControllerObjectives.GetXOfYLocString(Reference_questData.EndingProgress.ToString(), Reference_questData.Goal.ToString());
			SetProgressText(xOfYLocString);
			if (num)
			{
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_quest_complete, base.gameObject);
				PlayCompletePulse();
			}
			else
			{
				bool doLevelUp = false;
				PlayProgressPulse(doLevelUp);
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_quest_progress_poof, base.gameObject);
			}
			onTickedCallback?.Invoke(new RewardObjectiveContext
			{
				contextString = Reference_rewardContext,
				questData = Reference_questData,
				ClientInventoryUpdateReportItem = Reference_invUpdate
			});
		}
	}

	public void UpdateView(bool waitForTick)
	{
		if (!waitForTick)
		{
			UpdateRaidalFill();
			UpdateFinishedColors();
		}
	}

	public void UpdateRaidalFill()
	{
		IColorChallengePlayerEvent obj = (IColorChallengePlayerEvent)ColorMasteryEvent.PlayerEvent;
		int completedGames = obj.CompletedGames;
		int totalGames = obj.TotalGames;
		_radialFill = (float)completedGames / (float)totalGames;
		SetRadialFill(_radialFill);
	}

	public void UpdateFinishedColors()
	{
		IColorChallengePlayerEvent colorChallengePlayerEvent = (IColorChallengePlayerEvent)ColorMasteryEvent.PlayerEvent;
		_finishedColors.Clear();
		_finishedColors = colorChallengePlayerEvent.CompletedTracks;
		foreach (string finishedColor in _finishedColors)
		{
			SetFinishedColor(finishedColor);
		}
	}

	public new void PlayProgressPulse(bool doLevelUp = false)
	{
		_animator.SetBool(PulseProgress, value: true);
		UpdateView(waitForTick: false);
	}

	public void SetColorMasteryData()
	{
		UpdateRaidalFill();
		UpdateFinishedColors();
		_mainImageText.SetText("MainNav/General/Empty_String");
		_mainImageText.gameObject.SetActive(value: false);
		_textShadow.SetActive(value: false);
		if (Reference_questData != null)
		{
			MTGALocalizedString mTGALocalizedString = Reference_questData.LocKey;
			mTGALocalizedString.Parameters = new Dictionary<string, string> { 
			{
				"quantity",
				Reference_questData.Goal.ToString()
			} };
			MTGALocalizedString xOfYLocString = ContentControllerObjectives.GetXOfYLocString(Reference_questData.StartingProgress.ToString(), Reference_questData.Goal.ToString());
			SetProgressText(xOfYLocString);
			SetPopupDescription(mTGALocalizedString);
		}
	}
}
