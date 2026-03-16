using UnityEngine;

public class ParticleCounter : MonoBehaviour
{
	public int CurrentlyAliveParticles;

	public int PeakParticleCount;

	public int ParticleSystemsEmitting;

	public int PeakEmittersCount;

	private ParticleSystem[] particleSystems;

	private ParticleSystem[] enabledParticleSystems;

	private int activeParticleSystems;

	private void Start()
	{
		particleSystems = GetComponentsInChildren<ParticleSystem>(includeInactive: true);
		enabledParticleSystems = GetComponentsInChildren<ParticleSystem>();
	}

	private void Update()
	{
		CurrentlyAliveParticles = GetAllAliveParticles();
		if (CurrentlyAliveParticles > PeakParticleCount)
		{
			PeakParticleCount = CurrentlyAliveParticles;
		}
		activeParticleSystems = 0;
		ParticleSystem[] array = particleSystems;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].isEmitting)
			{
				activeParticleSystems++;
			}
		}
		ParticleSystemsEmitting = activeParticleSystems;
		if (ParticleSystemsEmitting > PeakEmittersCount)
		{
			PeakEmittersCount = ParticleSystemsEmitting;
		}
	}

	private int GetAllAliveParticles()
	{
		int num = 0;
		ParticleSystem[] array = particleSystems;
		foreach (ParticleSystem system in array)
		{
			num += GetAliveParticles(system);
		}
		return num;
	}

	private int GetAliveParticles(ParticleSystem system)
	{
		ParticleSystem.Particle[] particles = new ParticleSystem.Particle[system.particleCount];
		return system.GetParticles(particles);
	}
}
