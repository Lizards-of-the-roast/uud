using AssetLookupTree.Blackboard;
using Wotc.Mtga.Cards.Text;

namespace AssetLookupTree.Extractors.CardText.Chapter;

public class CardText_Chapter_Count : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		if (bb.CardTextEntry is ChapterTextEntry chapterTextEntry)
		{
			value = chapterTextEntry.GetChapterNumbers().Count;
			return true;
		}
		value = 0;
		return false;
	}
}
