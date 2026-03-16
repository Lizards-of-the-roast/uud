using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct AnimData
{
	public int vertexCount;

	public int mapWidth;

	public List<AnimationState> animClips;

	public string name;

	private Animation animation;

	private SkinnedMeshRenderer skin;

	public AnimData(Animation anim, SkinnedMeshRenderer smr, string goName)
	{
		vertexCount = smr.sharedMesh.vertexCount;
		mapWidth = Mathf.NextPowerOfTwo(vertexCount);
		animClips = new List<AnimationState>(anim.Cast<AnimationState>());
		animation = anim;
		skin = smr;
		name = goName;
	}

	public void AnimationPlay(string animName)
	{
		animation.Play(animName);
	}

	public void SampleAnimAndBakeMesh(ref Mesh m)
	{
		SampleAnim();
		BakeMesh(ref m);
	}

	private void SampleAnim()
	{
		if (animation == null)
		{
			Debug.LogError("animation is null");
		}
		else
		{
			animation.Sample();
		}
	}

	private void BakeMesh(ref Mesh m)
	{
		if (skin == null)
		{
			Debug.LogError("skin is null");
		}
		else
		{
			skin.BakeMesh(m);
		}
	}
}
