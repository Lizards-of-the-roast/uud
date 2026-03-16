using UnityEngine;

namespace Core.Meta.MainNavigation.Achievements;

[RequireComponent(typeof(Animator))]
public class BladeSetAnimationController : MonoBehaviour
{
	public enum Status
	{
		New,
		Default,
		ReadyToClaim,
		Done
	}

	private Animator _animator;

	private static readonly int _selected = Animator.StringToHash("Active");

	private static readonly int _logoLoaded = Animator.StringToHash("LogoLoaded");

	private static readonly int _status = Animator.StringToHash("Status");

	private static readonly int _timer = Animator.StringToHash("Timer");

	private void Awake()
	{
		_animator = GetComponent<Animator>();
	}

	public void ToggleSelected()
	{
		_animator.SetBool(_selected, !_animator.GetBool(_selected));
	}

	public void SetSelected(bool active)
	{
		_animator.SetBool(_selected, active);
	}

	public void SetLogoLoaded(bool logoLoaded)
	{
		_animator.SetBool(_logoLoaded, logoLoaded);
	}

	public void SetBladeStatus(Status status)
	{
		_animator.SetInteger(_status, (int)status);
	}

	public void SetTimer(bool active)
	{
		_animator.SetInteger(_timer, active ? 1 : 0);
	}
}
