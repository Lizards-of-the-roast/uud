using System;
using Newtonsoft.Json.Serialization;

namespace AssetLookupTree.TreeLoading.DeserializationPatterns;

public class TreeSerializationBinder : ISerializationBinder
{
	public Type BindToType(string assemblyName, string typeName)
	{
		return GetType().Assembly.GetType(typeName);
	}

	public void BindToName(Type serializedType, out string assemblyName, out string typeName)
	{
		assemblyName = null;
		typeName = serializedType.FullName;
	}
}
