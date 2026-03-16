using System;
using System.Collections.Generic;
using System.Linq;
using Wizards.Arena.Enums.Card;
using Wizards.Arena.Enums.Set;
using Wotc.Mtga.Wrapper;

namespace Core.Meta.MainNavigation.Store.Data;

[Serializable]
public class StoreSetFilterModel
{
	public string SetSymbol;

	public List<CollationMapping> Sets = new List<CollationMapping>();

	public List<GroupTag> Tags = new List<GroupTag>();

	public SetAvailability Availability;

	public CollationMapping SetSymbolAsCollationMapping => (CollationMapping)Enum.Parse(typeof(CollationMapping), SetSymbol);

	public HashSet<string> HashSetSet
	{
		get
		{
			HashSet<string> hashSet = new HashSet<string>();
			foreach (CollationMapping set in Sets)
			{
				hashSet.Add(set.GetName());
			}
			return hashSet;
		}
	}

	public Dictionary<string, int> SelectedSetsSortOrder => Sets.ToDictionary((CollationMapping x) => x.GetName(), (CollationMapping x) => Sets.IndexOf(x));
}
