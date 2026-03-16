using System.Collections;

public static class WrapperControllerExtensions
{
	public static IEnumerator WithLoadingIndicator(this IEnumerator innerEnumerator)
	{
		WrapperController.EnableLoadingIndicator(enabled: true);
		yield return innerEnumerator;
		WrapperController.EnableLoadingIndicator(enabled: false);
	}
}
