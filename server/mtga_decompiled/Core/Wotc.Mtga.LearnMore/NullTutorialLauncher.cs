namespace Wotc.Mtga.LearnMore;

public class NullTutorialLauncher : ITutorialLauncher
{
	public static readonly ITutorialLauncher Default = new NullTutorialLauncher();

	public void LaunchTutorial()
	{
	}

	public bool CanLaunchTutorial()
	{
		return false;
	}
}
