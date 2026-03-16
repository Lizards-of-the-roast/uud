using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wotc.Mtga.LearnMore;

[CreateAssetMenu(fileName = "Learn More Section", menuName = "Learn More/Section", order = 150)]
public class LearnMoreSection : ScriptableObject
{
	[SerializeField]
	private string _title;

	public const string _titleFieldName = "_title";

	[SerializeField]
	private string _sectionPrefab;

	public const string SectionPrefabFieldName = "_sectionPrefab";

	[SerializeField]
	private Sprite _icon;

	public const string IconFieldName = "_icon";

	[SerializeField]
	private LearnMoreSection[] _childSections;

	public const string ChildSectionsFieldName = "_childSections";

	[SerializeField]
	public string[] _requiredMilestones;

	public const string RequiredMilestonesFieldName = "_requiredMilestones";

	[SerializeField]
	public string Id;

	public string Title => _title;

	public string SectionPrefab => _sectionPrefab;

	public Sprite Icon => _icon;

	public LearnMoreSection[] ChildSections => _childSections;

	public string[] RequiredMilestones => _requiredMilestones;

	public bool IsSelfAccessible(Dictionary<string, bool> milestoneStates, out string reason)
	{
		if (_requiredMilestones == null || _requiredMilestones.Length == 0)
		{
			reason = "no requirements";
			return true;
		}
		if (milestoneStates == null)
		{
			reason = "no milestoneStates";
			return false;
		}
		string[] requiredMilestones = _requiredMilestones;
		foreach (string text in requiredMilestones)
		{
			if (milestoneStates.TryGetValue(text, out var value) && value)
			{
				reason = "milestone " + text + " completed";
				return true;
			}
		}
		reason = "no milestones met";
		return false;
	}

	private void OnEnable()
	{
		if (string.IsNullOrWhiteSpace(Id))
		{
			Id = Guid.NewGuid().ToString();
		}
	}
}
