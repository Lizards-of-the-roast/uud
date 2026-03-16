using System;
using UnityEngine;

[Serializable]
public struct CardInitInfo : ISerializationCallbackReceiver
{
	[HideInInspector]
	public string name;

	public Transform Anchor;

	public CardHolderType CardHolderType;

	public uint DefaultGrpId;

	public bool DisableRolloverZoom;

	public Counters[] Counters;

	public void OnBeforeSerialize()
	{
		name = $"{CardHolderType} - {DefaultGrpId}";
	}

	public void OnAfterDeserialize()
	{
	}
}
