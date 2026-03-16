using TMPro;
using UnityEngine;

public class RewardCompleteSet : MonoBehaviour
{
	[SerializeField]
	private TextMeshPro _titleText;

	public void Init(string setCode)
	{
		_titleText.text = setCode;
	}
}
