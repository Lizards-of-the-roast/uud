using TMPro;
using UnityEngine;

public class BrowserSubtitle : MonoBehaviour
{
	[SerializeField]
	protected TextMeshProUGUI subTitle;

	private string _subTitleStr = string.Empty;

	public virtual void SetSubTitleText(string text)
	{
		SetText(subTitle, text);
	}

	protected void SetText(TextMeshProUGUI textMesh, string text)
	{
		if (textMesh != null)
		{
			textMesh.SetText(text);
		}
	}

	private void ClearText()
	{
		SetText(subTitle, string.Empty);
	}
}
