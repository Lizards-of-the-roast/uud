using UnityEngine;
using UnityEngine.UI;

public class ScrollFadeTest : MonoBehaviour
{
	private void Start()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			base.transform.GetChild(i).GetComponentInChildren<Image>().color = new Color(Random.Range(0.4f, 1f), Random.Range(0.4f, 1f), Random.Range(0.4f, 1f), 1f);
		}
	}
}
