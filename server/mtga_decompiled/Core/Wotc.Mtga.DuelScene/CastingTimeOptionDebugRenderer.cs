using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.DuelScene;

public class CastingTimeOptionDebugRenderer : BaseUserRequestDebugRenderer<CastingTimeOptionRequest>
{
	private List<BaseUserRequestDebugRenderer> _childHandlers = new List<BaseUserRequestDebugRenderer>();

	public CastingTimeOptionDebugRenderer(CastingTimeOptionRequest request, IEnumerable<BaseUserRequestDebugRenderer> childHandlers)
		: base(request)
	{
		_childHandlers.AddRange(childHandlers);
	}

	public override void Render()
	{
		_childHandlers.ForEach(delegate(BaseUserRequestDebugRenderer x)
		{
			x.Render();
		});
	}

	public static IEnumerable<BaseUserRequestDebugRenderer> DefaultHandlers(CastingTimeOptionRequest request, ICardDatabaseAdapter cardDatabase)
	{
		if (request.CanCancel)
		{
			yield return new CancelRequestDebugRenderer(request);
		}
		foreach (BaseUserRequest childRequest in request.ChildRequests)
		{
			BaseUserRequestDebugRenderer baseUserRequestDebugRenderer = RendererForRequest(childRequest, cardDatabase);
			if (baseUserRequestDebugRenderer != null)
			{
				yield return baseUserRequestDebugRenderer;
			}
			else
			{
				Debug.LogError($"Unhandled Casting Time Option child request: {childRequest}");
			}
		}
	}

	private static BaseUserRequestDebugRenderer RendererForRequest(BaseUserRequest child, ICardDatabaseAdapter cardDatabase)
	{
		if (!(child is CastingTimeOption_KickerRequest request))
		{
			if (!(child is CastingTimeOption_DoneRequest request2))
			{
				if (!(child is CastingTimeOption_NumericInputRequest request3))
				{
					if (!(child is CastingTimeOption_ModalRequest request4))
					{
						if (!(child is CastingTimeOption_CostKeywordRequest request5))
						{
							if (child is CastingTimeOption_AdditionalCostRequest request6)
							{
								return new CtoAdditionalCostDebugRenderer(request6);
							}
							return null;
						}
						return new CtoCostKeywordDebugRenderer(request5);
					}
					return new CtoModalDebugRenderer(request4, cardDatabase);
				}
				return new CtoNumericInputDebugRenderer(request3);
			}
			return new CtoDoneDebugRenderer(request2);
		}
		return new CtoKickerDebugRenderer(request);
	}
}
