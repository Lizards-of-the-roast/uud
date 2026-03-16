using System;
using GreClient.Rules;
using Pooling;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene;

public class GenericStrategy : IHeadlessClientStrategy
{
	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly Random _rng;

	private readonly RequestHandlerFactory<SelectTargetsRequest> _selectTargetsFactory;

	private readonly TurnInformation _turnInformation = new TurnInformation();

	private MtgGameState _gameState;

	public GenericStrategy(ICardDatabaseAdapter cardDatabase, IObjectPool objectPool = null, Random rng = null)
	{
		_cardDatabase = cardDatabase;
		_rng = rng ?? new Random();
		_selectTargetsFactory = new SelectTargetsRequestRandomHandlerFactory(_rng, objectPool ?? new ObjectPool());
	}

	public void HandleRequest(BaseUserRequest request)
	{
		GetHandlerForRequest(request, _gameState).HandleRequest();
	}

	private BaseUserRequestHandler GetHandlerForRequest(BaseUserRequest request, MtgGameState gameState)
	{
		if (!(request is ActionsAvailableRequest request2))
		{
			if (!(request is DeclareAttackerRequest request3))
			{
				if (!(request is DeclareBlockersRequest request4))
				{
					if (!(request is CastingTimeOptionRequest request5))
					{
						if (!(request is NumericInputRequest request6))
						{
							if (!(request is GroupRequest request7))
							{
								if (!(request is SearchRequest request8))
								{
									if (!(request is SelectNGroupRequest request9))
									{
										if (!(request is SearchFromGroupsRequest request10))
										{
											if (!(request is DistributionRequest request11))
											{
												if (!(request is PayCostsRequest decision))
												{
													if (!(request is OptionalActionMessageRequest request12))
													{
														if (!(request is SelectFromGroupsRequest request13))
														{
															if (!(request is SelectNRequest request14))
															{
																if (!(request is SelectTargetsRequest request15))
																{
																	if (!(request is ChooseStartingPlayerRequest request16))
																	{
																		if (!(request is MulliganRequest request17))
																		{
																			if (!(request is OrderRequest request18))
																			{
																				if (!(request is GatherRequest request19))
																				{
																					if (!(request is AssignDamageRequest request20))
																					{
																						if (!(request is SelectReplacementRequest request21))
																						{
																							if (!(request is IntermissionRequest request22))
																							{
																								if (request is SubmitDeckRequest request23)
																								{
																									return new SubmitDeckRequestHandler(request23);
																								}
																								return new UnknownRequestHandler(request);
																							}
																							return new IntermissionRequestHandler(request22);
																						}
																						return new SelectReplacementRequestHandler(request21);
																					}
																					return new AssignDamageRequestHandler(request20);
																				}
																				return new GatherRequestHandler(request19);
																			}
																			return new OrderRequestHandler(request18);
																		}
																		return new MulliganRequestHandler(request17);
																	}
																	return new ChooseLocalPlayerRequestHandler(request16, gameState);
																}
																return _selectTargetsFactory.GetHandlerForRequest(request15);
															}
															return new SelectNRequestRandomHandler(request14, _rng, _cardDatabase);
														}
														return new SelectFromGroupsRequestRandomHandler(request13, _rng);
													}
													return new OptionalActionRequestRandomHandler(request12, _rng);
												}
												return new PayCostsRequestRandomHandler(decision, _rng);
											}
											return new DistributionRequestRandomHandler(request11, _rng);
										}
										return new SearchFromGroupsRequestRandomHandler(request10, _rng);
									}
									return new SelectNGroupRequestRandomHandler(request9, _rng);
								}
								return new SearchRequestRandomHandler(request8, _rng);
							}
							return new GroupRequestRandomHandler(request7, _rng);
						}
						return new NumericInputRequestRandomHandler(request6, _rng);
					}
					return new CastingTimeOptionRandomHandler(request5, _rng);
				}
				return new DeclareBlockersGenericHandler(request4);
			}
			return new DeclareAttackersGenericHandler(request3);
		}
		return new ActionsAvailableRequestGenericHandler(request2, _turnInformation, _cardDatabase);
	}

	public void SetGameState(MtgGameState state)
	{
		_gameState = state;
		_selectTargetsFactory.SetGameState(state);
		_turnInformation.SetActivePlayer(_gameState.ActivePlayer);
		_turnInformation.SetPhase(_gameState.CurrentPhase);
		_turnInformation.SetStep(_gameState.CurrentStep);
	}
}
