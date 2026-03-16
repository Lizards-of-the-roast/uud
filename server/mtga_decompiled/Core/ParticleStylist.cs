using System;
using System.Collections.Generic;
using UnityEngine;

public class ParticleStylist : MonoBehaviour
{
	public enum ColorBlendOptions
	{
		Default = 0,
		Multiply = 1,
		Overlay = 2,
		Add = 3,
		ColorDodge = 4,
		Override = 20,
		Custom = 21
	}

	[Serializable]
	public class ParticleColorBlendSetting
	{
		public string name;

		public ColorBlendOptions startColorMode;

		public ParticleSystem.MinMaxGradient startColorResult;

		public bool overTimeUsed;

		public ColorBlendOptions overTimeMode;

		public ParticleSystem.MinMaxGradient overTimeColorResult;

		public Material materialOverride;

		public void Colorize(ParticleSystem system, ColorBlendOptions defaultMode)
		{
			ParticleSystem.MainModule main = system.main;
			if (startColorMode != ColorBlendOptions.Default || defaultMode != ColorBlendOptions.Default)
			{
				main.startColor = startColorResult;
			}
			if ((overTimeUsed && startColorMode != ColorBlendOptions.Default) || defaultMode != ColorBlendOptions.Default)
			{
				ParticleSystem.ColorOverLifetimeModule colorOverLifetime = system.colorOverLifetime;
				colorOverLifetime.color = overTimeColorResult;
			}
			if ((bool)materialOverride)
			{
				system.GetComponent<ParticleSystemRenderer>().material = materialOverride;
			}
		}

		public void Preview(ParticleSystem.MinMaxGradient blendColor, ParticleSystem.MinMaxGradient startColorOriginal, ParticleSystem.MinMaxGradient overTimeColorOriginal, ColorBlendOptions mode)
		{
			Func<Color, Color, Color> func = ReturnMode(startColorMode, mode);
			if (func != null)
			{
				startColorResult = Blend(startColorOriginal, blendColor, func);
			}
			func = ReturnMode(startColorMode, mode);
			if (func != null && overTimeUsed)
			{
				overTimeColorResult = Blend(overTimeColorOriginal, blendColor, ReturnMode(overTimeMode, mode));
			}
		}

		private ParticleSystem.MinMaxGradient Blend(ParticleSystem.MinMaxGradient target, ParticleSystem.MinMaxGradient source, Func<Color, Color, Color> mode)
		{
			ParticleSystem.MinMaxGradient result = CloneMinMaxGradient(target);
			switch (target.mode)
			{
			case ParticleSystemGradientMode.Color:
				result.color = mode(result.color, source.Evaluate(0f));
				break;
			case ParticleSystemGradientMode.TwoColors:
				result.colorMin = mode(result.colorMin, source.Evaluate(0f, 0f));
				result.colorMax = mode(result.colorMax, source.Evaluate(0f, 1f));
				break;
			case ParticleSystemGradientMode.Gradient:
			case ParticleSystemGradientMode.RandomColor:
				result.gradient = ProcessGradient(result.gradient, source, 0f, mode);
				break;
			case ParticleSystemGradientMode.TwoGradients:
				ProcessGradient(result.gradientMin, source, 0f, mode);
				ProcessGradient(result.gradientMax, source, 1f, mode);
				break;
			}
			return result;
		}

