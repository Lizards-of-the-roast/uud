using Assets.Core.Meta.LearnMore;
using UnityEngine;

namespace Wotc.Mtga.LearnMore;

public interface ITableOfContentsSectionBuilder
{
	TableOfContentsSection Create(int contentType, Transform parent);

	void Destroy(TableOfContentsSection tableOfContentsSection);
}
