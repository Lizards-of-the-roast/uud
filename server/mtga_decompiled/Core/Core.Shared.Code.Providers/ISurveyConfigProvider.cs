using System.Collections.Generic;
using Core.Shared.Code.ClientModels;
using Wizards.Unification.Models;

namespace Core.Shared.Code.Providers;

public interface ISurveyConfigProvider
{
	void SetData(List<DTO_Survey> surveyData);

	Client_Survey GetSurveyConfig(string surveyName);
}
