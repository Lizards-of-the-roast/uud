using System.Collections.Generic;

namespace Wotc.Mtga.Cards.Text;

public class ChapterTextEntry : ICardTextEntry
{
	private readonly IReadOnlyList<uint> _chapterNumbers;

	private readonly string _chapterText;

	public ChapterTextEntry(IReadOnlyList<uint> chapterNumbers, string chapterText)
	{
		_chapterNumbers = chapterNumbers;
		_chapterText = chapterText;
	}

	public string GetText()
	{
		return _chapterText;
	}

	public IReadOnlyList<uint> GetChapterNumbers()
	{
		return _chapterNumbers;
	}
}
