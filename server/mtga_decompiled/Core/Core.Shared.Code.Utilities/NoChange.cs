using System;
using System.Collections.Generic;

namespace Core.Shared.Code.Utilities;

public readonly struct NoChange<T> : IDiffLine<T>, IDiffLineWithPreLine, IDiffLineWithPostLine
{
	public int PreLine { get; }

	public int PostLine { get; }

	public T Content { get; }

	public string Symbol => " ";

	public NoChange(int preLine, int postLine, T content)
	{
		PreLine = preLine;
		PostLine = postLine;
		Content = content;
	}

	public override string ToString()
	{
		return $"{PreLine} {PostLine} | {Content}";
	}

	public override bool Equals(object obj)
	{
		if (!(obj is NoChange<T> noChange))
		{
			return false;
		}
		if (PreLine == noChange.PreLine && PostLine == noChange.PostLine)
		{
			return EqualityComparer<T>.Default.Equals(Content, noChange.Content);
		}
		return false;
	}

	public bool Equals(NoChange<T> other)
	{
		if (PreLine == other.PreLine && PostLine == other.PostLine)
		{
			return EqualityComparer<T>.Default.Equals(Content, other.Content);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(PreLine, PostLine, Content);
	}
}
