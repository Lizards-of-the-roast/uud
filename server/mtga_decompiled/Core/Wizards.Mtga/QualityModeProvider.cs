using UnityEngine;
using Wizards.Mtga.Platforms;

namespace Wizards.Mtga;

public class QualityModeProvider
{
	private static readonly string DebugPlayerPrefString = "QualityModeProvider.UseDebugValues";

	private static readonly string DebugCardVFXPlayerPrefString = "QualityModeProvider.CardVFX";

	private static readonly string DebugPetPlayerPrefString = "QualityModeProvider.Pet";

	private static readonly string DebugForceMinSpecString = "QualityModeProvider.ForceMinSpec";

	private static readonly string DebugDisableAssetPoolString = "DebugDisableAssetPool";

	private static readonly string DebugDisableObjectPoolString = "DebugDisableObjectPool";

	private static bool gotCachedValues = false;

	private static bool cachedForceMinSpec;

	private static bool cachedUseDebugValues;

	private static QualityMode_CardVFX cachedDebugQualityModeCardVFX;

	private static QualityMode_Pet cachedDebugQualityModePet;

	private static bool cachedDebugDisableAssetPool;

	private static bool cachedDebugDisableObjectPool;

	public static bool ForceMinSpec
	{
		get
		{
			GetCachedValues();
			return cachedForceMinSpec;
		}
		set
		{
			if (cachedForceMinSpec != value)
			{
				cachedForceMinSpec = value;
				PlayerPrefsExt.SetBool(DebugForceMinSpecString, value, save: true);
			}
		}
	}

	public static bool UseDebugValues
	{
		get
		{
			GetCachedValues();
			return cachedUseDebugValues;
		}
		set
		{
			if (cachedUseDebugValues != value)
			{
				cachedUseDebugValues = value;
				PlayerPrefsExt.SetBool(DebugPlayerPrefString, value, save: true);
			}
		}
	}

	public static QualityMode_CardVFX DebugQualityModeCardVFX
	{
		get
		{
			GetCachedValues();
			return cachedDebugQualityModeCardVFX;
		}
		set
		{
			if (cachedDebugQualityModeCardVFX != value)
			{
				cachedDebugQualityModeCardVFX = value;
				PlayerPrefsExt.SetInt(DebugCardVFXPlayerPrefString, (int)value, save: true);
			}
		}
	}

	public static QualityMode_Pet DebugQualityModePet
	{
		get
		{
			GetCachedValues();
			return cachedDebugQualityModePet;
		}
		set
		{
			if (cachedDebugQualityModePet != value)
			{
				cachedDebugQualityModePet = value;
				PlayerPrefsExt.SetInt(DebugPetPlayerPrefString, (int)value, save: true);
			}
		}
	}

	public static bool DebugDisableAssetPool
	{
		get
		{
			GetCachedValues();
			return cachedDebugDisableAssetPool;
		}
		set
		{
			if (cachedDebugDisableAssetPool != value)
			{
				cachedDebugDisableAssetPool = value;
				PlayerPrefsExt.SetBool(DebugDisableAssetPoolString, value, save: true);
			}
		}
	}

	public static bool DebugDisableObjectPool
	{
		get
		{
			GetCachedValues();
			return cachedDebugDisableObjectPool;
		}
		set
		{
			if (cachedDebugDisableObjectPool != value)
			{
				cachedDebugDisableObjectPool = value;
				PlayerPrefsExt.SetBool(DebugDisableObjectPoolString, value, save: true);
			}
		}
	}

	public bool DisableAssetPool
	{
		get
		{
			if (!UseDebugValues)
			{
				return IsMinSpec();
			}
			return DebugDisableAssetPool;
		}
	}

	public bool DisableObjectPool
	{
		get
		{
			if (!UseDebugValues)
			{
				return IsMinSpec();
			}
			return DebugDisableObjectPool;
		}
	}

	private static void GetCachedValues()
	{
		if (!gotCachedValues)
		{
			cachedForceMinSpec = PlayerPrefsExt.GetBool(DebugForceMinSpecString, defaultValue: false);
			cachedUseDebugValues = PlayerPrefsExt.GetBool(DebugPlayerPrefString, defaultValue: false);
			cachedDebugQualityModeCardVFX = (QualityMode_CardVFX)PlayerPrefsExt.GetInt(DebugCardVFXPlayerPrefString);
			cachedDebugQualityModePet = (QualityMode_Pet)PlayerPrefsExt.GetInt(DebugPetPlayerPrefString);
			cachedDebugDisableAssetPool = PlayerPrefsExt.GetBool(DebugDisableAssetPoolString, defaultValue: false);
			cachedDebugDisableObjectPool = PlayerPrefsExt.GetBool(DebugDisableObjectPoolString, defaultValue: false);
			gotCachedValues = true;
		}
	}

	public bool IsMinSpec()
	{
		if (!Application.isEditor || !ForceMinSpec)
		{
			if (PlatformUtils.IsHandheld())
			{
				return QualitySettings.GetQualityLevel() == 0;
			}
			return false;
		}
		return true;
	}

	public virtual QualityMode_CardVFX GetQualityMode_CardVFX()
	{
		if (UseDebugValues)
		{
			return DebugQualityModeCardVFX;
		}
		if (IsMinSpec())
		{
			return QualityMode_CardVFX.MinSpec;
		}
		return QualityMode_CardVFX.Default;
	}

	public virtual QualityMode_Pet GetQualityMode_Pet()
	{
		if (UseDebugValues)
		{
			return DebugQualityModePet;
		}
		if (IsMinSpec())
		{
			return QualityMode_Pet.MinSpec;
		}
		return QualityMode_Pet.Default;
	}
}
