using UnityEngine;

public class FreeAssetTrackerOnDestroy : MonoBehaviour
{
	public AssetTracker AssetTracker = new AssetTracker();

	private void OnDestroy()
	{
		AssetTracker.Cleanup();
	}
}
