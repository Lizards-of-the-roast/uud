using UnityEngine;

public class EnableOnEnable : MonoBehaviour
{
	public GameObject[] Control;

	[Tooltip("Will set Control object to the opposite of this objects state. !Enabled.")]
	public bool OppositeState;

	private void OnEnable()
	{
		SetState(!OppositeState);
	}

	private void OnDisable()
	{
		SetState(OppositeState);
	}

	public void SetState(bool state)
	{
		for (int i = 0; i < Control.Length; i++)
		{
			if ((bool)Control[i])
			{
				Control[i].SetActive(state);
			}
		}
	}
}
