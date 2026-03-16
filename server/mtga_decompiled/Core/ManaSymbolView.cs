using UnityEngine;
using UnityEngine.UI;

public class ManaSymbolView : MonoBehaviour
{
	[SerializeField]
	private Image _symbolImage;

	public Image SymbolImage => _symbolImage;
}
