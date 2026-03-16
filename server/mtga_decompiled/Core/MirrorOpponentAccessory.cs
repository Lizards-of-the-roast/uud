using UnityEngine;

public class MirrorOpponentAccessory : MonoBehaviour
{
	private void Start()
	{
		if (GetComponent<AccessoryController>()._ownerPlayerNum == GREPlayerNum.Opponent)
		{
			base.transform.localScale = Vector3.Scale(base.transform.localScale, new Vector3(-1f, 1f, 1f));
			base.transform.localRotation = Quaternion.Euler(new Vector3(base.transform.localRotation.x, 170f - base.transform.localRotation.y, base.transform.localRotation.z));
		}
	}
}
