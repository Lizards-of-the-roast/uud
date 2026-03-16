using UnityEngine;

namespace Wotc.Mtga.Cards.Parts;

public class CDCPart_SummoningSickness : CDCPart
{
	[SerializeField]
	private ParticleSystem _rootParticles;

	[SerializeField]
	private GameObject _nestedVfxObjects;

	protected override void HandleDestructionInternal()
	{
		base.HandleDestructionInternal();
		SetVisible(!_cachedDestroyed);
	}

	public void SetVisible(bool shouldBeVisible)
	{
		bool flag = !_cachedDestroyed && shouldBeVisible;
		if ((bool)_rootParticles)
		{
			if (flag)
			{
				_rootParticles.Play(withChildren: true);
			}
			else
			{
				_rootParticles.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
			}
		}
		if ((bool)_nestedVfxObjects)
		{
			_nestedVfxObjects.SetActive(flag);
		}
	}
}
