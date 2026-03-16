using System;
using UnityEngine;

public class ParentTheseToThat : MonoBehaviour
{
	[Serializable]
	private class ParentGroup
	{
		public Transform NewParent;

		public bool MaintainOffset;

		public Transform[] ParentTheseThings = new Transform[1];

		public void Execute()
		{
			Transform[] parentTheseThings = ParentTheseThings;
			foreach (Transform transform in parentTheseThings)
			{
				if (transform == null || NewParent == null)
				{
					break;
				}
				transform.parent = NewParent;
				if (!MaintainOffset)
				{
					transform.localPosition = Vector3.zero;
					transform.localRotation = Quaternion.identity;
				}
			}
		}
	}

	[SerializeField]
	private ParentGroup[] ParentGroups = new ParentGroup[1];

	private void Start()
	{
		ParentGroup[] parentGroups = ParentGroups;
		for (int i = 0; i < parentGroups.Length; i++)
		{
			parentGroups[i].Execute();
		}
	}
}
