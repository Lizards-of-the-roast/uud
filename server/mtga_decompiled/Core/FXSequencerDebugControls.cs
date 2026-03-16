using UnityEngine;

public class FXSequencerDebugControls : MonoBehaviour
{
	public FXSequencer debugSequence;

	private void Update()
	{
		if (!Input.GetKeyDown(KeyCode.Keypad0))
		{
			return;
		}
		foreach (Transform item in debugSequence.transform)
		{
			Object.Destroy(item.gameObject);
		}
		debugSequence.enabled = true;
		debugSequence.Reinitialize();
	}
}
