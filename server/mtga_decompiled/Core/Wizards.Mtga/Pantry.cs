using System;
using System.Collections.Generic;
using SharedClientCore.SharedClientCore.Code;
using UnityEngine;

namespace Wizards.Mtga;

public class Pantry : Pantry<Pantry.Scope>
{
	public enum Scope
	{
		Static,
		Application,
		Environment,
		Wrapper,
		Deckbuilder
	}

	private static Pantry instance;

	private static EnvironmentDescription currentEnvironment;

	public static EnvironmentDescription CurrentEnvironment
	{
		get
		{
			return currentEnvironment;
		}
		set
		{
			if (currentEnvironment != value)
			{
				currentEnvironment = value;
				instance.ResetContainer(Scope.Environment);
			}
		}
	}

	private Pantry(Dictionary<Scope, Dictionary<Type, Func<object>>> constructorsByScope)
		: base(constructorsByScope)
	{
	}

	public static Pantry Create(Dictionary<Scope, Dictionary<Type, Func<object>>> constructorsByScope)
	{
		if (instance == null)
		{
			instance = new Pantry(constructorsByScope);
		}
		return instance;
	}

	public static void ResetScope(Scope scope)
	{
		instance.ResetContainer(scope);
	}

	public static void Restart()
	{
		foreach (Scope value in EnumHelper.GetValues(typeof(Scope)))
		{
			if (value != Scope.Static)
			{
				ResetScope(value);
			}
		}
	}

	public static void ResetAll()
	{
		Debug.Log("Resetting all pantry scopes.");
		foreach (Scope value in Enum.GetValues(typeof(Scope)))
		{
			instance.ResetContainer(value);
		}
	}

	public static T Get<T>() where T : class
	{
		return instance.GetServiceInstance<T>();
	}
}
