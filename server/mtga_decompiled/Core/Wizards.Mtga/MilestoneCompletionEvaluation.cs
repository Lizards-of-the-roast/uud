using UnityEngine;
using UnityEngine.Events;
using Wotc.Mtga.Events;

namespace Wizards.Mtga;

public class MilestoneCompletionEvaluation : MonoBehaviour
{
	[SerializeField]
	private CampaignGraphMilestones _milestoneToEvaluate;

	[SerializeField]
	private bool _evaluateOnEnable = true;

	[SerializeField]
	[Tooltip("Triggered when this component evaluates that the milestone is not completed.")]
	private UnityEvent _evaluateToNotcompleted;

	[SerializeField]
	[Tooltip("Triggered when this component evaluates that the milestone is completed.")]
	private UnityEvent _evaluateToCompleted;

	public CampaignGraphMilestones MilestoneToEvaluate
	{
		get
		{
			return _milestoneToEvaluate;
		}
		set
		{
			_milestoneToEvaluate = value;
		}
	}

	private void OnEnable()
	{
		if (_evaluateOnEnable)
		{
			Evaluate();
		}
	}

	public void Evaluate()
	{
		bool? flag = _milestoneToEvaluate.MilestoneCompleted();
		if (flag.HasValue && flag.Value)
		{
			_evaluateToCompleted.Invoke();
		}
		else
		{
			_evaluateToNotcompleted.Invoke();
		}
	}
}
