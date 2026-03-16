using UnityEngine;

public class UVScroll : MonoBehaviour
{
	public Vector2 Speed;

	private Material targetMaterial;

	private bool useTSO;

	private const string propNameTSO = "_TextureScaleOffset";

	private void Start()
	{
		MeshRenderer component = GetComponent<MeshRenderer>();
		if (component != null)
		{
			targetMaterial = component.material;
		}
		SkinnedMeshRenderer component2 = GetComponent<SkinnedMeshRenderer>();
		if (component2 != null)
		{
			targetMaterial = component2.material;
		}
		if (!(targetMaterial == null))
		{
			useTSO = targetMaterial.HasProperty("_TextureScaleOffset");
		}
	}

	private void Update()
	{
		if (!(targetMaterial == null))
		{
			Vector2 vector = Speed * Time.deltaTime;
			if (useTSO)
			{
				Vector4 vector2 = targetMaterial.GetVector("_TextureScaleOffset");
				vector2.z = (vector2.z + vector.x) % 1f;
				vector2.w = (vector2.w + vector.y) % 1f;
				targetMaterial.SetVector("_TextureScaleOffset", vector2);
			}
			else
			{
				Vector2 mainTextureOffset = targetMaterial.mainTextureOffset;
				mainTextureOffset.x = (mainTextureOffset.x + vector.x) % 1f;
				mainTextureOffset.y = (mainTextureOffset.y + vector.y) % 1f;
				targetMaterial.mainTextureOffset = mainTextureOffset;
			}
		}
	}
}
