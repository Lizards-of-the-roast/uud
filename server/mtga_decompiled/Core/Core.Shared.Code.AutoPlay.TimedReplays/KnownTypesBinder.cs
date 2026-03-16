using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;

namespace Core.Shared.Code.AutoPlay.TimedReplays;

public class KnownTypesBinder : ISerializationBinder
{
	public IList<Type> KnownTypes { get; set; }

	public Type BindToType(string assemblyName, string typeName)
	{
		return KnownTypes.SingleOrDefault((Type t) => t.Name == typeName);
	}

	public void BindToName(Type serializedType, out string assemblyName, out string typeName)
	{
		assemblyName = null;
		typeName = serializedType.Name;
	}
}
