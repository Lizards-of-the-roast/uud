using UnityEngine;

public class AnimationFramerateLimiter : MonoBehaviour
{
	[SerializeField]
	[Range(0f, 60f)]
	private int FPS = 8;

	private float _time;

	private Animator animator;

	private void Start()
	{
		if (animator == null)
		{
			animator = GetComponent<Animator>();
		}
	}

	private void Update()
	{
		animator.speed = 0f;
		_time += Time.deltaTime;
		float num = 1f / (float)FPS;
		if (_time > num)
		{
			_time -= num;
			animator.speed = num / Time.deltaTime;
		}
	}

	private void OnDisable()
	{
		animator.speed = 1f;
	}
}
