using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class RigidbodyWind : MonoBehaviour
{
	private static readonly HashSet<RigidbodyWind> _allRigidbodyWinds = new HashSet<RigidbodyWind>();

	private ParticleSystem ps;

	private ParticleSystem.Particle[] particles;

	private Rigidbody myRigidbody;

	public static void ToggleAll(bool enabled)
	{
		foreach (RigidbodyWind allRigidbodyWind in _allRigidbodyWinds)
		{
			if (allRigidbodyWind != null)
			{
				allRigidbodyWind.enabled = enabled;
			}
		}
	}

	private void Start()
	{
		_allRigidbodyWinds.Add(this);
		ps = base.gameObject.GetComponent<ParticleSystem>();
		ParticleSystem.MainModule main = ps.main;
		main.startLifetime = 100f;
		main.startSpeed = 0f;
		main.simulationSpace = ParticleSystemSimulationSpace.World;
		main.maxParticles = 1;
		ParticleSystem.EmissionModule emission = ps.emission;
		emission.rateOverTime = 0f;
		base.gameObject.GetComponent<ParticleSystemRenderer>().enabled = false;
		ParticleSystem.ExternalForcesModule externalForces = ps.externalForces;
		externalForces.enabled = true;
		particles = new ParticleSystem.Particle[1];
		ps.Emit(1);
		ps.GetParticles(particles, 1, 0);
		particles[0].position = Vector3.zero;
		ps.SetParticles(particles, 1);
		myRigidbody = base.gameObject.GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		ps.GetParticles(particles, 1, 0);
		myRigidbody.velocity += particles[0].velocity;
		particles[0].velocity = Vector3.zero;
		particles[0].position = myRigidbody.position;
		particles[0].remainingLifetime = 100f;
		ps.SetParticles(particles, 1, 0);
	}

	private void OnDestroy()
	{
		_allRigidbodyWinds.Remove(this);
	}
}
