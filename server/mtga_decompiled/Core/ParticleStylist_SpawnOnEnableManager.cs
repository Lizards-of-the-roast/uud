using System.Collections.Generic;
using UnityEngine;

public class ParticleStylist_SpawnOnEnableManager : MonoBehaviour
{
	[Header("Dynamic Lists")]
	public List<SpawnOnEnable> spawnOnEnables = new List<SpawnOnEnable>();

	public List<ParticleStylist> stylists = new List<ParticleStylist>();

	private Dictionary<string, ParticleStylist> styleEvents = new Dictionary<string, ParticleStylist>();

	private void Awake()
	{
		foreach (ParticleStylist s in stylists)
		{
			ParticleStylist value = stylists.Find((ParticleStylist x) => x.prefab == s.prefab);
			styleEvents.Add(s.prefab.name + "(Clone)", value);
		}
		foreach (SpawnOnEnable spawnOnEnable in spawnOnEnables)
		{
			if (spawnOnEnable.OnInstantiate != null)
			{
				Debug.LogWarning("Multiple Stylist Conflict: Multiple stylists are trying to effect the same spawn on enable, we cannot support this.");
			}
			spawnOnEnable.OnInstantiate = ColorOnSpawn;
		}
	}

	private void ColorOnSpawn(GameObject instance)
	{
		styleEvents[instance.name]?.Apply(instance);
	}

	private void OnValidate()
	{
	}

	private void Configure()
	{
	}
}
