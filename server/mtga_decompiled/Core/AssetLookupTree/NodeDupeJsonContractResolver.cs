using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using Wizards.Mtga.Utils;

namespace AssetLookupTree;

public class NodeDupeJsonContractResolver : DefaultContractResolver
{
	protected readonly IReadOnlyDictionary<Type, JsonConverter> _supportedContracts;

	protected readonly StringCache _stringCache = new StringCache();

	public NodeDupeJsonContractResolver()
	{
		_supportedContracts = new Dictionary<Type, JsonConverter>
		{
			[typeof(Color)] = new ColorConverter(),
			[typeof(Vector2)] = new Vector2Converter(),
			[typeof(Vector3)] = new Vector3Converter(),
			[typeof(Vector4)] = new Vector4Converter(),
			[typeof(AltAssetReference)] = new AltAssetReferenceConverter(_stringCache),
			[typeof(SpaceData)] = new SpaceDataConverter(),
			[typeof(OffsetData)] = new OffsetDataConverter(),
			[typeof(VfxPrefabData)] = new VfxPrefabDataConverter(),
			[typeof(VfxData)] = new VfxDataConverter(_stringCache),
			[typeof(AudioEvent)] = new AudioEventConverter(_stringCache),
			[typeof(SfxData)] = new SfxDataConverter()
		};
	}

	protected override JsonContract CreateContract(Type objectType)
	{
		JsonContract jsonContract = base.CreateContract(objectType);
		if (_supportedContracts.TryGetValue(objectType, out var value))
		{
			jsonContract.Converter = value;
		}
		return jsonContract;
	}
}
