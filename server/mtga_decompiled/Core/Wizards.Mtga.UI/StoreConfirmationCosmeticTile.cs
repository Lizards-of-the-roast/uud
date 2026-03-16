using UnityEngine;
using Wotc.Mtga.Loc;

namespace Wizards.Mtga.UI;

public class StoreConfirmationCosmeticTile : MonoBehaviour
{
	[SerializeField]
	private Localize _itemText;

	private void OnValidate()
	{
	}

	public void SetText(MTGALocalizedString text)
	{
		if (_itemText != null && text != null)
		{
			_itemText.SetText(text);
		}
	}
}
