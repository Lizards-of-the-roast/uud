using UnityEngine;

public class TimedAnimationCycler : MonoBehaviour
{
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private float maxTime;

	[SerializeField]
	private float minTime;

	[SerializeField]
	private string cycleName;

	[SerializeField]
	private string[] cycledRotateEvents;

	private float timer;

	private float targetTime;

	private int counter;

	public void Start()
	{
		targetTime = Random.Range(minTime, maxTime);
	}

	public void Update()
	{
		timer += Time.deltaTime;
		if (timer >= targetTime)
		{
			Cycle();
			targetTime = Random.Range(minTime, maxTime);
			timer = 0f;
		}
	}

	public void Cycle()
	{
		counter %= cycledRotateEvents.Length;
		animator.SetTrigger(cycledRotateEvents[counter]);
		counter++;
	}
}
