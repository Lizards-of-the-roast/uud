using System;
using System.Collections.Generic;

namespace Core.Shared.Code.Utilities;

public sealed class Addition<T> : IDiffLine<T>, IDiffLineWithPostLine
{
	public int PostLine { get; }

	public T Content { get; }

	public string Symbol => "+";

	public Addition(int postLine, T content)
	{
		PostLine = postLine;
		Content = content;
	}

	public override string ToString()
	{
		return $"{PostLine} + {Content}";
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Addition<T> addition))
		{
			return false;
		}
		if (PostLine == addition.PostLine)
		{
			return EqualityComparer<T>.Default.Equals(Content, addition.Content);
		}
		return false;
	}

	public bool Equals(Addition<T> other)
	{
		if (PostLine == other.PostLine)
		{
			return EqualityComparer<T>.Default.Equals(Content, other.Content);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(PostLine, Content);
	}
}
