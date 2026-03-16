using System;
using System.Collections.Generic;
using Pooling;
using UnityEngine;
using Wizards.Mtga;

public class CritterManager : MonoBehaviour
{
	[Serializable]
	public class CritterInfo
	{
		public CritterBehaviour Prefab;

		public List<SplineMovementData> PossibleSplineMovementDatas;

		public float MinSpeed;

		public float MaxSpeed;

		public List<Transform> PossibleSpawnLocations;
	}

	public float critterCoolDownMin = 5f;

	public float critterCoolDownMax = 20f;

	public CritterInfo[] critters;

	private float timeToCritter;

	public readonly List<CritterBehaviour> activeCritters = new List<CritterBehaviour>();

	private IUnityObjectPool _objectPool;

	public void Awake()
	{
		_objectPool = Pantry.Get<IUnityObjectPool>();
	}

	public void AllSkitter()
	{
		for (int i = 0; i < activeCritters.Count; i++)
		{
			activeCritters[i].Skitter();
		}
	}

	private void Update()
	{
		timeToCritter -= Time.deltaTime;
		if (timeToCritter <= 0f)
		{
			SpawnRandomCritter();
		}
	}

	private void SpawnRandomCritter()
	{
		if (critters.Length != 0)
		{
			int num = UnityEngine.Random.Range(0, critters.Length);
			timeToCritter = UnityEngine.Random.Range(critterCoolDownMin, critterCoolDownMax);
			CritterInfo critterInfo = critters[num];
			CritterBehaviour critterBehaviour = ((_objectPool == null) ? UnityEngine.Object.Instantiate(critterInfo.Prefab).GetComponent<CritterBehaviour>() : _objectPool.PopObject(critterInfo.Prefab.gameObject).GetComponent<CritterBehaviour>());
			activeCritters.Add(critterBehaviour);
			critterBehaviour.transform.parent = base.transform;
			critterBehaviour.Manager = this;
			if (critterInfo.Prefab._critterType == CritterBehaviour.CritterType.splineCritter)
			{
				SplineMovementData splineData = critterInfo.PossibleSplineMovementDatas[UnityEngine.Random.Range(0, critterInfo.PossibleSplineMovementDatas.Count)];
				float speed = UnityEngine.Random.Range(critterInfo.MinSpeed, critterInfo.MaxSpeed);
				StartCoroutine(critterBehaviour.SplineBehave(splineData, speed));
			}
			else if (critterInfo.Prefab._critterType == CritterBehaviour.CritterType.animatedMotionCritter)
			{
				Transform transform = critterInfo.PossibleSpawnLocations[UnityEngine.Random.Range(0, critterInfo.PossibleSpawnLocations.Count)];
				critterBehaviour.transform.position = transform.position;
				critterBehaviour.transform.rotation = transform.rotation;
				StartCoroutine(critterBehaviour.AnimBehave());
			}
		}
	}
}
