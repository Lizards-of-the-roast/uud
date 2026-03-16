using System.Collections.Generic;
using Core.Shared.Code.ClientModels;
using Wizards.Unification.Models;

namespace Core.Shared.Code.Providers;

public class SurveyConfigProvider : ISurveyConfigProvider
{
	public static string POST_MATCH_SURVEY = "PostMatchSurvey";

	private List<DTO_Survey> _surveys;

	public static SurveyConfigProvider Create()
	{
		return new SurveyConfigProvider();
	}

	public void SetData(List<DTO_Survey> surveyData)
	{
		_surveys = surveyData;
	}

	public Client_Survey GetSurveyConfig(string surveyName)
	{
		if (_surveys != null && _surveys.Count > 0)
		{
			foreach (DTO_Survey survey in _surveys)
			{
				if (survey.Name == surveyName)
				{
					return Client_Survey.ConvertFromDTO(survey);
				}
			}
		}
		return null;
	}
}
