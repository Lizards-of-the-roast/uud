using UnityEngine;

public class CardBundleCardAnimation : MonoBehaviour
{
	public bool WaitForComplete { get; set; }

	public void Animation_InsertComplete()
	{
		GetComponentInParent<EPPDeckUpgradeController>().Animation_InsertDeck(this);
	}
}
