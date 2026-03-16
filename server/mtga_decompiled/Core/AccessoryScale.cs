using UnityEngine;

public class AccessoryScale : MonoBehaviour
{
	public Vector3 LocalScale = new Vector3(1f, 1f, 1f);

	public Vector3 OpponentScale = new Vector3(1f, 1f, 1f);

	private bool scaleSet;

	private void Update()
	{
		if (!scaleSet)
		{
			SetScale();
			scaleSet = true;
		}
	}

	private void SetScale()
	{
		if (base.transform.parent.name == "LocalTotemRoot")
		{
			base.transform.localScale = LocalScale;
		}
		if (base.transform.parent.name == "OpponentTotemRoot")
		{
			base.transform.localScale = OpponentScale;
		}
	}
}
