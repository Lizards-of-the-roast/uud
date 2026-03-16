using System.Collections.Generic;
using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

namespace EsportsCrowd;

public class CrowdPersonStatus
{
	public readonly Dictionary<CardColor, float> HypeThreshold = new Dictionary<CardColor, float>(6)
	{
		{
			CardColor.Colorless,
			0f
		},
		{
			CardColor.White,
			0f
		},
		{
			CardColor.Blue,
			0f
		},
		{
			CardColor.Black,
			0f
		},
		{
			CardColor.Red,
			0f
		},
		{
			CardColor.Green,
			0f
		}
	};

	public float GenericHypeThreshold = 1f;

	public float HypeRecoverySpeed = 1f;

	public float HypeAwarenessSpeed = 1f;

	public float OverflowHypeMultiplier = 1f;

	public float AbsorbedHypeMultiplier;

	public float HypeRange = 0.1f;

	public readonly Dictionary<CardColor, float> HypeValues = new Dictionary<CardColor, float>(6)
	{
		{
			CardColor.Colorless,
			0f
		},
		{
			CardColor.White,
			1f
		},
		{
			CardColor.Blue,
			1f
		},
		{
			CardColor.Black,
			1f
		},
		{
			CardColor.Red,
			1f
		},
		{
			CardColor.Green,
			1f
		}
	};

	public float GenericHype;

	public CardColor? CurrentAffinity;

	public void HandleEvent(ref HypeEvent hypeEvent, out bool affinityChanged, out bool cheerTriggered)
	{
		affinityChanged = false;
		cheerTriggered = false;
		float num = 0f;
		float num2 = 0f;
		if (!hypeEvent.Affinity.HasValue)
		{
			affinityChanged = false;
			float genericHype = GenericHype;
			float genericHypeThreshold = GenericHypeThreshold;
			float num3 = genericHype + hypeEvent.Value;
			if (num3 > genericHypeThreshold)
			{
				GenericHype = num3 % genericHypeThreshold;
				num = genericHypeThreshold - genericHype;
				num2 = num3 - genericHypeThreshold;
				cheerTriggered = true;
			}
			else
			{
				GenericHype = num3;
				num = hypeEvent.Value;
			}
		}
		else
		{
			CardColor value = hypeEvent.Affinity.Value;
			float num4 = HypeValues[value];
			float num5 = HypeThreshold[value];
			float num6 = num4 + hypeEvent.Value;
			if (num6 > num5)
			{
				if (!CurrentAffinity.HasValue || CurrentAffinity.Value != value)
				{
					CurrentAffinity = value;
					affinityChanged = true;
				}
				HypeValues[value] = num6 % num5;
				num = num5 - num4;
				num2 = num6 - num5;
				cheerTriggered = true;
				foreach (CardColor key in HypeThreshold.Keys)
				{
					if (key != value)
					{
						HypeValues[key] = Mathf.Max(0f, HypeValues[key] - num5);
					}
				}
			}
			else
			{
				HypeValues[value] = num6;
				num = hypeEvent.Value;
			}
		}
		num *= AbsorbedHypeMultiplier;
		num2 *= OverflowHypeMultiplier;
		hypeEvent.Value = num + num2;
	}
}
