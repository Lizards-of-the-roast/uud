using System;
using UnityEngine;

[Serializable]
public struct DraftPackLayoutData
{
	public AspectRatio AspectRatio;

	public Vector3 UpperLeft;

	public float Scale;

	public Vector3 Offset;

	public int ColumnCount;

	public int RowCount;
}
