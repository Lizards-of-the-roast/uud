using System.Collections.Generic;
using UnityEngine;
using Wotc.Mtga.Client.Models.Mercantile;

namespace Core.Meta.Shared;

public class PrefabBoosterPopulator : PrefabPopulator
{
	[SerializeField]
	private List<int> _boosterCollationIds;

	[SerializeField]
	private Dictionary<StoreCardView, StoreCardData> _cardSleevesAndStyles;

	private List<Renderer> _boosterRenderers;

	private Material _originalBoosterMaterial;

	private readonly List<Material> _trackedMaterials = new List<Material>();

	public override void Populate()
	{
		StoreItemDisplay.GetBoosterPackRenderers(base.gameObject, ref _boosterRenderers, ref _originalBoosterMaterial);
		StoreItemDisplay.FreeMaterials(_trackedMaterials);
		StoreItemDisplay.ApplyBoosterMaterialsToPacks(_boosterRenderers, _boosterCollationIds, _originalBoosterMaterial, _trackedMaterials);
	}

	private void OnDestroy()
	{
		StoreItemDisplay.FreeMaterials(_trackedMaterials);
	}
}
