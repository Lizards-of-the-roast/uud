using UnityEngine;

public class InspectorUnityEventRangeAttribute : PropertyAttribute
{
	public string minValueStructName;

	public string maxValueStructName;

	public InspectorUnityEventRangeAttribute(string _minValueStruct, string _maxValueStruct)
	{
		minValueStructName = _minValueStruct;
		maxValueStructName = _maxValueStruct;
	}

	public InspectorUnityEventRangeAttribute()
	{
		minValueStructName = "";
		maxValueStructName = "";
	}
}
