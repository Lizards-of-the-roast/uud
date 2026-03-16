using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct EmoteReplyChatterBucket
{
	[SerializeField]
	public string emoteToReplyTo;

	[SerializeField]
	public List<ChatterPair> stringAudioPairs;
}
