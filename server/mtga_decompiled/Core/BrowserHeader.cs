using TMPro;
using UnityEngine;

public class BrowserHeader : MonoBehaviour
{
	[SerializeField]
	protected TextMeshProUGUI header;

	[SerializeField]
	protected TextMeshProUGUI subheader;

	public virtual void SetHeaderText(string text)
	{
		SetText(header, text);
	}

	public virtual void SetSubheaderText(string text)
	{
		SetText(subheader, text);
	}

	protected void SetText(TextMeshProUGUI textMesh, string text)
	{
		if (textMesh != null)
		{
			textMesh.SetText(text);
		}
	}
}
