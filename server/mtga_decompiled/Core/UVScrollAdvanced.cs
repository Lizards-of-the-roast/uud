using UnityEngine;

public class UVScrollAdvanced : MonoBehaviour
{
	public Vector2 Speed;

	private Material targetMaterial;

	private bool initialized;

	public string TextureName = "_EmissionMap2";

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
			initialized = targetMaterial.HasProperty(TextureName);
			if (!initialized)
			{
				Debug.LogError("UVScrollAdvanced set to target non-existent property: " + TextureName);
			}
		}
	}

	private void Update()
	{
		if (!targetMaterial.Equals(null) && initialized)
		{
			Vector2 vector = Speed * Time.deltaTime;
			Vector2 textureOffset = targetMaterial.GetTextureOffset(TextureName);
			textureOffset.x = (textureOffset.x + vector.x) % 1f;
			textureOffset.y = (textureOffset.y + vector.y) % 1f;
			targetMaterial.SetTextureOffset(TextureName, textureOffset);
		}
	}
}
