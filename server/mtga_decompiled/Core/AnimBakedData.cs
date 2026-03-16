using UnityEngine;

public struct AnimBakedData
{
	public string name;

	public float animLen;

	public byte[] rawAnimMap;

	public int animMapWidth;

	public int animMapHeight;

	public AnimBakedData(string name, float animLen, Texture2D animMap)
	{
		this.name = name;
		this.animLen = animLen;
		animMapHeight = animMap.height;
		animMapWidth = animMap.width;
		rawAnimMap = animMap.GetRawTextureData();
	}
}
