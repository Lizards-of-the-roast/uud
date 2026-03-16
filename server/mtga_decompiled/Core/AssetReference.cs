using System;
using System.Collections.Generic;

[Serializable]
public class AssetReference
{
	public static List<AssetReference> ReferencesToDeDupe = new List<AssetReference>();

	public string RelativePath;

	public AssetReference()
	{
		ReferencesToDeDupe.Add(this);
	}
}
