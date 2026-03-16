using System.Collections.Generic;
using UnityEngine;

public class AnimationToTexture
{
	private AnimData? animData;

	private List<Vector3> vertices = new List<Vector3>();

	private Mesh bakedMesh;

	private List<AnimBakedData> bakedDataList = new List<AnimBakedData>();

	public void SetAnimData(GameObject go)
	{
		if (go == null)
		{
			Debug.LogError("Gameobject is null; please choose a mesh object to pull animations from.");
			return;
		}
		Animation component = go.GetComponent<Animation>();
		SkinnedMeshRenderer componentInChildren = go.GetComponentInChildren<SkinnedMeshRenderer>();
		if (component == null)
		{
			Debug.LogError("No valid animations associated with this gameobject; this process only works with legacy animation.");
			return;
		}
		if (componentInChildren == null)
		{
			Debug.LogError("No skinned mesh renderer associated with this gameobject; this process only works with animated skinned meshes.");
			return;
		}
		bakedMesh = new Mesh();
		animData = new AnimData(component, componentInChildren, go.name);
	}

	public List<AnimBakedData> Bake()
	{
		if (!animData.HasValue)
		{
			Debug.LogError("Errors baking animations to textures.");
			return bakedDataList;
		}
		for (int i = 0; i < animData.Value.animClips.Count; i++)
		{
			BakePerAnimClip(animData.Value.animClips[i]);
		}
		return bakedDataList;
	}

	private void BakePerAnimClip(AnimationState curAnim)
	{
		int num = 0;
		float num2 = 0f;
		float num3 = 0f;
		num = Mathf.ClosestPowerOfTwo((int)(curAnim.clip.frameRate * curAnim.length));
		num3 = curAnim.length / (float)num;
		Texture2D texture2D = new Texture2D(animData.Value.mapWidth, num, TextureFormat.RGBAHalf, mipChain: false);
		texture2D.name = $"{animData.Value.name}_{curAnim.name}";
		animData.Value.AnimationPlay(curAnim.name);
		for (int i = 0; i < num; i++)
		{
			curAnim.time = num2;
			animData.Value.SampleAnimAndBakeMesh(ref bakedMesh);
			for (int j = 0; j < bakedMesh.vertexCount; j++)
			{
				Vector3 vector = bakedMesh.vertices[j];
				texture2D.SetPixel(j, i, new Color(vector.x, vector.y, vector.z));
			}
			num2 += num3;
		}
		texture2D.Apply();
		bakedDataList.Add(new AnimBakedData(texture2D.name, curAnim.clip.length, texture2D));
	}
}
