using System;
using System.Collections.Generic;

namespace Core.Shared.Code.Utilities;

public sealed class Deletion<T> : IDiffLine<T>, IDiffLineWithPreLine
{
	public int PreLine { get; }

	public T Content { get; }

	public string Symbol => "-";

	public Deletion(int preLine, T content)
	{
		PreLine = preLine;
		Content = content;
	}

	public override string ToString()
	{
		return $"{PreLine} - {Content}";
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Deletion<T> deletion))
		{
			return false;
		}
		if (PreLine == deletion.PreLine)
		{
			return EqualityComparer<T>.Default.Equals(Content, deletion.Content);
		}
		return false;
	}

	public bool Equals(Deletion<T> other)
	{
		if (PreLine == other.PreLine)
		{
			return EqualityComparer<T>.Default.Equals(Content, other.Content);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(PreLine, Content);
	}
}
