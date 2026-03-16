using UnityEngine;

namespace Core.Meta.MainNavigation.Achievements.Scripts;

[RequireComponent(typeof(Animator))]
public class AchievementScreenAnimationController : MonoBehaviour
{
	private static readonly int _hubActiveBoolParameter = Animator.StringToHash("HubActive");

	private Animator _animator;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
	}

	public void SetHubActive(bool active)
	{
		_animator.SetBool(_hubActiveBoolParameter, active);
	}
}
