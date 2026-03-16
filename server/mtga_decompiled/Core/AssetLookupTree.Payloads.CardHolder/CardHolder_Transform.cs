using System;
using System.Collections.Generic;
using AssetLookupTree.Payloads.CardHolder.Converter;
using Newtonsoft.Json;

namespace AssetLookupTree.Payloads.CardHolder;

[JsonConverter(typeof(CardHolder_TransformConverter))]
public class CardHolder_Transform : IPayload
{
	public OffsetData OffsetData = new OffsetData();

	public IEnumerable<string> GetFilePaths()
	{
		return Array.Empty<string>();
	}
}
