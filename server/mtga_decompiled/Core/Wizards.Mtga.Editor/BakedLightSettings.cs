using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace Wizards.Mtga.Editor;

[ExecuteInEditMode]
public class BakedLightSettings : MonoBehaviour
{
	public FalloffType FalloffType = FalloffType.Legacy;

	public void OnEnable()
	{
		Lightmapping.SetDelegate(LightMappingDelegate);
	}

	private void OnDisable()
	{
		Lightmapping.ResetDelegate();
	}

	private void LightMappingDelegate(Light[] requests, NativeArray<LightDataGI> lightsOutput)
	{
		DirectionalLight dir = default(DirectionalLight);
		PointLight point = default(PointLight);
		SpotLight spot = default(SpotLight);
		RectangleLight rect = default(RectangleLight);
		DiscLight disc = default(DiscLight);
		Cookie cookie = default(Cookie);
		LightDataGI value = default(LightDataGI);
		for (int i = 0; i < requests.Length; i++)
		{
			Light light = requests[i];
			switch (light.type)
			{
			case UnityEngine.LightType.Directional:
				LightmapperUtils.Extract(light, ref dir);
				LightmapperUtils.Extract(light, out cookie);
				value.Init(ref dir, ref cookie);
				break;
			case UnityEngine.LightType.Point:
				LightmapperUtils.Extract(light, ref point);
				LightmapperUtils.Extract(light, out cookie);
				value.Init(ref point, ref cookie);
				break;
			case UnityEngine.LightType.Spot:
				LightmapperUtils.Extract(light, ref spot);
				LightmapperUtils.Extract(light, out cookie);
				value.Init(ref spot, ref cookie);
				break;
			case UnityEngine.LightType.Area:
				LightmapperUtils.Extract(light, ref rect);
				LightmapperUtils.Extract(light, out cookie);
				value.Init(ref rect, ref cookie);
				break;
			case UnityEngine.LightType.Disc:
				LightmapperUtils.Extract(light, ref disc);
				LightmapperUtils.Extract(light, out cookie);
				value.Init(ref disc, ref cookie);
				break;
			default:
				value.InitNoBake(light.GetInstanceID());
				break;
			}
			value.cookieID = ((!(light.cookie == null)) ? light.cookie.GetInstanceID() : 0);
			value.falloff = FalloffType;
			lightsOutput[i] = value;
		}
	}
}
