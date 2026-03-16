using System;

namespace Wizards.Mtga.Assets;

public class AssetException : Exception
{
	public object? Context { get; }

	public AssetException(string message)
		: base(message)
	{
	}

	public AssetException(string message, object? context = null)
		: this(null, message, context)
	{
	}

	public AssetException(Exception? exception, string message, object? context = null)
		: this(message, exception, context)
	{
	}

	public AssetException(string message, Exception? exception, object? context = null)
		: base(message, exception)
	{
		Context = context;
	}

	public AssetException(string message, Exception[] exceptions)
		: base(message, new AggregateException(exceptions))
	{
	}
}
