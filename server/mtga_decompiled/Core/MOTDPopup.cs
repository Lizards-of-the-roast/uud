using TMPro;
using UnityEngine;

public class MOTDPopup : PopupBase
{
	[Header("Assets")]
	[SerializeField]
	private TextMeshProUGUI _titleText;

	[SerializeField]
	private TextMeshProUGUI _bodyText;

	public override void OnEnter()
	{
		Hide();
	}

	public override void OnEscape()
	{
		Hide();
	}

	public void Activate(string title, string message)
	{
		_titleText.text = title;
		_bodyText.text = message;
		base.Activate(activate: true);
	}
}
