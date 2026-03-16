using System;
using System.Collections.Generic;
using Core.Shared.Code.Utilities;
using GreClient.CardData;
using GreClient.Rules;
using TMPro;
using UnityEngine;
using Wotc.Mtga.Cards.ArtCrops;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.Cards;

public class CDCMaterialFiller : MonoBehaviour
{
	[SerializeField]
	private bool prepoulateMaterials = true;

	[SerializeField]
	private string _primaryArtSuffix = string.Empty;

	[SerializeField]
	private string _secondaryArtSuffix = string.Empty;

	[SerializeField]
	[ArtCropFormat]
	private string _cropFormatName = string.Empty;

	[SerializeField]
	private bool _canDissolve = true;

	[SerializeField]
	private bool _canMaterialSwap = true;

	[SerializeField]
	private List<RendererMaterialPairs> _renderersAndMaterials = new List<RendererMaterialPairs>();

	[SerializeField]
	private Vector2 _artOffset = Vector2.zero;

	private Dictionary<AltReferencedMaterialAndBlock, MaterialReplacementData> _replacementData = new Dictionary<AltReferencedMaterialAndBlock, MaterialReplacementData>();

	private CardMaterialBuilder _cardMaterialBuilder;

	private ICardDatabaseAdapter _cardDatabase = NullCardDatabaseAdapter.Default;

	private bool _hasBeenInit;

	public void Init(CardMaterialBuilder cardMaterialBuilder, ICardDatabaseAdapter cardDatabase)
	{
		_cardMaterialBuilder = cardMaterialBuilder;
		_cardDatabase = cardDatabase ?? NullCardDatabaseAdapter.Default;
		if (!_hasBeenInit)
		{
			SetupMaterials();
			_hasBeenInit = true;
		}
	}

	private void SetupMaterials()
	{
		foreach (RendererMaterialPairs renderersAndMaterial in _renderersAndMaterials)
		{
			Material[] sharedMaterials = renderersAndMaterial.Renderer.sharedMaterials;
			for (int i = 0; i < renderersAndMaterial.MaterialIndexes.Count; i++)
			{
				int num = renderersAndMaterial.MaterialIndexes[i];
				MaterialOverrideType overrideType = getMaterialOverride(i, renderersAndMaterial.MaterialIndexes, renderersAndMaterial.MaterialOverrideTypes);
				if (sharedMaterials.Length <= num)
				{
					continue;
				}
				Material material = sharedMaterials[num];
				if (!material)
				{
					continue;
				}
				if (HasMaterialDataForSimilarName(material.name, out var existingKey))
				{
					_replacementData[existingKey].Indices.Add(new MeshMaterialIndex
					{
						Renderer = renderersAndMaterial.Renderer,
						Index = num
					});
					continue;
				}
				Action<int> onMatAssignment = null;
				if (_cardMaterialBuilder != null)
				{
					onMatAssignment = _cardMaterialBuilder.DecrementReferenceCount;
				}
				AltReferencedMaterialAndBlock altReferencedMaterialAndBlock = new AltReferencedMaterialAndBlock(material);
				MeshMaterialIndex item = new MeshMaterialIndex
				{
					Renderer = renderersAndMaterial.Renderer,
					Index = num
				};
				MaterialReplacementData value = new MaterialReplacementData(altReferencedMaterialAndBlock, new List<MeshMaterialIndex> { item }, overrideType, onMatAssignment);
				_replacementData[altReferencedMaterialAndBlock] = value;
			}
		}
		static MaterialOverrideType getMaterialOverride(int idx, IReadOnlyList<int> indexes, IReadOnlyList<MaterialOverrideType> overrides)
		{
			if (idx >= indexes.Count || idx >= overrides.Count)
			{
				return MaterialOverrideType.Generic;
			}
			return overrides[idx];
		}
	}

	private bool HasMaterialDataForSimilarName(string materialName, out AltReferencedMaterialAndBlock existingKey)
	{
		existingKey = null;
		foreach (KeyValuePair<AltReferencedMaterialAndBlock, MaterialReplacementData> replacementDatum in _replacementData)
		{
			if (string.Equals(replacementDatum.Key.SharedMaterial.name, materialName, StringComparison.InvariantCultureIgnoreCase))
			{
				existingKey = replacementDatum.Key;
				return true;
			}
		}
		return false;
	}

	public void UpdateMaterials(ICardDataAdapter model, CardHolderType cardHolderType, Func<MtgGameState> getCurrentGameState, bool dimmed, bool mousedOver)
	{
		if (_cardMaterialBuilder != null && _canMaterialSwap)
		{
			CardViewUtilities.ReplaceMaterialData(_cardMaterialBuilder, _cardDatabase, _replacementData, model, cardHolderType, getCurrentGameState, dimmed, invertColor: false, _primaryArtSuffix, _secondaryArtSuffix, _cropFormatName, _artOffset, mousedOver);
			return;
		}
		foreach (KeyValuePair<AltReferencedMaterialAndBlock, MaterialReplacementData> replacementDatum in _replacementData)
		{
			replacementDatum.Value.UpdateOverride(replacementDatum.Key);
		}
	}

