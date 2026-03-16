using UnityEngine;

public class ObjWorldPosToScreen : MonoBehaviour
{
	private Transform target;

	private Camera cam;

	private Material _Material;

	[Tooltip("Property name in the material you would like to set")]
	public string materialPropertyName = "scriptOffset";

	public bool setInMaterial = true;

	private void Start()
	{
		Setup();
		_Material = GetComponent<Renderer>().material;
	}

	private void Setup()
	{
		target = base.transform;
		cam = Camera.main;
	}

	private void Update()
	{
		if (target == null || cam == null)
		{
			Setup();
		}
		Vector3 vector = cam.WorldToScreenPoint(target.position);
		Vector2 vector2 = new Vector2(vector.x / (float)Screen.width, vector.y / (float)Screen.height);
		Vector4 value = new Vector4(vector2.x, vector2.y, 0f, 0f);
		if (setInMaterial)
		{
			_Material.SetVector(materialPropertyName, value);
		}
	}
}
