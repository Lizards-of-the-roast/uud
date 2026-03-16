using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Wotc.Mtga.Extensions;

namespace AssetLookupTree;

[Serializable]
public class VfxPrefabData
{
	public float StartTime = 1f;

	public float CleanupAfterTime = 4f;

	public bool SkipSelfCleanup;

	public List<AltAssetReference<GameObject>> AllPrefabs = new List<AltAssetReference<GameObject>>(1);

	[JsonIgnore]
	public string RandomPrefabPath => AllPrefabs.SelectRandom()?.RelativePath;
}
