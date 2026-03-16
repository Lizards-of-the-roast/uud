namespace Wotc.Mtga.AutoPlay;

public class ScriptResult
{
	public bool IsSuccess;

	public AutoPlayAction.Failure Failure;

	private ScriptResult(bool success, AutoPlayAction.Failure failure)
	{
		IsSuccess = success;
		Failure = failure;
	}

	public static ScriptResult Success()
	{
		return new ScriptResult(success: true, null);
	}

	public static ScriptResult Fail(AutoPlayAction.Failure failure)
	{
		return new ScriptResult(success: false, failure);
	}
}
