using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public class CategoryData
{
	public DuelScene_CDC CategoricalCdc { get; set; }

	public List<uint> InstanceIds { get; set; }
}
