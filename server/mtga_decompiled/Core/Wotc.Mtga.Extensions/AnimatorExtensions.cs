using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wotc.Mtga.Extensions;

public static class AnimatorExtensions
{
	public static void SetTrigger(this Animator animator, string name, bool value)
	{
		if (value)
		{
			animator.SetTrigger(name);
		}
		else
		{
			animator.ResetTrigger(name);
		}
	}

	public static void SetTrigger(this Animator animator, int nameHash, bool value)
	{
		if (value)
		{
			animator.SetTrigger(nameHash);
		}
		else
		{
			animator.ResetTrigger(nameHash);
		}
	}

	public static void SetTriggerIfContains(this Animator animator, int nameHash)
	{
		if (animator != null && animator.ContainsParameter(nameHash))
		{
			animator.SetTrigger(nameHash);
		}
	}

	public static bool ContainsParameter(this Animator animator, int nameHash)
	{
		IEnumerable<AnimatorControllerParameter> parameters = animator.parameters;
		foreach (AnimatorControllerParameter item in parameters ?? Enumerable.Empty<AnimatorControllerParameter>())
		{
			if (item.nameHash == nameHash)
			{
				return true;
			}
		}
		return false;
	}
}
