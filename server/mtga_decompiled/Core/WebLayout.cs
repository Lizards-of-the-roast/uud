using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WebLayout : MonoBehaviour
{
	private void OnEnable()
	{
		UpdateWeb();
	}

	private void UpdateWeb()
	{
		Transform[] array = new Transform[base.transform.childCount];
		for (int i = 0; i < base.transform.childCount; i++)
		{
			array[i] = base.transform.GetChild(i);
		}
		float num = array[0].localRotation.eulerAngles.z / 180f * MathF.PI;
		List<Transform> list = new List<Transform>();
		float num2 = 0f;
		for (int j = 0; j < array[0].childCount + 1; j++)
		{
			Transform transform = ((j < array[0].childCount) ? array[0].GetChild(j) : null);
			float num3 = ((transform != null) ? array[0].GetChild(j).localPosition.magnitude : 0f);
			if (Mathf.Abs(num2 - num3) > 10f)
			{
				if (list.Count > 0)
				{
					Vector3 localPosition = list[0].localPosition;
					float num4 = Mathf.Atan2(localPosition.y, localPosition.x) - MathF.PI / 2f;
					if (list.Count == 1)
					{
						num4 = 0f;
					}
					else if (num4 < 0.1f)
					{
						num4 = ((list.Count == 1) ? 0f : 0.1f);
					}
					for (int k = 0; k < list.Count; k++)
					{
						float num5 = ((list.Count > 1) ? ((float)k / (float)(list.Count - 1)) : 0f);
						float num6 = num4 - num5 * num4 * 2f;
						list[k].localPosition = new Vector3(num2 * Mathf.Cos(num6 + MathF.PI / 2f), num2 * Mathf.Sin(num6 + MathF.PI / 2f), localPosition.z);
						list[k].rotation = Quaternion.identity;
						for (int l = 1; l < array.Length; l++)
						{
							int siblingIndex = list[k].GetSiblingIndex();
							if (array[l].childCount > siblingIndex)
							{
								num6 = num4 - num5 * num4 * 2f - (float)l / (float)array.Length * MathF.PI * 2f;
								num6 -= array[l].localRotation.eulerAngles.z / 180f * MathF.PI + num;
								Transform child = array[l].GetChild(siblingIndex);
								child.localPosition = new Vector3(num2 * Mathf.Cos(num6 + MathF.PI / 2f), num2 * Mathf.Sin(num6 + MathF.PI / 2f), localPosition.z);
								child.rotation = Quaternion.identity;
							}
						}
					}
					list.Clear();
				}
				num2 = num3;
			}
			list.Add(transform);
		}
	}
}
