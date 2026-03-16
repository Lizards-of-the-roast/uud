using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Wotc.Mtga.LearnMore;

[CreateAssetMenu(fileName = "Learn More Structure", menuName = "Learn More/Structure", order = 0)]
public class LearnMoreStructure : ScriptableObject
{
	public const string SectionsListFieldName = "_sections";

	[SerializeField]
	private List<LearnMoreSection> _sections;

	public ReadOnlyCollection<LearnMoreSection> Sections => _sections.AsReadOnly();
}
