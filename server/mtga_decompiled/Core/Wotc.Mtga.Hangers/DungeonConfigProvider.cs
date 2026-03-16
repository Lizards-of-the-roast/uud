using System;
using System.Collections.Generic;
using System.Text;
using GreClient.CardData;

namespace Wotc.Mtga.Hangers;

public class DungeonConfigProvider : IHangerConfigProvider
{
	private IHangerConfigProvider[] _providers;

	private List<HangerConfig> combinedConfigs = new List<HangerConfig>();

	private static StringBuilder _bodySB = new StringBuilder();

	public DungeonConfigProvider(params IHangerConfigProvider[] providers)
	{
		_providers = providers ?? Array.Empty<IHangerConfigProvider>();
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		combinedConfigs.Clear();
		IHangerConfigProvider[] providers = _providers;
		foreach (IHangerConfigProvider hangerConfigProvider in providers)
		{
			combinedConfigs.AddRange(hangerConfigProvider.GetHangerConfigs(model));
		}
		HangerConfig hangerConfig = MergedHanger(combinedConfigs);
		if (!hangerConfig.Equals(default(HangerConfig)))
		{
			yield return hangerConfig;
		}
	}

	public static HangerConfig MergedHanger(IReadOnlyList<HangerConfig> configs)
	{
		if (configs.Count > 1)
		{
			_bodySB.Clear();
			_bodySB.Append(configs[0].Details);
			string spritePath = configs[0].SpritePath;
			for (int i = 1; i < configs.Count; i++)
			{
				_bodySB.AppendLine();
				_bodySB.AppendLine();
				_bodySB.Append(configs[i].Header);
				_bodySB.AppendLine();
				_bodySB.Append(configs[i].Details);
				if (string.IsNullOrEmpty(spritePath))
				{
					spritePath = configs[i].SpritePath;
				}
			}
			return new HangerConfig(configs[0].Header, _bodySB.ToString(), null, spritePath);
		}
		if (configs.Count > 0)
		{
			return configs[0];
		}
		return default(HangerConfig);
	}
}
