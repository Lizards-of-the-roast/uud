using System;
using AssetLookupTree.Payloads.Prefab;
using Core.Code.AssetLookupTree.AssetLookup;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga;

public class TooltipSystemFactory
{
	public static TooltipSystem Create()
	{
		AssetLookupManager assetLookupManager = Pantry.Get<AssetLookupManager>();
		IClientLocProvider locMan = Pantry.Get<IClientLocProvider>();
		GameObject gameObject = AssetLoader.Instantiate(assetLookupManager.AssetLookupSystem.GetPrefabPath<TooltipSystemCanvasPrefab, GameObject>());
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		TooltipSystem componentInChildren = gameObject.GetComponentInChildren<TooltipSystem>();
		componentInChildren.Init(assetLookupManager.AssetLookupSystem, new NullBILogger(), locMan);
		TooltipTrigger.Inject(componentInChildren);
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		HostPlatform hostPlatform = currentEnvironment.HostPlatform;
		if ((uint)(hostPlatform - 1) <= 1u)
		{
			return componentInChildren;
		}
		throw new ArgumentOutOfRangeException($"Unrecognized Environment {currentEnvironment.HostPlatform}");
	}
}
