using UnityEngine;

[ExecuteInEditMode]
public class SpaceChildrenEvenly : MonoBehaviour
{
	public Vector3 LocalDirection = Vector3.down;

	public float Distance = 1f;

	private void OnEnable()
	{
		Layout();
	}

	private void OnTransformChildrenChanged()
	{
		Layout();
	}

	private void LateUpdate()
	{
		Layout();
	}

	[ContextMenu("Layout Now")]
	private void Layout()
	{
		int num = 0;
		for (int i = 0; i < base.transform.childCount; i++)
		{
			if (base.transform.GetChild(i).gameObject.activeSelf)
			{
				base.transform.GetChild(i).localPosition = LocalDirection * Distance * num;
				num++;
			}
		}
	}
}
