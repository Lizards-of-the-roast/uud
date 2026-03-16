using System;
using System.Collections.Generic;
using System.Linq;
using Wizards.Arena.Models;
using Wizards.Arena.Promises;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Wizards.Mtga.PreferredPrinting;

public class PreferredPrintingDataProvider : IPreferredPrintingDataProvider, IDisposable
{
	private IPreferredPrintingServiceWrapper _serviceWrapper;

	private Dictionary<int, PreferredPrintingWithStyle> _allPreferredPrintings;

	private Promise<Dictionary<int, PreferredPrintingWithStyle>> _refreshing;

	public static IPreferredPrintingDataProvider Create()
	{
		return new PreferredPrintingDataProvider(Pantry.Get<IPreferredPrintingServiceWrapper>());
	}

	public PreferredPrintingDataProvider(IPreferredPrintingServiceWrapper serviceWrapper)
	{
		_serviceWrapper = serviceWrapper;
		_allPreferredPrintings = new Dictionary<int, PreferredPrintingWithStyle>();
	}

	public IReadOnlyDictionary<int, PreferredPrintingWithStyle> GetAllPreferredPrintings()
	{
		return _allPreferredPrintings ?? new Dictionary<int, PreferredPrintingWithStyle>();
	}

	public PreferredPrintingWithStyle GetPreferredPrintingForTitleId(int titleId)
	{
		PreferredPrintingWithStyle value = null;
		if (_allPreferredPrintings.TryGetValue(titleId, out value))
		{
			return value;
		}
		return null;
	}

	public Promise<bool> SetPreferredPrintingForTitleId(int titleId, int grpId, string styleCode)
	{
		Promise<bool> result = _serviceWrapper.SetPreferredPrinting(titleId, grpId, styleCode).IfError(delegate(Promise<bool> promise)
		{
			SimpleLog.LogError("SetPreferredPrintingForTitleId FAILED: " + promise.Error.Exception);
		});
		_allPreferredPrintings[titleId] = new PreferredPrintingWithStyle
		{
			printingGrpId = grpId,
			styleCode = styleCode
		};
		return result;
	}

	public Promise<bool> RemovePreferredPrintingForTitleId(int titleId)
	{
		Promise<bool> result = _serviceWrapper.RemovePreferredPrinting(titleId).IfError(delegate(Promise<bool> promise)
		{
			SimpleLog.LogError("RemovePreferredPrintingForTitleId FAILED: " + promise.Error.Exception);
		});
		_allPreferredPrintings.Remove(titleId);
		return result;
	}

	public Promise<Dictionary<int, PreferredPrintingWithStyle>> ForceRefreshPreferredPrintings()
	{
		_refreshing = _serviceWrapper.GetAllPreferredPrintings().Convert(ConvertToClientPreferredPrintingWithStyle).IfSuccess(delegate(Promise<Dictionary<int, PreferredPrintingWithStyle>> p)
		{
			_allPreferredPrintings = p.Result;
		})
			.Then(delegate
			{
				clearRefreshingPromise();
			})
			.IfError(delegate(Promise<Dictionary<int, PreferredPrintingWithStyle>> promise)
			{
				SimpleLog.LogError("ForceRefreshPreferredPrintings FAILED: " + promise.Error.Exception);
			});
		return _refreshing;
	}

	private void clearRefreshingPromise()
	{
		_refreshing = null;
	}

	private Dictionary<int, PreferredPrintingWithStyle> ConvertToClientPreferredPrintingWithStyle(Dictionary<int, DTO_PreferredPrintingWithStyle> preferredPrintings)
	{
		return preferredPrintings.Where((KeyValuePair<int, DTO_PreferredPrintingWithStyle> x) => x.Value != null).ToDictionary((KeyValuePair<int, DTO_PreferredPrintingWithStyle> x) => x.Key, (KeyValuePair<int, DTO_PreferredPrintingWithStyle> x) => ConvertToClientType(x.Value));
	}

	private PreferredPrintingWithStyle ConvertToClientType(DTO_PreferredPrintingWithStyle preferredPrinting)
	{
		return new PreferredPrintingWithStyle
		{
			printingGrpId = preferredPrinting.GrpId,
			styleCode = preferredPrinting.StyleCode
		};
	}

	public void Dispose()
	{
		_allPreferredPrintings.Clear();
		_allPreferredPrintings = null;
		_refreshing = null;
		_serviceWrapper = null;
	}
}
