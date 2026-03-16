using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Meta.MainNavigation;

public class CarouselDataProvider
{
	private ICarouselServiceWrapper _wrapper;

	private static readonly ICarouselFilter[] Filters = new ICarouselFilter[1]
	{
		new CarouselAssetFilter()
	};

	public bool Initialized { get; private set; }

	public static CarouselDataProvider Create()
	{
		return new CarouselDataProvider();
	}

	private CarouselDataProvider()
	{
		_wrapper = Pantry.Get<ICarouselServiceWrapper>();
	}

	public Promise<List<Client_CarouselItem>> RefreshCarouselItems(string region, string language)
	{
		return RefreshCarouselItemsAsync(region, language);
	}

	private Promise<List<Client_CarouselItem>> RefreshCarouselItemsAsync(string region, string language)
	{
		return _wrapper.GetCarouselItems(region, language, Application.platform.ToString()).IfError((Promise<List<Client_CarouselItem>> p) => p).IfSuccess((Promise<List<Client_CarouselItem>> p) => p.Result.AsyncWhere(FilterItem).AsPromise().Convert((IEnumerable<Client_CarouselItem> items) => items.OrderByDescending((Client_CarouselItem i) => i.Priority).ToList())
			.Then(delegate
			{
				Initialized = true;
			}));
	}

	private static async Task<bool> FilterItem(Client_CarouselItem item)
	{
		ICarouselFilter[] filters = Filters;
		foreach (ICarouselFilter carouselFilter in filters)
		{
			if (item == null)
			{
				return false;
			}
			if (!(await carouselFilter.checkVisible(item)))
			{
				return false;
			}
		}
		return true;
	}
}