	public void GenerateDissolveMaterial(ICardDataAdapter model, CardHolderType cardHolderType, Func<MtgGameState> getCurrentGameState, string rampTexturePath, string noiseTexturePath, HashSet<MaterialReplacementData> dissolveMaterials)
	{
		if (!_canDissolve)
		{
			return;
		}
		foreach (KeyValuePair<AltReferencedMaterialAndBlock, MaterialReplacementData> replacementDatum in _replacementData)
		{
			MaterialReplacementData value = replacementDatum.Value;
			AltReferencedMaterialAndBlock altReferencedMaterialAndBlock = null;
			if (value.Instance != null)
			{
				altReferencedMaterialAndBlock = value.Instance;
			}
			else if (_canMaterialSwap)
			{
				altReferencedMaterialAndBlock = _cardMaterialBuilder.GetMaterialReplacement(_cardDatabase, value.Original, value.OriginalTextureName, value.OverrideType, model, cardHolderType, getCurrentGameState, dimmed: false, dissolve: true, invertColors: false, _primaryArtSuffix, _secondaryArtSuffix, _cropFormatName, _artOffset);
				value.UpdateInstance(altReferencedMaterialAndBlock);
			}
			else
			{
				AltReferencedMaterialAndBlock altReferencedMaterialAndBlock2 = value.Override ?? value.Original;
				if (altReferencedMaterialAndBlock2.HasUsedMaterialBlock())
				{
					MaterialPropertyBlock matblock = value.CopyFirstPropertyBlock();
					altReferencedMaterialAndBlock = altReferencedMaterialAndBlock2.CopyWithMatBlock(matblock);
				}
				else
				{
					altReferencedMaterialAndBlock = new AltReferencedMaterialAndBlock(altReferencedMaterialAndBlock2.SharedMaterial);
				}
				value.UpdateInstance(altReferencedMaterialAndBlock);
			}
			if (!string.IsNullOrEmpty(rampTexturePath) && altReferencedMaterialAndBlock.SharedMaterial.HasProperty(ShaderPropertyIds.RampPropId))
			{
				altReferencedMaterialAndBlock.SetTexture("_Ramp", rampTexturePath);
			}
			if (!string.IsNullOrEmpty(noiseTexturePath) && altReferencedMaterialAndBlock.SharedMaterial.HasProperty(ShaderPropertyIds.NoisePropId))
			{
				altReferencedMaterialAndBlock.SetTexture("_Noise", noiseTexturePath);
			}
			dissolveMaterials.Add(value);
		}
	}

	public void Cleanup()
	{
		foreach (KeyValuePair<AltReferencedMaterialAndBlock, MaterialReplacementData> replacementDatum in _replacementData)
		{
			replacementDatum.Value.UpdateOverride(replacementDatum.Key);
			replacementDatum.Value.Reset();
		}
		_cardMaterialBuilder = null;
	}

	public void SetDestroyed(bool isDestroyed)
	{
		if (!isDestroyed)
		{
			EnableRenderers(enabled: true);
		}
	}

	public void EnableRenderers(bool enabled)
	{
		foreach (RendererMaterialPairs renderersAndMaterial in _renderersAndMaterials)
		{
			Renderer renderer = renderersAndMaterial.Renderer;
			if (renderer != null && renderer.enabled != enabled)
			{
				renderer.enabled = enabled;
			}
		}
	}

	public void CleanupDissolveMaterial()
	{
		if (!_canDissolve)
		{
			return;
		}
		foreach (KeyValuePair<AltReferencedMaterialAndBlock, MaterialReplacementData> replacementDatum in _replacementData)
		{
			replacementDatum.Value.Reset();
		}
	}

	public void SetDepthArtVectors(Vector2 finalOffset)
	{
		foreach (KeyValuePair<AltReferencedMaterialAndBlock, MaterialReplacementData> replacementDatum in _replacementData)
		{
			AltReferencedMaterialAndBlock altReferencedMaterialAndBlock = replacementDatum.Value.Instance ?? replacementDatum.Value.Override ?? replacementDatum.Value.Original;
			MaterialPropertyBlock materialPropertyBlock = altReferencedMaterialAndBlock?.MatBlock;
			if (materialPropertyBlock != null)
			{
				if (altReferencedMaterialAndBlock.SharedMaterial.HasProperty(ShaderPropertyIds.ViewDirOffsetPropId))
				{
					materialPropertyBlock.SetVector(ShaderPropertyIds.ViewDirOffsetPropId, finalOffset);
				}
				replacementDatum.Value.UpdatePropertyBlocks();
			}
		}
	}

