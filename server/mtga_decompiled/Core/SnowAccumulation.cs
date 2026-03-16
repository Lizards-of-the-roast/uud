using Core.Shared.Code.Utilities;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class SnowAccumulation : MonoBehaviour
{
	public float accumulationSpeed = 1f;

	private Renderer renderer;

	private float snowLevel;

	private const float maxSnowLevel = 2f;

	private void Start()
	{
		renderer = base.gameObject.GetComponent<Renderer>();
	}

	private void Update()
	{
		float b = snowLevel + accumulationSpeed * 0.01f * Time.deltaTime;
		snowLevel = Mathf.Min(2f, b);
		renderer.material.SetFloat(ShaderPropertyIds.SnowLevelPropId, snowLevel);
	}

	public void RemoveSnow()
	{
		snowLevel = 0f;
	}
}
