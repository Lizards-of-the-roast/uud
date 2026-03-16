using System;
using System.Collections.Generic;

namespace Core.Meta.MainNavigation;

public class WrapperCompass
{
	private readonly Dictionary<Type, WrapperCompassGuide> _compassGuides = new Dictionary<Type, WrapperCompassGuide>();

	public static WrapperCompass Create()
	{
		return new WrapperCompass();
	}

	public T GetGuide<T>() where T : WrapperCompassGuide
	{
		if (!_compassGuides.ContainsKey(typeof(T)))
		{
			return null;
		}
		T result = (T)_compassGuides[typeof(T)];
		_compassGuides.Remove(typeof(T));
		return result;
	}

	public void SetGuide(WrapperCompassGuide guide)
	{
		_compassGuides[guide.GetType()] = guide;
	}
}
