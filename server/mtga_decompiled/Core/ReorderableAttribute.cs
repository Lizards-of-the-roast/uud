using UnityEngine;

public class ReorderableAttribute : PropertyAttribute
{
	public bool draggable = true;

	public bool displayAddRemoveButtons = true;

	public bool displayElementLabels = true;

	public bool displaySelectionDetail;

	public string displayProperties = "";

	private string[] _displayProperties;

	private float[] _displayRatios;

	public string[] GetDisplayProperties()
	{
		if (_displayProperties == null)
		{
			if (string.IsNullOrEmpty(displayProperties))
			{
				_displayProperties = new string[0];
			}
			else
			{
				displayProperties = displayProperties.Replace(" ", "");
				_displayProperties = displayProperties.Split(',');
			}
			int num = _displayProperties.Length;
			_displayRatios = new float[_displayProperties.Length];
			float num2 = 0f;
			for (int i = 0; i < num; i++)
			{
				string[] array = _displayProperties[i].Split(':');
				_displayProperties[i] = array[0];
				_displayRatios[i] = ((array.Length > 1) ? float.Parse(array[1]) : (1f / (float)num));
				if (_displayRatios[i] > 2f)
				{
					_displayRatios[i] *= 0.01f;
				}
				num2 += _displayRatios[i];
			}
			num2 = 1f / num2;
			for (int j = 0; j < num; j++)
			{
				_displayRatios[j] *= num2;
			}
		}
		return _displayProperties;
	}

	public float GetDisplayRatio(int index)
	{
		if (_displayProperties == null)
		{
			GetDisplayProperties();
		}
		if ((_displayRatios != null) & (index < _displayRatios.Length))
		{
			return _displayRatios[index];
		}
		return 0f;
	}
}
