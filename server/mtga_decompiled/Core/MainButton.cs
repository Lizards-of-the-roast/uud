using UnityEngine;
using Wotc.Mtga.Loc;

public class MainButton : MonoBehaviour
{
	[SerializeField]
	private Localize _mainButtonLocalize;

	public void SetVisualState()
	{
		_mainButtonLocalize.SetText("MainNav/Landing/Play");
		GetComponent<CustomButton>().Interactable = true;
	}
}
