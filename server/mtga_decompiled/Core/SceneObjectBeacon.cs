using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SceneObjectBeacon : MonoBehaviour
{
	public static Dictionary<string, GameObject> Beacons = new Dictionary<string, GameObject>();

	public string BeaconName;

	public bool disableOnAwake;

	public void Awake()
	{
		InitializeBeacon();
		if (disableOnAwake && Application.isPlaying)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public virtual void InitializeBeacon()
	{
		if (string.IsNullOrEmpty(BeaconName))
		{
			BeaconName = base.name;
		}
		int num = 0;
		string beaconName = BeaconName;
		while (Beacons.ContainsKey(BeaconName))
		{
			BeaconName = beaconName + " " + num++;
		}
		Beacons[BeaconName] = base.gameObject;
	}

	public void OnDestroy()
	{
		if (!Beacons.ContainsKey(BeaconName))
		{
			foreach (KeyValuePair<string, GameObject> beacon in Beacons)
			{
				if (beacon.Value == base.gameObject)
				{
					Beacons.Remove(beacon.Key);
					return;
				}
			}
		}
		Beacons.Remove(BeaconName);
	}
}
