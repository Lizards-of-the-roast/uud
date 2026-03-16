using Spine.Unity;
using UnityEngine;

public class SpineReplacementWithMask : MonoBehaviour
{
	public Material originalMaterial;

	public Material replacementMaterialT;

	public SkeletonAnimation skeletonAnimation;

	private void Start()
	{
		if (originalMaterial == null)
		{
			originalMaterial = skeletonAnimation.SkeletonDataAsset.atlasAssets[0].PrimaryMaterial;
		}
		SetReplacementEnabled();
	}

	private void SetReplacementEnabled()
	{
		if (replacementMaterialT != null)
		{
			skeletonAnimation.CustomMaterialOverride[originalMaterial] = replacementMaterialT;
		}
		else
		{
			skeletonAnimation.CustomMaterialOverride.Remove(originalMaterial);
		}
	}
}
