using System;
using Assets.Core.Shared.Code;
using Core.Shared.Code.ClientModels;
using TMPro;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Loc;

public class SeasonEndText : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _text;

	private SeasonAndRankDataProvider _seasonAndRankDataProvider;

	private void Awake()
	{
		_seasonAndRankDataProvider = Pantry.Get<SeasonAndRankDataProvider>();
	}

	private void Update()
	{
		if (_text == null)
		{
			return;
		}
		Client_SeasonInfo seasonInfo = GetSeasonInfo();
		if (seasonInfo == null || Languages.ActiveLocProvider == null)
		{
			_text.text = "";
			return;
		}
		TimeSpan timeSpan = seasonInfo.seasonEndTime - ServerGameTime.GameTime;
		if (timeSpan.TotalDays > 2.0)
		{
			_text.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Season/SeasonEnds_Days", ("days", timeSpan.Days.ToString()));
		}
		else if (timeSpan.TotalHours > 8.0)
		{
			_text.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Season/SeasonEnds_Hours", ("hours", ((int)timeSpan.TotalHours).ToString()));
		}
		else if (timeSpan.TotalHours > 1.0)
		{
			_text.text = ((timeSpan.Minutes == 1) ? Languages.ActiveLocProvider.GetLocalizedText("MainNav/Season/SeasonEnds_HoursAndMinute", ("hours", timeSpan.Hours.ToString())) : Languages.ActiveLocProvider.GetLocalizedText("MainNav/Season/SeasonEnds_HoursAndMinutes", ("hours", timeSpan.Hours.ToString()), ("minutes", timeSpan.Minutes.ToString())));
		}
		else if ((int)timeSpan.TotalHours == 1)
		{
			_text.text = ((timeSpan.Minutes == 1) ? Languages.ActiveLocProvider.GetLocalizedText("MainNav/Season/SeasonEnds_HourAndMinute") : Languages.ActiveLocProvider.GetLocalizedText("MainNav/Season/SeasonEnds_HourAndMinutes", ("minutes", timeSpan.Minutes.ToString())));
		}
		else if (timeSpan.Minutes > 1)
		{
			_text.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Season/SeasonEnds_Minutes", ("minutes", timeSpan.Minutes.ToString()));
		}
		else if (timeSpan.Minutes == 1)
		{
			_text.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Season/SeasonEnds_Minute");
		}
		else
		{
			_text.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Season/SeasonEnds_LessThanMinute");
		}
	}

	private Client_SeasonInfo GetSeasonInfo()
	{
		Client_SeasonInfo client_SeasonInfo = _seasonAndRankDataProvider.SeasonInfo?.currentSeason;
		if (client_SeasonInfo == null || client_SeasonInfo.seasonOrdinal == 0)
		{
			return null;
		}
		return client_SeasonInfo;
	}
}
