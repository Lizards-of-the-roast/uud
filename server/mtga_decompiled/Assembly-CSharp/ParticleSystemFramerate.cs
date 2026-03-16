using UnityEngine;

[ExecuteAlways]
public class ParticleSystemFramerate : MonoBehaviour
{
	public int fps;

	private ParticleSystem particle;

	private float timeElapsed;

	private float displayTime;

	public float SimulationSpeed = 0.1f;

	private void OnEnable()
	{
		particle = GetComponent<ParticleSystem>();
	}

	private void LateUpdate()
	{
		timeElapsed += Time.deltaTime;
		if (timeElapsed - displayTime > 1f / (float)fps)
		{
			displayTime = timeElapsed;
			particle.Simulate(SimulationSpeed, withChildren: true, restart: false, fixedTimeStep: false);
			particle.Pause();
		}
	}
}
