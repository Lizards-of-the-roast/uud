using UnityEngine;

public class BattlefieldChunk : MonoBehaviour
{
	public Collider boxCollider;

	public GameObject obj;

	[HideInInspector]
	public Bounds bounds => GetBoundsOfChildrenRenderer();

	[HideInInspector]
	public Bounds boundsMesh => GetBoundsOfChildrenMesh();

	public Vector3 Corner
	{
		get
		{
			Bounds bounds = boxCollider.bounds;
			return new Vector3(bounds.center.x + bounds.extents.x, base.transform.parent.position.y, bounds.center.z - bounds.extents.z);
		}
	}

	public Bounds GetBoundsOfChildrenMesh()
	{
		MeshFilter[] componentsInChildren = GetComponentsInChildren<MeshFilter>();
		Bounds result = default(Bounds);
		MeshFilter[] array = componentsInChildren;
		foreach (MeshFilter meshFilter in array)
		{
			result.Encapsulate(meshFilter.mesh.bounds);
		}
		return result;
	}

	public Bounds GetBoundsOfChildrenRenderer()
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		Bounds result = default(Bounds);
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			result.Encapsulate(renderer.bounds);
		}
		return result;
	}
}
