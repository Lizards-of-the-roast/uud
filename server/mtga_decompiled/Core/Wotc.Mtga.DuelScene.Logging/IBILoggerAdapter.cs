namespace Wotc.Mtga.DuelScene.Logging;

public interface IBILoggerAdapter
{
	void Log(params (string, string)[] payload);
}
