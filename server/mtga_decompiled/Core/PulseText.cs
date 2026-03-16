using UnityEngine;
using UnityEngine.UI;

public class PulseText : MonoBehaviour
{
	public Text TargetText;

	private void Update()
	{
		Color color = TargetText.color;
		color.a = Mathf.Sin(Time.time * 3f) * 0.5f + 0.5f;
		TargetText.color = color;
	}
}
