using System.Collections.Generic;
using UnityEngine;
using Wotc.Mtga.Extensions;

public class WrapperSkinSwapper : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> _skins;

	public void SwitchSkin(int index)
	{
		if (index >= 0 && index < _skins.Count)
		{
			ResetSkins();
			_skins[index].SetActive(value: true);
		}
		else
		{
			Debug.LogError($"Tried Switching to Invalid Skin on {base.gameObject.name}");
		}
	}

	private void ResetSkins()
	{
		foreach (GameObject skin in _skins)
		{
			skin.UpdateActive(active: false);
		}
	}
}
