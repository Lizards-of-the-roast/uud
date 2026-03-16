using System.Collections.Generic;
using UnityEngine;
using Wotc.Mtga.Extensions;

namespace EventPage.Components;

public class BoosterPacksComponent : EventComponent
{
	[SerializeField]
	private GameObject _draftBoosterPacks;

	[SerializeField]
	private GameObject _sealedBoosterPacks;

	private readonly List<Material> _trackedMaterials = new List<Material>();

	public void SetMaterials(List<uint> collationIds)
	{
		GameObject gameObject;
		if (collationIds.Count == 3)
		{
			gameObject = _draftBoosterPacks;
			_sealedBoosterPacks.gameObject.UpdateActive(active: false);
		}
		else
		{
			gameObject = _sealedBoosterPacks;
			_draftBoosterPacks.gameObject.UpdateActive(active: false);
		}
		gameObject.UpdateActive(active: true);
		FreeMaterials();
		Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
		int num = Mathf.Min(collationIds.Count, componentsInChildren.Length);
		for (int i = 0; i < num; i++)
		{
			uint num2 = collationIds[i];
			Material boosterMaterial = BoosterPayloadUtilities.GetBoosterMaterial(componentsInChildren[i].sharedMaterial, (int)num2, num2 < 300000, WrapperController.Instance.AssetLookupSystem);
			_trackedMaterials.Add(boosterMaterial);
			componentsInChildren[i].material = boosterMaterial;
		}
	}

	private void OnDestroy()
	{
		FreeMaterials();
	}

	private void FreeMaterials()
	{
		foreach (Material trackedMaterial in _trackedMaterials)
		{
			BoosterPayloadUtilities.FreeBoosterMaterial(trackedMaterial);
		}
		_trackedMaterials.Clear();
	}
}
