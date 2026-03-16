using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wotc.Mtga.Extensions;

public class AnimateMaterialParameter : MonoBehaviour
{
	[SerializeField]
	private bool animateMultipleMeshes;

	[SerializeField]
	private bool useMatBlocks = true;

	[SerializeField]
	private GameObject targetMesh;

	[Tooltip("Use for multiple meshes that animate same property")]
	[DisableIf("animateMultipleMeshes")]
	[SerializeField]
	private GameObject[] multiTargetMeshes;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private string[] parameters;

	private Material[] materials;

	private MaterialPropertyBlock matBlock;

	private List<MeshRenderer> meshRenderers;

	private List<SkinnedMeshRenderer> skinnedMeshRenderers;

	[ContextMenu("Toggle Material Mode")]
	private void ToggleMaterialMode()
	{
		useMatBlocks = !useMatBlocks;
	}

	private void Start()
	{
		SetMaterials();
	}

	public void SetMaterials()
	{
		meshRenderers = new List<MeshRenderer>();
		skinnedMeshRenderers = new List<SkinnedMeshRenderer>();
		if (targetMesh != null)
		{
			AddRenderersFromGameObject(targetMesh);
		}
		if (animateMultipleMeshes && multiTargetMeshes != null && multiTargetMeshes.Length != 0)
		{
			GameObject[] array = multiTargetMeshes;
			foreach (GameObject obj in array)
			{
				AddRenderersFromGameObject(obj);
			}
		}
		IEnumerable<Material> first = meshRenderers.SelectMany((MeshRenderer r) => r.sharedMaterials);
		IEnumerable<Material> second = skinnedMeshRenderers.SelectMany((SkinnedMeshRenderer r) => r.sharedMaterials);
		materials = first.Concat(second).ToArray();
		matBlock = new MaterialPropertyBlock();
	}

	private void AddRenderersFromGameObject(GameObject obj)
	{
		if (obj.TryGetComponent<MeshRenderer>(out var component))
		{
			meshRenderers.Add(component);
		}
		if (obj.TryGetComponent<SkinnedMeshRenderer>(out var component2))
		{
			skinnedMeshRenderers.Add(component2);
		}
	}

	private void LateUpdate()
	{
		SetFloat();
	}

	public void SetFloat()
	{
		string[] array = parameters;
		foreach (string text in array)
		{
			if (!animator.ContainsParameter(Animator.StringToHash(text)))
			{
				continue;
			}
			float value = animator.GetFloat(text);
			if (useMatBlocks)
			{
				matBlock.SetFloat(text, value);
				foreach (MeshRenderer meshRenderer in meshRenderers)
				{
					for (int j = 0; j < meshRenderer.sharedMaterials.Length; j++)
					{
						meshRenderer.SetPropertyBlock(matBlock, j);
					}
				}
				foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
				{
					for (int k = 0; k < skinnedMeshRenderer.sharedMaterials.Length; k++)
					{
						skinnedMeshRenderer.SetPropertyBlock(matBlock, k);
					}
				}
				continue;
			}
			Material[] array2 = materials;
			foreach (Material material in array2)
			{
				if (material.HasProperty(text))
				{
					material.SetFloat(text, value);
				}
			}
		}
	}
}
