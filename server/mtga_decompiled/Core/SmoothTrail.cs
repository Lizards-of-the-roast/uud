using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class SmoothTrail : MonoBehaviour
{
	public float stepTime = 0.001f;

	public float emitLength;

	private ParticleSystem ps;

	private float steps;

	private int loops;

	private float emitTime;

	private void Awake()
	{
		ps = base.gameObject.GetComponent<ParticleSystem>();
	}

	private void OnEnable()
	{
		emitTime = 0f;
		ps.Simulate(0f, withChildren: false, restart: true, fixedTimeStep: false);
	}

	private void Update()
	{
		if (!(emitTime > emitLength))
		{
			steps = Time.deltaTime / stepTime;
			loops = (int)steps;
			for (int i = 0; i < loops; i++)
			{
				ps.Simulate(stepTime, withChildren: false, restart: false, fixedTimeStep: false);
			}
			emitTime += Time.deltaTime;
		}
	}
}
