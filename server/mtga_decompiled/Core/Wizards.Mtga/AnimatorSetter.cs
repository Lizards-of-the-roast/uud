using System.Collections.Generic;
using UnityEngine;

namespace Wizards.Mtga;

public class AnimatorSetter : MonoBehaviour
{
	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private List<string> _boolParameterNames = new List<string>();

	[SerializeField]
	private List<string> _intParameterNames = new List<string>();

	[SerializeField]
	private List<string> _floatParameterNames = new List<string>();

	public void SetBool(bool value)
	{
		foreach (string boolParameterName in _boolParameterNames)
		{
			_animator.SetBool(boolParameterName, value);
		}
	}

	public void SetInt(int value)
	{
		foreach (string intParameterName in _intParameterNames)
		{
			_animator.SetInteger(intParameterName, value);
		}
	}

	public void SetFloat(float value)
	{
		foreach (string floatParameterName in _floatParameterNames)
		{
			_animator.SetFloat(floatParameterName, value);
		}
	}
}
