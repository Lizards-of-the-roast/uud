using UnityEngine;

[ExecuteInEditMode]
public class PlayParticles : MonoBehaviour
{
	public ParticleSystem[] vfx = new ParticleSystem[1];

	public void PlayVFX(int Element)
	{
		vfx[Element].Play(withChildren: true);
	}

	public void StopVFX(int Element)
	{
		vfx[Element].Stop(withChildren: true);
	}
}
