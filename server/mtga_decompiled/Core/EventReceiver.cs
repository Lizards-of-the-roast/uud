using UnityEngine;

public class EventReceiver : MonoBehaviour
{
	[SerializeField]
	private GameObject Cardback;

	public void ResetCardback()
	{
		Cardback.GetComponent<CDCPart_ControllerAnimatedCardback>().ResetCardback();
	}

	private void Start()
	{
	}
}
