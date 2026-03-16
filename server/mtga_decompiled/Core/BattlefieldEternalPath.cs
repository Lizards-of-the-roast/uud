using System.Collections.Generic;
using UnityEngine;

public class BattlefieldEternalPath : MonoBehaviour
{
	public GameObject groundObject;

	public int zThreshold;

	private List<GameObject> _chunks = new List<GameObject>();

	private GameObject currentChunk;

	private GameObject nextChunk;

	private void Start()
	{
		currentChunk = SpawnChunk();
		nextChunk = SpawnChunk(currentChunk);
	}

	private void Update()
	{
		if (currentChunk.GetComponent<BattlefieldChunk>().Corner.z > (float)zThreshold)
		{
			GameObject obj = currentChunk;
			currentChunk = nextChunk;
			nextChunk = SpawnChunk(currentChunk);
			Object.DestroyImmediate(obj);
		}
	}

	private GameObject SpawnChunk()
	{
		GameObject obj = Object.Instantiate(groundObject, base.gameObject.transform.position, Quaternion.identity);
		obj.transform.parent = base.gameObject.transform;
		return obj;
	}

	private GameObject SpawnChunk(GameObject chunkObj)
	{
		Vector3 corner = chunkObj.GetComponent<BattlefieldChunk>().GetComponent<BattlefieldChunk>().Corner;
		GameObject obj = Object.Instantiate(groundObject, corner, Quaternion.identity);
		obj.transform.parent = base.gameObject.transform;
		return obj;
	}

	private void OnDisable()
	{
		foreach (GameObject chunk in _chunks)
		{
			Object.DestroyImmediate(chunk);
		}
		_chunks.Clear();
	}
}
