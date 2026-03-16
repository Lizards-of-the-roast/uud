using System;
using HasbroGo.Errors;

namespace HasbroGo;

public class ErrorEventArgs : EventArgs
{
	public object ErrorCategory { get; private set; }

	public Error Error { get; private set; }

	public object ExtraData { get; private set; }

	public ErrorEventArgs(Error error, object errorCategory = null, object extraData = null)
	{
		Error = error;
		ErrorCategory = errorCategory;
		ExtraData = extraData;
	}
}
