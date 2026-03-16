using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class InspectorDoubleButtonAttribute : PropertyAttribute
{
	public static float kDefaultButtonWidth = 100f;

	public readonly string MethodName;

	public readonly string MethodName2;

	public readonly string iconForButton1 = "";

	public readonly string buttonText1 = "";

	public readonly string iconForButton2 = "";

	public readonly string buttonText2 = "";

	public readonly string pathToIcons = "";

	private float _buttonWidth = kDefaultButtonWidth;

	public float ButtonWidth
	{
		get
		{
			return _buttonWidth;
		}
		set
		{
			_buttonWidth = value;
		}
	}

	public InspectorDoubleButtonAttribute(string MethodName)
	{
		this.MethodName = MethodName;
	}

	public InspectorDoubleButtonAttribute(string MethodName, string MethodName2, string buttonText1 = "", string pathforMethod1Icon = "", string pathToIcons = "Assets/Resources/", string buttonText2 = "", string pathforMethod2Icon = "")
	{
		this.pathToIcons = pathToIcons;
		this.MethodName = MethodName;
		iconForButton1 = pathforMethod1Icon;
		this.buttonText1 = buttonText1;
		this.MethodName2 = MethodName2;
		iconForButton2 = pathforMethod2Icon;
		this.buttonText2 = buttonText2;
	}
}
