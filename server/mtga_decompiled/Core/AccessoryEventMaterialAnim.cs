using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccessoryEventMaterialAnim : MonoBehaviour
{
	[SerializeField]
	private float duration = 1f;

	[SerializeField]
	private string parameter = "";

	[SerializeField]
	private float startValue = 1f;

	[SerializeField]
	private float endValue;

	[SerializeField]
	private GameObject[] targetMeshes;

	private float _value;

	private MaterialPropertyBlock _matBlock;

	private List<SkinnedMeshRenderer> _skin_renderers = new List<SkinnedMeshRenderer>();

	private List<MeshRenderer> _mesh_renderers = new List<MeshRenderer>();

	private void Start()
	{
		GameObject[] array = targetMeshes;
		foreach (GameObject obj in array)
		{
			SkinnedMeshRenderer component = obj.GetComponent<SkinnedMeshRenderer>();
			if (component != null)
			{
				_skin_renderers.Add(component);
			}
			MeshRenderer component2 = obj.GetComponent<MeshRenderer>();
			if (component2 != null)
			{
				_mesh_renderers.Add(component2);
			}
		}
		_matBlock = new MaterialPropertyBlock();
	}

	public void AnimateMaterialLinearDecay()
	{
		StartCoroutine(LinearDecay());
	}

	private IEnumerator LinearDecay()
	{
		float _elapsed = 0f;
		while (duration > _elapsed)
		{
			_value = Mathf.Lerp(startValue, endValue, _elapsed / duration);
			_elapsed += Time.deltaTime;
			if (_mesh_renderers.Count > 0)
			{
				foreach (MeshRenderer mesh_renderer in _mesh_renderers)
				{
					mesh_renderer.GetPropertyBlock(_matBlock);
					_matBlock.SetFloat(parameter, _value);
					mesh_renderer.SetPropertyBlock(_matBlock);
				}
			}
			if (_skin_renderers.Count > 0)
			{
				foreach (SkinnedMeshRenderer skin_renderer in _skin_renderers)
				{
					skin_renderer.GetPropertyBlock(_matBlock);
					_matBlock.SetFloat(parameter, _value);
					skin_renderer.SetPropertyBlock(_matBlock);
				}
			}
			yield return null;
		}
		_value = endValue;
		if (_mesh_renderers.Count > 0)
		{
			foreach (MeshRenderer mesh_renderer2 in _mesh_renderers)
			{
				mesh_renderer2.GetPropertyBlock(_matBlock);
				_matBlock.SetFloat(parameter, _value);
				mesh_renderer2.SetPropertyBlock(_matBlock);
			}
		}
		if (_skin_renderers.Count <= 0)
		{
			yield break;
		}
		foreach (SkinnedMeshRenderer skin_renderer2 in _skin_renderers)
		{
			skin_renderer2.GetPropertyBlock(_matBlock);
			_matBlock.SetFloat(parameter, _value);
			skin_renderer2.SetPropertyBlock(_matBlock);
		}
	}
}
