using UnityEngine;

namespace Core.Meta.MainNavigation.Achievements;

[RequireComponent(typeof(Animator))]
public class AchievementGroupDisplayAnimationController : MonoBehaviour
{
	private static readonly int _readyToClaimParameter = Animator.StringToHash("ReadyToClaim");

	private Animator _animator;

	public bool HasReadyToClaimAchievements
	{
		get
		{
			return _animator.GetBool(_readyToClaimParameter);
		}
		set
		{
			_animator.SetBool(_readyToClaimParameter, value);
		}
	}

	private void Awake()
	{
		_animator = GetComponent<Animator>();
	}
}
