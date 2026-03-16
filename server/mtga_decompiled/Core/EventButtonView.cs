using TMPro;
using UnityEngine;

public class EventButtonView : MonoBehaviour
{
	public GameObject NewObject;

	public GameObject ActiveObject;

	public TMP_Text TitleLabel;

	private CustomButton _button;

	public CustomButton Button
	{
		get
		{
			if (_button == null)
			{
				_button = GetComponentInChildren<CustomButton>();
			}
			return _button;
		}
	}
}
