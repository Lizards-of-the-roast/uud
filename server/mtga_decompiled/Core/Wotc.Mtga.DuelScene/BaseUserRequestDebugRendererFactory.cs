using GreClient.Rules;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.DuelScene;

public class BaseUserRequestDebugRendererFactory
{
	private ICardDatabaseAdapter _cardDatabase;

	public BaseUserRequestDebugRendererFactory(ICardDatabaseAdapter cardDatabase)
	{
		_cardDatabase = cardDatabase;
	}

	public BaseUserRequestDebugRenderer CreateDebugRendererForRequest(BaseUserRequest request, MtgGameState gameState)
	{
		if (!(request is CastingTimeOptionRequest request2))
		{
			if (!(request is ActionsAvailableRequest actionsAvailableRequest))
			{
				if (!(request is SelectTargetsRequest selectTargetsRequest))
				{
					if (!(request is DeclareAttackerRequest declareAttackerRequest))
					{
						if (!(request is DeclareBlockersRequest declareBlockersRequest))
						{
							if (!(request is ChooseStartingPlayerRequest chooseStartingPlayerRequest))
							{
								if (!(request is SelectNRequest request3))
								{
									if (!(request is SearchRequest request4))
									{
										if (!(request is PayCostsRequest payCostsRequest))
										{
											if (!(request is SelectReplacementRequest request5))
											{
												if (!(request is OptionalActionMessageRequest optionalActionMessageRequest))
												{
													if (!(request is MulliganRequest mulliganRequest))
													{
														if (!(request is SubmitDeckRequest request6))
														{
															if (request is SelectNGroupRequest request7)
															{
																return new SelectNGroupRequestDebugRenderer(request7);
															}
															return null;
														}
														return new SubmitDeckRequestDebugRenderer(request6);
													}
													return new MulliganRequestDebugRenderer(mulliganRequest);
												}
												return new OptionalActionMessageRequestDebugRenderer(optionalActionMessageRequest);
											}
											return new SelectReplacementRequestDebugRenderer(request5, gameState, _cardDatabase);
										}
										if (payCostsRequest.EffectCost != null && payCostsRequest.EffectCost.CostSelection != null)
										{
											return new SelectNRequestDebugRenderer(payCostsRequest.EffectCost.CostSelection, gameState, _cardDatabase);
										}
										return new PayCostsRequestDebugRenderer(payCostsRequest, gameState, _cardDatabase);
									}
									return new SearchRequestDebugRenderer(request4, gameState, _cardDatabase);
								}
								return new SelectNRequestDebugRenderer(request3, gameState, _cardDatabase);
							}
							return new ChooseStartingPlayerRequestDebugRenderer(chooseStartingPlayerRequest, gameState);
						}
						return new DeclareBlockersRequestDebugRenderer(declareBlockersRequest, gameState, _cardDatabase);
					}
					return new DeclareAttackerRequestDebugRenderer(declareAttackerRequest, gameState, _cardDatabase);
				}
				return new SelectTargetsRequestDebugRenderer(selectTargetsRequest, gameState, _cardDatabase);
			}
			return new ActionsAvailableRequestDebugRenderer(actionsAvailableRequest, gameState, _cardDatabase);
		}
		return new CastingTimeOptionDebugRenderer(request2, CastingTimeOptionDebugRenderer.DefaultHandlers(request2, _cardDatabase));
	}
}
