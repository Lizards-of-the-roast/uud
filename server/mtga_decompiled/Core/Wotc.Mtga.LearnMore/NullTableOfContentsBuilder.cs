using Assets.Core.Meta.LearnMore;
using UnityEngine;

namespace Wotc.Mtga.LearnMore;

public class NullTableOfContentsBuilder : ITableOfContentsSectionBuilder
{
	public static readonly ITableOfContentsSectionBuilder Default = new NullTableOfContentsBuilder();

	public TableOfContentsSection Create(int contentType, Transform parent)
	{
		return null;
	}

	public void Destroy(TableOfContentsSection tableOfContentsSection)
	{
	}
}
