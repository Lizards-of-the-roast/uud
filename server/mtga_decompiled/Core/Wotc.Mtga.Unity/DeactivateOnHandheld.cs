using UnityEngine;
using Wizards.Mtga.Platforms;

namespace Wotc.Mtga.Unity;

public class DeactivateOnHandheld : MonoBehaviour
{
	private void OnEnable()
	{
		if (PlatformUtils.IsHandheld())
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
