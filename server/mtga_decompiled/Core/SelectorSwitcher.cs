using System;
using System.Collections.Generic;
using UnityEngine;
using Wizards.Mtga.Platforms;

public class SelectorSwitcher : MonoBehaviour
{
	[Serializable]
	protected class SelectorVariants
	{
		public DeviceType runtimePlatform;

		public GameObject variant;
	}

	protected enum DeviceType
	{
		Desktop,
		Handheld_16x9,
		Handheld_4x3
	}

	protected DeviceType GetDeviceType()
	{
		if (PlatformUtils.GetCurrentDeviceType() == UnityEngine.DeviceType.Handheld)
		{
			return DeviceType.Handheld_16x9;
		}
		return DeviceType.Desktop;
	}

	protected GameObject InstantiateVariant(List<SelectorVariants> variants)
	{
		return UnityEngine.Object.Instantiate(variants.Find((SelectorVariants x) => x.runtimePlatform == GetDeviceType()).variant, base.transform);
	}
}
