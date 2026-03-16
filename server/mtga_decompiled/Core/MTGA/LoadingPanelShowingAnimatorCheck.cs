using UnityEngine;

namespace MTGA;

public class LoadingPanelShowingAnimatorCheck : StateMachineBehaviour
{
	[SerializeField]
	private string _loadingScreenParamterName;

	private int _parameterNameHashValue;

	private Animator _animator;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_animator == null)
		{
			_animator = animator;
		}
		_parameterNameHashValue = Animator.StringToHash(_loadingScreenParamterName);
		SetParameter(LoadingPanelShowing.IsShowing);
		LoadingPanelShowing.IsShowingChanged += SetParameter;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		LoadingPanelShowing.IsShowingChanged -= SetParameter;
	}

	private void OnDestroy()
	{
		LoadingPanelShowing.IsShowingChanged -= SetParameter;
	}

	private void SetParameter(bool loadingPanelIsShowing)
	{
		_animator.SetBool(_parameterNameHashValue, loadingPanelIsShowing);
	}
}
