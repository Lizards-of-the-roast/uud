using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Meta.NewPlayerExperience.Graph;
using UnityEngine;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wotc.Mtga.LearnMore;

namespace Core.Meta.MainNavigation.NavBar;

public class CodexNewPip : MonoBehaviour
{
	[SerializeField]
	private LearnMoreStructure _codexStructure;

	[SerializeField]
	private GameObject _newPipObject;

	private PlayerPrefsDataProvider _playerPrefsDataProvider;

	private NewPlayerExperienceStrategy _npeStrategy;

	private readonly List<LearnMoreSection> _lowestLevelContentSections = new List<LearnMoreSection>();

	private void OnEnable()
	{
		LearnToPlayRunTimeHierarchy.SectionSetToRead += EvaluatePip;
	}

	private void OnDisable()
	{
		LearnToPlayRunTimeHierarchy.SectionSetToRead -= EvaluatePip;
	}

	private async void Start()
	{
		_playerPrefsDataProvider = Pantry.Get<PlayerPrefsDataProvider>();
		_npeStrategy = Pantry.Get<NewPlayerExperienceStrategy>();
		await _playerPrefsDataProvider.WaitForInitialized();
		EvaluatePip();
	}

	public async Task EvaluatePip()
	{
		foreach (LearnMoreSection section in _codexStructure.Sections)
		{
			FindLowestLevelContentSections(section);
		}
		Dictionary<string, bool> milestoneStates = await _npeStrategy.GetNpeGraphMilestones();
		bool unreadSections = false;
		foreach (LearnMoreSection curSection in _lowestLevelContentSections)
		{
			Promise<bool> promise = await _playerPrefsDataProvider.GetPreferenceBool(ReadPrefKey(curSection.Id)).AsTask;
			if (promise.Successful && !promise.Result && curSection.IsSelfAccessible(milestoneStates, out var _))
			{
				unreadSections = true;
				break;
			}
		}
		_newPipObject.SetActive(unreadSections);
	}

	private void FindLowestLevelContentSections(LearnMoreSection learnMoreSection)
	{
		if (learnMoreSection == null)
		{
			return;
		}
		if (learnMoreSection.ChildSections.Length == 0)
		{
			_lowestLevelContentSections.Add(learnMoreSection);
			return;
		}
		LearnMoreSection[] childSections = learnMoreSection.ChildSections;
		foreach (LearnMoreSection learnMoreSection2 in childSections)
		{
			FindLowestLevelContentSections(learnMoreSection2);
		}
	}

	private string ReadPrefKey(string sectionId)
	{
		return "LTP_IsRead_" + sectionId;
	}
}
