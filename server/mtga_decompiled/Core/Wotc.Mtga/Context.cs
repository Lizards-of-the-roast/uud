using System;
using System.Collections.Generic;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga;

public class Context : IContext
{
	private readonly IReadOnlyDictionary<Type, object> _referenceMap;

	private readonly IContext _parent;

	public Context(IReadOnlyDictionary<Type, object> referenceMap, IContext parent = null)
	{
		_referenceMap = new Dictionary<Type, object>(referenceMap ?? DictionaryExtensions.Empty<Type, object>());
		_parent = parent ?? NullContext.Default;
	}

	public T Get<T>()
	{
		if (!_referenceMap.TryGetValue(typeof(T), out var value))
		{
			return _parent.Get<T>();
		}
		return (T)value;
	}
}
