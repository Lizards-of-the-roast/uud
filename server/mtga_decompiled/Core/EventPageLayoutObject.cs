using System;
using EventPage;
using UnityEngine;

public class EventPageLayoutObject : MonoBehaviour, IComparable<EventPageLayoutObject>
{
	[Header("Layout Information")]
	[Tooltip("Objects will be childed to the corresponding transform mapped on the scaffolding object")]
	public ComponentLocation Location;

	[Tooltip("Objects will be sorted by this index in ascending order")]
	public int ChildIndex;

	public int CompareTo(EventPageLayoutObject other)
	{
		return ChildIndex.CompareTo(other.ChildIndex);
	}
}
