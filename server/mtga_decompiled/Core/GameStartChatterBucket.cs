using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct GameStartChatterBucket
{
	[SerializeField]
	public uint minimumCardsInHand;

	[SerializeField]
	public List<ChatterPair> stringAudioPairs;
}
