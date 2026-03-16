using Core.Shared.Code.Utilities;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
	public Vector3 MovementDirection = Vector3.left;

	public float MovementSpeed = 0.1f;

	public GameObject PlaneGameObject;

	public GameObject TargetAGameObject;

	public GameObject TargetBGameObject;

	public Material TargetAMaterial;

	public Material TargetBMaterial;

	public Vector3 InitialPosition;

	private void Start()
	{
		TargetAMaterial = TargetAGameObject.GetComponent<Renderer>().material;
		TargetBMaterial = TargetBGameObject.GetComponent<Renderer>().material;
		InitialPosition = PlaneGameObject.transform.position;
	}

	private void Update()
	{
		PlaneGameObject.transform.position += MovementSpeed * Time.deltaTime * MovementDirection;
		Vector3 position = PlaneGameObject.transform.position;
		Vector4 value = new Vector4(position.x, position.y, position.z, -1f);
		TargetAMaterial.SetVector(ShaderPropertyIds.CuttingPlanePosPropId, value);
		Vector4 value2 = new Vector4(position.x, position.y, position.z, 1f);
		TargetBMaterial.SetVector(ShaderPropertyIds.CuttingPlanePosPropId, value2);
		if (Input.GetKeyUp(KeyCode.Space))
		{
			PlaneGameObject.transform.position = InitialPosition;
		}
	}
}
