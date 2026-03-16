using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Wotc.Mtga;

namespace Wizards.Mtga.Credits;

public class CreditsDataProvider : ICreditsDataProvider
{
	private List<CreditSectionData> _creditsData;

	private bool _initialized;

	public static ICreditsDataProvider Create()
	{
		return new CreditsDataProvider();
	}

	private void Initialize()
	{
		List<CreditSectionData> list = JsonConvert.DeserializeObject<List<CreditSectionData>>(File.ReadAllText(AssetLoader.GetRawFilePath(DataSourceUtilities.GetCurrentDataSource(), "credits.json")));
		if (list == null)
		{
			SimpleLog.LogError("Credits json data could not be loaded");
		}
		else
		{
			_creditsData = list;
		}
		_initialized = true;
	}

	public IReadOnlyCollection<CreditSectionData> GetCredits()
	{
		if (!_initialized)
		{
			Initialize();
		}
		return _creditsData;
	}
}
