using System.Linq;
using AssetLookupTree.Blackboard;
using Wotc.Mtga.Cards.Text;

namespace AssetLookupTree.Evaluators.CardText.Chapter;

public class CardText_Chapter_Numbers : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardTextEntry is ChapterTextEntry chapterTextEntry)
		{
			return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, from x in chapterTextEntry.GetChapterNumbers()
				select (int)x, MinCount, MaxCount);
		}
		return false;
	}
}
