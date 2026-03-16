using System;
using UnityEngine;
using UnityEngine.Serialization;

public class AccessoryVariantCycler : MonoBehaviour
{
	[Serializable]
	private class Variants
	{
		[SerializeField]
		private GameObject[] VariantObjects;

		public void Activate()
		{
			GameObject[] variantObjects = VariantObjects;
			for (int i = 0; i < variantObjects.Length; i++)
			{
				variantObjects[i].SetActive(value: true);
			}
		}
	}

	[SerializeField]
	[FormerlySerializedAs("name")]
	private string _name;

	[SerializeField]
	private GameObject[] disableOnChange;

	[SerializeField]
	private Variants[] accessoryVariants;

	[HideInInspector]
	public int variantIdx;

	public void SetAccessoryVariant(int idx)
	{
		GameObject[] array = disableOnChange;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
		accessoryVariants[idx].Activate();
	}
}
