using System;

namespace Wotc.Mtga.DuelScene.Logging;

public class NullBILoggerAdapter : IBILoggerAdapter
{
	public void Log(params (string, string)[] payload)
	{
		throw new NotImplementedException();
	}
}
