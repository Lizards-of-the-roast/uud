using System;
using System.Collections.Generic;
using AssetLookupTree.Evaluators;
using AssetLookupTree.Evaluators.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AssetLookupTree;

public class AltJsonContractResolver : NodeDupeJsonContractResolver
{
	private readonly IReadOnlyDictionary<Type, JsonConverter> _supportedBaseContracts;

	private readonly IReadOnlyDictionary<Type, JsonConverter> _supportedGenericBaseContracts;

	public AltJsonContractResolver()
	{
		_supportedBaseContracts = new Dictionary<Type, JsonConverter>
		{
			[typeof(AltAssetReference)] = _supportedContracts[typeof(AltAssetReference)],
			[typeof(Utility_Compound)] = new Utility_CompoundConverter(),
			[typeof(EvaluatorBase_String)] = new EvaluatorBase_StringConverter(_stringCache),
			[typeof(EvaluatorBase_Boolean)] = new EvaluatorBase_BooleanConverter(),
			[typeof(EvaluatorBase_Int)] = new EvaluatorBase_IntConverter(),
			[typeof(EvaluatorBase_IntToInt)] = new EvaluatorBase_IntToIntConverter(),
			[typeof(EvaluatorBase_DateTime)] = new EvaluatorBase_DateTimeConverter(),
			[typeof(EvaluatorBase_List<int>)] = new EvaluatorBase_ListIntConverter(),
			[typeof(EvaluatorBase_List<string>)] = new EvaluatorBase_ListStringConverter(_stringCache)
		};
		_supportedGenericBaseContracts = new Dictionary<Type, JsonConverter> { [typeof(EvaluatorBase_List<>)] = new EvaluatorBase_ListConverter() };
	}

	protected override JsonContract CreateContract(Type objectType)
	{
		JsonContract jsonContract = base.CreateContract(objectType);
		if (objectType.BaseType != null)
		{
			JsonConverter value2;
			if (_supportedBaseContracts.TryGetValue(objectType.BaseType, out var value))
			{
				jsonContract.Converter = value;
			}
			else if (objectType.BaseType.IsGenericType && _supportedGenericBaseContracts.TryGetValue(objectType.BaseType.GetGenericTypeDefinition(), out value2))
			{
				jsonContract.Converter = value2;
			}
		}
		return jsonContract;
	}
}