		private static Gradient ProcessGradient(Gradient a, ParticleSystem.MinMaxGradient b, float select, Func<Color, Color, Color> mode)
		{
			GradientColorKey[] array = (GradientColorKey[])a.colorKeys.Clone();
			GradientAlphaKey[] array2 = (GradientAlphaKey[])a.alphaKeys.Clone();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].color = mode(array[i].color, b.Evaluate(array[i].time, select));
			}
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j].alpha *= b.Evaluate(array2[j].time, select).a;
			}
			a.alphaKeys = array2;
			a.colorKeys = array;
			return a;
		}

		public static Func<Color, Color, Color> ReturnMode(ColorBlendOptions mode, ColorBlendOptions defaultMode)
		{
			if (defaultMode == ColorBlendOptions.Default && mode == ColorBlendOptions.Default)
			{
				return TakeA;
			}
			return mode switch
			{
				ColorBlendOptions.Default => ReturnMode(defaultMode, defaultMode), 
				ColorBlendOptions.Multiply => Multiply, 
				ColorBlendOptions.Overlay => Overlay, 
				ColorBlendOptions.Add => Add, 
				ColorBlendOptions.ColorDodge => ColorDodge, 
				ColorBlendOptions.Override => TakeB, 
				_ => null, 
			};
		}

		public static ParticleSystem.MinMaxGradient CloneMinMaxGradient(ParticleSystem.MinMaxGradient target)
		{
			ParticleSystem.MinMaxGradient result = new ParticleSystem.MinMaxGradient
			{
				color = target.color,
				colorMax = target.colorMax,
				colorMin = target.colorMin
			};
			if (target.gradient != null)
			{
				result.gradient = new Gradient();
				result.gradient.colorKeys = (GradientColorKey[])target.gradient.colorKeys.Clone();
				result.gradient.alphaKeys = (GradientAlphaKey[])target.gradient.alphaKeys.Clone();
			}
			if (target.gradientMax != null)
			{
				result.gradientMax = new Gradient();
				result.gradientMax.colorKeys = (GradientColorKey[])target.gradientMax.colorKeys.Clone();
				result.gradientMax.alphaKeys = (GradientAlphaKey[])target.gradientMax.alphaKeys.Clone();
			}
			if (target.gradientMin != null)
			{
				result.gradientMin = new Gradient();
				result.gradientMin.colorKeys = (GradientColorKey[])target.gradientMin.colorKeys.Clone();
				result.gradientMin.alphaKeys = (GradientAlphaKey[])target.gradientMin.alphaKeys.Clone();
			}
			result.mode = target.mode;
			return result;
		}

		private static Color TakeA(Color a, Color b)
		{
			return a;
		}

		private static Color TakeB(Color a, Color b)
		{
			return b;
		}

		private static Color Multiply(Color a, Color b)
		{
			return a * b;
		}

		private static Color Add(Color a, Color b)
		{
			Color color = new Color
			{
				r = a.r + b.r,
				g = a.g + b.g,
				b = a.b + b.b,
				a = a.a * b.a
			};
			return a;
		}

		private static Color ColorDodge(Color a, Color b)
		{
			Color color = new Color
			{
				r = a.r / (1f - b.r),
				g = a.g / (1f - b.g),
				b = a.b / (1f - b.b),
				a = a.a * b.a
			};
			return a;
		}

		private static Color Overlay(Color target, Color blend)
		{
			return new Color
			{
				r = Overlay(target.r, blend.r),
				g = Overlay(target.g, blend.g),
				b = Overlay(target.b, blend.b),
				a = target.a * target.b
			};
		}

		private static float Overlay(float target, float blend)
		{
			if (target > 0.5f)
			{
				return target * 1f - (1f - 2f * (target - 0.5f)) * (1f - blend);
			}
			return 2f * target * blend;
		}
	}

	public GameObject prefab;

	public bool test;

	public ColorBlendOptions defaultColorBlend;

	public ParticleSystem.MinMaxGradient tint = new ParticleSystem.MinMaxGradient
	{
		color = Color.white
	};

	public List<ParticleColorBlendSetting> configuration = new List<ParticleColorBlendSetting>();

	private List<Transform> testTargets = new List<Transform>();

	private void Configure()
	{
		if (!prefab)
		{
			return;
		}
		ParticleSystem[] componentsInChildren = prefab.GetComponentsInChildren<ParticleSystem>();
		for (int num = configuration.Count - 1; num > -1; num--)
		{
			bool flag = true;
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].name == configuration[num].name)
				{
					flag = false;
				}
			}
			if (flag)
			{
				configuration.RemoveAt(num);
			}
		}
		ParticleSystem[] array = componentsInChildren;
		foreach (ParticleSystem s in array)
		{
			ParticleColorBlendSetting particleColorBlendSetting = configuration.Find((ParticleColorBlendSetting x) => x.name == s.name);
			if (particleColorBlendSetting == null)
			{
				particleColorBlendSetting = new ParticleColorBlendSetting();
				particleColorBlendSetting.name = s.name;
				configuration.Add(particleColorBlendSetting);
			}
			particleColorBlendSetting.overTimeUsed = s.colorOverLifetime.enabled;
			particleColorBlendSetting.Preview(tint, s.main.startColor, s.colorOverLifetime.color, defaultColorBlend);
		}
	}

	public void Apply(GameObject instance)
	{
		ParticleSystem[] componentsInChildren = instance.GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem s in componentsInChildren)
		{
			configuration.Find((ParticleColorBlendSetting x) => x.name == s.name)?.Colorize(s, defaultColorBlend);
		}
	}

	public void ClearTestTargets()
	{
		testTargets.Clear();
	}

	public void AddTestTarget(Transform t)
	{
		testTargets.Add(t);
	}

	private void OnValidate()
	{
	}
}
