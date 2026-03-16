using System;
using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

[Serializable]
public struct Counters : ISerializationCallbackReceiver
{
	[HideInInspector]
	public string name;

	public CounterType CounterType;

	public uint NumberOfCounters;

	public void OnBeforeSerialize()
	{
		name = $"{CounterType} - {NumberOfCounters}";
	}

	public void OnAfterDeserialize()
	{
	}
}
