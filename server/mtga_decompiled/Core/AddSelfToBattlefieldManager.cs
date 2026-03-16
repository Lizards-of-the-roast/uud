using UnityEngine;

public class AddSelfToBattlefieldManager : MonoBehaviour
{
	private Light _light;

	private Material[] _materialsAdded;

	private ParticleSystem _particleAdded;

	private PlayAudioBFTrigger _soundAdded;

	private void Start()
	{
		_light = base.gameObject.GetComponent<Light>();
		if (base.gameObject.GetComponent<Renderer>() != null)
		{
			_materialsAdded = base.gameObject.GetComponent<Renderer>()?.materials ?? new Material[0];
		}
		_particleAdded = base.gameObject.GetComponent<ParticleSystem>();
		_soundAdded = base.gameObject.GetComponent<PlayAudioBFTrigger>();
		if ((bool)_light)
		{
			BattlefieldManager.dimmableLights?.Add(_light);
			BattlefieldManager.dimmableLightDefaultIntensities?.Add(_light.intensity);
		}
		if ((bool)_particleAdded)
		{
			BattlefieldManager.windTriggeredParticles?.Add(_particleAdded);
		}
		if ((bool)_soundAdded)
		{
			BattlefieldManager.triggeredSounds?.Add(_soundAdded);
		}
		Material[] materialsAdded = _materialsAdded;
		if (materialsAdded != null && materialsAdded.Length != 0)
		{
			BattlefieldManager.dimmableMaterials?.AddRange(_materialsAdded);
		}
	}

	private void OnDestroy()
	{
		if ((bool)_light)
		{
			BattlefieldManager.dimmableLights?.Remove(_light);
			BattlefieldManager.dimmableLightDefaultIntensities?.Remove(_light.intensity);
			_light = null;
		}
		if ((bool)_particleAdded)
		{
			BattlefieldManager.windTriggeredParticles?.Remove(_particleAdded);
			_particleAdded = null;
		}
		if ((bool)_soundAdded)
		{
			BattlefieldManager.triggeredSounds?.Remove(_soundAdded);
			_soundAdded = null;
		}
		if (_materialsAdded == null)
		{
			return;
		}
		Material[] materialsAdded = _materialsAdded;
		foreach (Material material in materialsAdded)
		{
			if ((bool)material)
			{
				BattlefieldManager.dimmableMaterials?.Remove(material);
			}
		}
	}
}
