using System;
using System.Collections.Generic;
using Assets.Core.Shared.Code;
using Core.Meta.MainNavigation.Achievements;
using UnityEngine;
using Wotc.Mtga.Loc;

public class AchievementEndTimeDisplay : MonoBehaviour
{
	[SerializeField]
	private Localize _expirationLabel;

	private TimeSpan _lastTimeLeft;

	private DateTime _endTime;

	private BladeSetAnimationController _animator;

	private bool _showDisplay;

	private IClientLocProvider _locProvider;

	public void Init(bool shouldShow, DateTime endTime, IClientLocProvider locProvider)
	{
		_showDisplay = shouldShow;
		_endTime = endTime;
		_locProvider = locProvider;
	}

	private void Update()
	{
		if (!_showDisplay)
		{
			return;
		}
		TimeSpan lastTimeLeft = _endTime - ServerGameTime.GameTime;
		if (_lastTimeLeft.Seconds != lastTimeLeft.Seconds)
		{
			_lastTimeLeft = lastTimeLeft;
			MTGALocalizedString mTGALocalizedString;
			if (lastTimeLeft.TotalDays > 2.0)
			{
				mTGALocalizedString = "MainNav/Achievements/AchievementSetEnds_Days";
				mTGALocalizedString.Parameters = new Dictionary<string, string> { 
				{
					"days",
					lastTimeLeft.TotalDays.ToString("n0")
				} };
			}
			else if (lastTimeLeft.TotalHours > 8.0)
			{
				mTGALocalizedString = "MainNav/Achievements/AchievementSetEnds_Hours";
				mTGALocalizedString.Parameters = new Dictionary<string, string> { 
				{
					"hours",
					lastTimeLeft.Hours.ToString()
				} };
			}
			else if ((int)lastTimeLeft.TotalHours == 1)
			{
				mTGALocalizedString = ((lastTimeLeft.Minutes == 1) ? "MainNav/Achievements/AchievementSetEnds_HourAndMinute" : "MainNav/Achievements/AchievementSetEnds_HourAndMinutes");
				mTGALocalizedString.Parameters = ((lastTimeLeft.Minutes == 1) ? null : new Dictionary<string, string> { 
				{
					"minutes",
					lastTimeLeft.Minutes.ToString()
				} });
			}
			else if (lastTimeLeft.TotalHours > 1.0)
			{
				mTGALocalizedString = ((lastTimeLeft.Minutes == 1) ? "MainNav/Achievements/AchievementSetEnds_HoursAndMinute" : "MainNav/Achievements/AchievementSetEnds_HoursAndMinutes");
				mTGALocalizedString.Parameters = ((lastTimeLeft.Minutes == 1) ? new Dictionary<string, string> { 
				{
					"hours",
					lastTimeLeft.Hours.ToString()
				} } : new Dictionary<string, string>
				{
					{
						"hours",
						lastTimeLeft.Hours.ToString()
					},
					{
						"minutes",
						lastTimeLeft.Minutes.ToString()
					}
				});
			}
			else if (lastTimeLeft.Minutes <= 1)
			{
				mTGALocalizedString = ((lastTimeLeft.Minutes != 1) ? ((MTGALocalizedString)"MainNav/Achievements/AchievementSetEnds_LessThanMinute") : ((MTGALocalizedString)"MainNav/Achievements/AchievementSetEnds_Minute"));
			}
			else
			{
				mTGALocalizedString = "MainNav/Achievements/AchievementSetEnds_Minutes";
				mTGALocalizedString.Parameters = new Dictionary<string, string> { 
				{
					"minutes",
					lastTimeLeft.Minutes.ToString()
				} };
			}
			_expirationLabel.SetText(mTGALocalizedString);
		}
	}
}
