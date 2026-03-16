using TMPro;
using UnityEngine;

public class StackPrompt : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI PromptText;

	public void SetPromptText(string text)
	{
		PromptText.text = text;
	}
}
