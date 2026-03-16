using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Core.Meta.NewPlayerExperience.Graph;
using UnityEngine;
using Wizards.Arena.Promises;
using Wizards.Mtga;

namespace Wotc.Mtga.LearnMore;

public class LearnToPlayRunTimeHierarchy
{
	private readonly Dictionary<string, SectionObjectReferences> _sections;

	private readonly LearnMoreStructure _structure;

	private readonly PlayerPrefsDataProvider _prefs;

	private readonly NewPlayerExperienceStrategy _npeGraphStrategy;

	public static event Func<Task> SectionSetToRead;

	public LearnToPlayRunTimeHierarchy(LearnMoreStructure structure, PlayerPrefsDataProvider playerPrefsDataProvider, NewPlayerExperienceStrategy npeGraphStrategy)
	{
		_sections = new Dictionary<string, SectionObjectReferences>();
		_structure = structure;
		_prefs = playerPrefsDataProvider;
		_npeGraphStrategy = npeGraphStrategy;
	}

	public IEnumerable<SectionObjectReferences> Get()
	{
		return _sections.Values;
	}

	public bool TryGet(string id, out SectionObjectReferences refs)
	{
		refs = null;
		if (id == null)
		{
			return false;
		}
		if (_sections.TryGetValue(id, out refs))
		{
			return true;
		}
		StackTrace arg = new StackTrace(0, fNeedFileInfo: true);
		SimpleLog.LogError($"LTP: {id}: Section Refs missing{Environment.NewLine}{arg}");
		return false;
	}

	public async Task Populate()
	{
		_sections.Clear();
		Dictionary<string, bool> milestoneStates = await _npeGraphStrategy.GetNpeGraphMilestones();
		SectionObjectReferences[] noAncestors = Array.Empty<SectionObjectReferences>();
		foreach (LearnMoreSection section in _structure.Sections)
		{
			await PopulateSections(section, noAncestors, milestoneStates);
		}
	}

	private async Task<SectionObjectReferences> PopulateSections(LearnMoreSection section, SectionObjectReferences[] ancestors, Dictionary<string, bool> milestoneStates)
	{
		if (section == null)
		{
			SimpleLog.LogError("LTP: PopulateSections: Null Learn More Table of Contents section with ancestors " + string.Join(",", ancestors.Select((SectionObjectReferences _) => _.ToString())));
			return null;
		}
		string text = SectionObjectReferences.AttemptedLocalizedSectionTitle(section);
		string path = SectionObjectReferences.ToDebugPath(ancestors) + ">" + text;
		if (_sections.TryGetValue(section.Id, out var value))
		{
			SimpleLog.LogError("LTP: " + path + ": PopulateSections: Duplicate section id " + section.Id + ", prior " + value.Path);
			return null;
		}
		try
		{
			string selfAccessibleReason;
			bool isSelfAccessible = section.IsSelfAccessible(milestoneStates, out selfAccessibleReason);
			Promise<bool> promise = await IsTopicRead(section.Id).AsTask;
			SectionObjectReferences sectionObjectRefs = new SectionObjectReferences(section, ancestors, isSelfAccessible, !promise.Successful || promise.Result);
			_sections.Add(section.Id, sectionObjectRefs);
			bool showChild = false;
			bool newChild = false;
			if (section.ChildSections != null && section.ChildSections.Length != 0)
			{
				SectionObjectReferences[] ancestorsPlusMe = ancestors.Append(sectionObjectRefs).ToArray();
				LearnMoreSection[] childSections = section.ChildSections;
				foreach (LearnMoreSection section2 in childSections)
				{
					SectionObjectReferences sectionObjectReferences = await PopulateSections(section2, ancestorsPlusMe, milestoneStates);
					sectionObjectRefs.Children.Add(sectionObjectReferences);
					if (sectionObjectReferences.Show)
					{
						showChild = true;
						UnityEngine.Debug.Log(string.Format("LTP: {0}: {1}: show child {2}", path, "PopulateSections", sectionObjectReferences));
						if (sectionObjectReferences.ShowNewFlag)
						{
							newChild = true;
							UnityEngine.Debug.Log(string.Format("LTP: {0}: {1}: new child {2}", path, "PopulateSections", sectionObjectReferences));
						}
					}
				}
			}
			sectionObjectRefs.InitializeShowAndNew(showChild, newChild, selfAccessibleReason);
			return sectionObjectRefs;
		}
		catch (Exception arg)
		{
			SimpleLog.LogError(string.Format("LTP: {0}: {1}: Exception {2}", path, "PopulateSections", arg));
			return null;
		}
	}

	private string ReadPrefKey(string topicId)
	{
		return "LTP_IsRead_" + topicId;
	}

	private Promise<bool> IsTopicRead(string topicId)
	{
		return _prefs.GetPreferenceBool(ReadPrefKey(topicId));
	}

	private void SetTopicRead(string topicId)
	{
		_prefs.SetPreferenceBool(ReadPrefKey(topicId), value: true);
	}

	public void SetRead(SectionObjectReferences sectionRefs)
	{
		SetTopicRead(sectionRefs.Id);
		sectionRefs.IsSelfRead = true;
		sectionRefs.ShowNewFlag = false;
		TurnOffAncestorNewFlagsIfAllChildrenRead(sectionRefs.Parent);
		LearnToPlayRunTimeHierarchy.SectionSetToRead?.Invoke();
	}

	private void TurnOffAncestorNewFlagsIfAllChildrenRead(SectionObjectReferences lowestParent)
	{
		while (lowestParent != null && TurnOffParentNewFlagIfAllChildrenRead(lowestParent))
		{
			lowestParent = lowestParent.Parent;
		}
	}

	private bool TurnOffParentNewFlagIfAllChildrenRead(SectionObjectReferences parentRefs)
	{
		bool flag = false;
		foreach (SectionObjectReferences child in parentRefs.Children)
		{
			if (child.ShowNewFlag)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			parentRefs.ShowNewFlag = false;
			return true;
		}
		return false;
	}
}