	public bool PrepopulateRenderersAndMaterials()
	{
		if (!prepoulateMaterials)
		{
			return false;
		}
		GatherRenderers(base.gameObject, out var outRenderers);
		_renderersAndMaterials.Clear();
		foreach (Renderer item in outRenderers)
		{
			GameObject gameObject = item.gameObject;
			if (!(gameObject.gameObject.name == "Expansion Symbol") && !(gameObject.gameObject.name == "DamageIcon") && !(gameObject.gameObject.name == "Paintbrush"))
			{
				List<int> list = new List<int>();
				List<MaterialOverrideType> list2 = new List<MaterialOverrideType>();
				for (int i = 0; i < item.sharedMaterials.Length; i++)
				{
					list.Add(i);
					list2.Add(MaterialOverrideType.Generic);
				}
				_renderersAndMaterials.Add(new RendererMaterialPairs
				{
					Renderer = item,
					MaterialIndexes = list,
					MaterialOverrideTypes = list2
				});
			}
		}
		return _renderersAndMaterials.Count > 0;
	}

	public bool PopulateOverrideTypes()
	{
		if (_renderersAndMaterials.Exists((RendererMaterialPairs x) => x.MaterialIndexes.Count != x.MaterialOverrideTypes.Count))
		{
			bool result = false;
			for (int num = 0; num < _renderersAndMaterials.Count; num++)
			{
				RendererMaterialPairs rendererMaterialPairs = _renderersAndMaterials[num];
				if (rendererMaterialPairs.MaterialOverrideTypes.Count < rendererMaterialPairs.MaterialIndexes.Count)
				{
					while (rendererMaterialPairs.MaterialOverrideTypes.Count < rendererMaterialPairs.MaterialIndexes.Count)
					{
						rendererMaterialPairs.MaterialOverrideTypes.Add(_canMaterialSwap ? MaterialOverrideType.Generic : MaterialOverrideType.None);
						result = true;
					}
				}
				else if (rendererMaterialPairs.MaterialOverrideTypes.Count > rendererMaterialPairs.MaterialIndexes.Count)
				{
					while (rendererMaterialPairs.MaterialOverrideTypes.Count > rendererMaterialPairs.MaterialIndexes.Count)
					{
						rendererMaterialPairs.MaterialOverrideTypes.RemoveAt(rendererMaterialPairs.MaterialOverrideTypes.Count - 1);
						result = true;
					}
				}
			}
			return result;
		}
		return false;
	}

	public bool SetCardSleeveOverrides()
	{
		if (!_canMaterialSwap)
		{
			return false;
		}
		bool result = false;
		if (PopulateOverrideTypes())
		{
			Debug.Log("Populated Override Types");
			result = true;
		}
		foreach (RendererMaterialPairs renderersAndMaterial in _renderersAndMaterials)
		{
			Material[] sharedMaterials = renderersAndMaterial.Renderer.sharedMaterials;
			for (int i = 0; i < renderersAndMaterial.MaterialIndexes.Count; i++)
			{
				if (renderersAndMaterial.MaterialOverrideTypes[i] == MaterialOverrideType.CardSleeve)
				{
					continue;
				}
				int num = renderersAndMaterial.MaterialIndexes[i];
				if (sharedMaterials.Length <= num)
				{
					continue;
				}
				Material material = sharedMaterials[num];
				if (!(material == null))
				{
					string text = material.name;
					if (!string.IsNullOrEmpty(text) && !(text != "Mat_CardBack"))
					{
						renderersAndMaterial.MaterialOverrideTypes[i] = MaterialOverrideType.CardSleeve;
						result = true;
					}
				}
			}
		}
		return result;
	}

	private void GatherRenderers(GameObject root, out HashSet<Renderer> outRenderers)
	{
		HashSet<Renderer> renderers = new HashSet<Renderer>();
		GatherFromObjectAndChildren(root);
		outRenderers = renderers;
		void GatherFromObjectAndChildren(GameObject gameObject)
		{
			if (!gameObject.GetComponent<TMP_Text>() && !gameObject.GetComponent<TMP_SubMesh>() && !gameObject.GetComponent<ParticleSystem>())
			{
				Renderer component = gameObject.GetComponent<Renderer>();
				if ((bool)component)
				{
					renderers.Add(component);
				}
			}
			Transform transform = gameObject.transform;
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform child = transform.GetChild(i);
				if (!child.GetComponent<CDCPart>())
				{
					GatherFromObjectAndChildren(child.gameObject);
				}
			}
		}
	}
}
