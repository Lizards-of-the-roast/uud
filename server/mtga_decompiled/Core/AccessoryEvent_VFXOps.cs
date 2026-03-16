using UnityEngine;

[CreateAssetMenu(fileName = "AE_VFX_New", menuName = "ScriptableObject/AccessoryEvents/VFXOps", order = 1)]
public class AccessoryEvent_VFXOps : AccessoryEventSO
{
	[SerializeField]
	private Color _startColor;

	public void Execute(ParticleSystem particleSystem)
	{
		ParticleSystem.MainModule main = particleSystem.main;
		main.startColor = _startColor;
	}

	public void ChangeVfxGroupColor(AccessoryEventVFXGroup vfxGroup)
	{
		for (int i = 0; i < vfxGroup.vfx.Length; i++)
		{
			ParticleSystem.MainModule main = vfxGroup.vfx[i].main;
			main.startColor = _startColor;
		}
	}
}
