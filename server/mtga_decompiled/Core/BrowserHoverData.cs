using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "NewBrowserHoverData", menuName = "ScriptableObject/Browsers/BrowserHoverData", order = 10)]
public class BrowserHoverData : ScriptableObject
{
	public float ForwardOffset = 2.5f;

	public float YOffset = 0.35f;

	public float XOffset;

	public float Scale = 1f;
}
