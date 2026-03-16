using UnityEngine;

public class AccessoryRotation : MonoBehaviour
{
	public Vector3 LocalRotation = new Vector3(0f, 10f, 0f);

	public Vector3 OpponentRotation = new Vector3(0f, 40f, 0f);

	public bool LiveMode;

	private bool rotationSet;

	private void Start()
	{
		LiveMode = false;
	}

	private void Update()
	{
		if (LiveMode)
		{
			SetRotation();
		}
		if (!rotationSet)
		{
			SetRotation();
			rotationSet = true;
		}
	}

	private void SetRotation()
	{
		if (base.transform.parent.name == "LocalTotemRoot")
		{
			base.transform.localRotation = Quaternion.Euler(LocalRotation);
		}
		if (base.transform.parent.name == "OpponentTotemRoot")
		{
			base.transform.localRotation = Quaternion.Euler(OpponentRotation);
		}
	}
}
