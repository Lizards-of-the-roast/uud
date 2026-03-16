using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

public class HeuristicStrategy : IHeadlessClientStrategy
{
	private readonly float _minWaitTime;

	private readonly float _maxWaitTime;

	private readonly float _maxIdleTime;

	private readonly DeckHeuristic _deckHeuristic;

	private readonly AttackConfig _attackConfig;

	private readonly BlockConfig _blockConfig;

	private readonly System.Random _rng = new System.Random();

	private readonly SelectTargetsState _targetState = new SelectTargetsState();

	private readonly TurnInformation _turnInformation = new TurnInformation();

	private readonly ICardDatabaseAdapter _cardDatabase;

	private MtgGameState _gameState;

	private BaseUserRequest _handledRequest;

	private readonly UnityFamiliar _botFamiliar;

	private Coroutine _idleTimerCoroutine;

	private Coroutine _handleRequestCoroutine;

	public HeuristicStrategy(float minWaitTime, float maxWaitTime, float maxIdleTime, DeckHeuristic heuristic, AttackConfig attackConfig, BlockConfig blockConfig, UnityFamiliar unityFamiliar, ICardDatabaseAdapter cardDatabase)
	{
		_minWaitTime = minWaitTime;
		_maxWaitTime = maxWaitTime;
		_maxIdleTime = maxIdleTime;
		_deckHeuristic = heuristic;
		_attackConfig = attackConfig;
		_blockConfig = blockConfig;
		_cardDatabase = cardDatabase;
		_botFamiliar = unityFamiliar;
	}

	public void HandleRequest(BaseUserRequest request)
	{
		float num = ((request.Type == RequestType.ChooseStartingPlayer) ? 5 : 0);
		float minInclusive = num + _minWaitTime;
		float maxInclusive = num + _maxWaitTime;
		float num2 = UnityEngine.Random.Range(minInclusive, maxInclusive);
		if (TryGetHandlerForRequest(request, out var requestHandler))
		{
			requestHandler = new DelayedRequestHandler(requestHandler, num2);
			requestHandler.HandleRequest();
			_handledRequest = request;
		}
		else if (_handleRequestCoroutine == null || _handledRequest != request)
		{
			if (_handleRequestCoroutine != null)
			{
				_botFamiliar.StopCoroutine(_handleRequestCoroutine);
				_handleRequestCoroutine = null;
			}
			StartIdleTimer(request, _maxIdleTime);
			_handleRequestCoroutine = _botFamiliar.StartCoroutine(CO_WaitBeforeHandleRequest(request, num2));
		}
	}

	public void SetGameState(MtgGameState state)
	{
		_gameState = state;
		_turnInformation.SetActivePlayer(_gameState.ActivePlayer);
		_turnInformation.SetPhase(_gameState.CurrentPhase);
		_turnInformation.SetStep(_gameState.CurrentStep);
	}

	private bool TryGetHandlerForRequest(BaseUserRequest request, out BaseUserRequestHandler requestHandler)
	{
		requestHandler = GetHandlerForRequest(request, _gameState);
		return requestHandler != null;
	}

	private BaseUserRequestHandler GetHandlerForRequest(BaseUserRequest request, MtgGameState gameState)
	{
		if (!(request is ActionsAvailableRequest request2))
		{
			if (!(request is PayCostsRequest payCostsRequest))
			{
				if (!(request is OptionalActionMessageRequest request3))
				{
					if (!(request is SelectFromGroupsRequest request4))
					{
						if (!(request is SelectNRequest request5))
						{
							if (!(request is SelectTargetsRequest request6))
							{
								if (!(request is CastingTimeOptionRequest request7))
								{
									if (!(request is NumericInputRequest request8))
									{
										if (!(request is GroupRequest request9))
										{
											if (!(request is SearchRequest request10))
											{
												if (!(request is SelectNGroupRequest request11))
												{
													if (!(request is SearchFromGroupsRequest request12))
													{
														if (!(request is DistributionRequest request13))
														{
															if (!(request is ChooseStartingPlayerRequest request14))
															{
																if (!(request is MulliganRequest request15))
																{
																	if (!(request is OrderRequest request16))
																	{
																		if (!(request is GatherRequest request17))
																		{
																			if (!(request is AssignDamageRequest request18))
																			{
																				if (!(request is SelectReplacementRequest request19))
																				{
																					if (!(request is IntermissionRequest request20))
																					{
																						if (request is SubmitDeckRequest request21)
																						{
																							return new SubmitDeckRequestHandler(request21);
																						}
																						return null;
																					}
																					return new IntermissionRequestHandler(request20);
																				}
																				return new SelectReplacementRequestHandler(request19);
																			}
																			return new AssignDamageRequestHandler(request18);
																		}
																		return new GatherRequestHandler(request17);
																	}
																	return new OrderRequestHandler(request16);
																}
																return new MulliganRequestHandler(request15);
															}
															return new ChooseLocalPlayerRequestHandler(request14, gameState);
														}
														return new DistributionRequestRandomHandler(request13, _rng);
													}
													return new SearchFromGroupsRequestRandomHandler(request12, _rng);
												}
												return new SelectNGroupRequestRandomHandler(request11, _rng);
											}
											return new SearchRequestRandomHandler(request10, _rng);
										}
										return new GroupRequestRandomHandler(request9, _rng);
									}
									return new NumericInputRequestRandomHandler(request8, _rng);
								}
								return new CastingTimeOptionRandomHandler(request7, _rng);
							}
							return new SelectTargetHeuristicHandler(request6, _deckHeuristic, _gameState, _targetState, _turnInformation, _rng, _cardDatabase);
						}
						return new SelectNRequestHeuristicHandler(request5, _deckHeuristic, _gameState, _cardDatabase, _rng, new SelectNRequestRandomHandler(request5, _rng, _cardDatabase));
					}
					return new SelectFromGroupsHeuristicHandler(request4, _deckHeuristic, _gameState, _cardDatabase, _rng);
				}
				return new OptionalActionRequestHeuristicHandler(request3, _deckHeuristic, _gameState, _cardDatabase, new OptionalActionRequestRandomHandler(request3, _rng));
			}
			return new PayCostHeuristicHandler(payCostsRequest, _gameState, _rng, _cardDatabase, new PayCostsRequestRandomHandler(payCostsRequest, _rng));
		}
		return new ActionsAvailableHeuristicHandler(request2, _deckHeuristic, _gameState, _turnInformation, _cardDatabase);
	}

	private IEnumerator CO_HandleDeclareAttackerRequest(DeclareAttackerRequest request)
	{
		if (request.AttackWarnings.Exists((AttackWarning warning) => warning.Type == AttackWarningType.CannotAttackAlone))
		{
			request.DeclareAllAttackers(request.Attackers[0].LegalDamageRecipients[0]);
			_turnInformation.attackersDeclared = true;
			yield break;
		}
		if (_turnInformation.attackersDeclared)
		{
			AttemptSubmitAttackers(request);
			yield break;
		}
		if (_turnInformation.attackersToDeclare.Count == 0)
		{
			AttackingAI.StartThread(_deckHeuristic, _gameState, new AttackingConfiguration(request), _cardDatabase);
			yield return new WaitForEndOfFrame();
			yield return new WaitUntil(() => !AttackingAI.IsCalculating || (float)AttackingAI.ThreadTimer.ElapsedMilliseconds > _attackConfig.MaxAttackCalculationTime * 1000f);
			AttackingAI.StopThread();
			if (AttackingAI.SortedAttackConfigurations.Count > 0)
			{
				float num = (float)_gameState.LocalPlayerBattlefieldCards.Count((MtgCardInstance card) => card.CardTypes.Contains(CardType.Creature)) / _attackConfig.AICreatureDensityFactor;
				float num2 = (float)_gameState.OpponentBattlefieldCards.Count((MtgCardInstance card) => card.CardTypes.Contains(CardType.Creature)) / _attackConfig.PlayerCreatureDensityFactor;
				float num3 = (float)_turnInformation.numberOfTurnsWithoutAttacks / _attackConfig.IdleAttackTurnsDensityFactor;
				int num4 = (int)num + (int)num2 + (int)num3;
				if (num4 < 0)
				{
					num4 = 0;
				}
				else if (num4 >= AttackingAI.SortedAttackConfigurations.Count)
				{
					num4 = AttackingAI.SortedAttackConfigurations.Count - 1;
				}
				_turnInformation.attackersToDeclare = AttackingAI.SortedAttackConfigurations[num4].CommittedAttackerIds;
				AttackingAI.SortedAttackConfigurations.Clear();
			}
		}
		if (_turnInformation.attackersToDeclare.Count > 0)
		{
			uint num5 = _turnInformation.attackersToDeclare[0];
			_turnInformation.attackersToDeclare.RemoveAt(0);
			foreach (Attacker attacker in request.Attackers)
			{
				if (attacker.AttackerInstanceId == num5 && attacker.LegalDamageRecipients.Count > 0)
				{
					attacker.SelectedDamageRecipient = attacker.LegalDamageRecipients[0];
					request.UpdateAttacker(attacker);
					break;
				}
			}
		}
		else
		{
			AttemptSubmitAttackers(request);
		}
		if (_turnInformation.attackersToDeclare.Count == 0)
		{
			_turnInformation.attackersDeclared = true;
		}
	}

	private IEnumerator CO_HandleAssignBlockerRequest(DeclareBlockersRequest request)
	{
		if (_turnInformation.blockersDeclared)
		{
			AttemptSubmitBlockers(request);
			yield break;
		}
		if (_turnInformation.blocksToAssign.Count == 0)
		{
			List<BlockToMake> requiredBlocksToMake = new List<BlockToMake>();
			bool flag = false;
			if (request.BlockWarnings.Count > 0)
			{
				foreach (BlockWarning blockWarning2 in request.BlockWarnings)
				{
					if (blockWarning2.Type != BlockWarningType.MustBeBlockedByAll)
					{
						continue;
					}
					requiredBlocksToMake.Clear();
					foreach (Blocker allBlocker in request.AllBlockers)
					{
						BlockToMake item = new BlockToMake(allBlocker.BlockerInstanceId, blockWarning2.InstanceId);
						requiredBlocksToMake.Add(item);
					}
					flag = true;
					break;
				}
			}
			if (flag)
			{
				_turnInformation.blocksToAssign = requiredBlocksToMake;
			}
			else
			{
				BlockingAI.StartThread(_deckHeuristic, _gameState, new BlockingConfiguration(new SimpleGameStateConstruction(_deckHeuristic, _gameState.LocalPlayer.InstanceId, _gameState, _cardDatabase.AbilityDataProvider), request), _cardDatabase);
				yield return new WaitForEndOfFrame();
				yield return new WaitUntil(() => !BlockingAI.IsCalculating || (float)BlockingAI.ThreadTimer.ElapsedMilliseconds > _blockConfig.MaxBlockCalculationTime * 1000f);
				BlockingAI.StopThread();
				if (BlockingAI.SortedBlockConfigurations.Count > 0)
				{
					_turnInformation.blocksToAssign = BlockingAI.SortedBlockConfigurations[0].GetListOfBlocksToMake();
					BlockingAI.SortedBlockConfigurations.Clear();
				}
				if (request.BlockWarnings.Count > 0)
				{
					if (request.BlockWarnings.Exists((BlockWarning warning) => warning.Type == BlockWarningType.CannotBlockAlone) && _turnInformation.blocksToAssign.Count == 1)
					{
						_turnInformation.blocksToAssign.Clear();
					}
					foreach (BlockWarning blockWarning in request.BlockWarnings)
					{
						if (blockWarning.Type == BlockWarningType.MustBlock)
						{
							if (!_turnInformation.blocksToAssign.Exists((BlockToMake blockToMake2) => blockToMake2.BlockerId == blockWarning.InstanceId))
							{
								uint a = _gameState.AttackInfo.Keys.SelectRandom();
								BlockToMake item2 = new BlockToMake(blockWarning.InstanceId, a);
								requiredBlocksToMake.Add(item2);
							}
						}
						else
						{
							if (blockWarning.Type != BlockWarningType.MustBeBlocked || _turnInformation.blocksToAssign.Exists((BlockToMake blockToMake2) => blockToMake2.AttackerId == blockWarning.InstanceId))
							{
								continue;
							}
							Blocker weakestBlocker = null;
							foreach (Blocker potentialBlocker in request.AllBlockers)
							{
								List<BlockWarning> list = new List<BlockWarning>();
								foreach (BlockWarning blockWarning3 in request.BlockWarnings)
								{
									if (blockWarning != blockWarning3 && blockWarning3.Type == BlockWarningType.MustBeBlocked)
									{
										list.Add(blockWarning3);
									}
								}
								bool flag2 = true;
								foreach (BlockWarning otherBlockWarning in list)
								{
									if (_turnInformation.blocksToAssign.Exists((BlockToMake blockToMake2) => blockToMake2.BlockerId == potentialBlocker.BlockerInstanceId && blockToMake2.AttackerId == otherBlockWarning.InstanceId))
									{
										flag2 = false;
										break;
									}
								}
								if (flag2)
								{
									if (weakestBlocker == null)
									{
										weakestBlocker = potentialBlocker;
									}
									else if (_deckHeuristic.ScoreCard(_gameState.GetCardById(potentialBlocker.BlockerInstanceId)) < _deckHeuristic.ScoreCard(_gameState.GetCardById(weakestBlocker.BlockerInstanceId)))
									{
										weakestBlocker = potentialBlocker;
									}
								}
							}
							if (weakestBlocker != null)
							{
								_turnInformation.blocksToAssign.Remove(_turnInformation.blocksToAssign.Find((BlockToMake blockToMake2) => blockToMake2.BlockerId == weakestBlocker.BlockerInstanceId));
								_turnInformation.blocksToAssign.Add(new BlockToMake(weakestBlocker.BlockerInstanceId, blockWarning.InstanceId));
							}
						}
					}
					if (requiredBlocksToMake.Count > 0)
					{
						_turnInformation.blocksToAssign.AddRange(requiredBlocksToMake);
					}
				}
				MtgCardInstance card;
				List<BlockToMake> list2 = _turnInformation.blocksToAssign.FindAll((BlockToMake block) => _gameState.GetCardById(block.AttackerId).Abilities.Exists((AbilityPrintingData ability) => ability.Id == 1026) || _gameState.GetCardById(block.AttackerId).AttachedWithIds.Exists((uint refId) => _gameState.TryGetCard(refId, out card) && card.GrpId == 69989));
				foreach (BlockToMake allRelevantBlocks in list2)
				{
					List<BlockToMake> blocksToSameAttacker = list2.FindAll((BlockToMake block) => block.AttackerId == allRelevantBlocks.AttackerId);
					if (blocksToSameAttacker.Count <= 0)
					{
						continue;
					}
					BlockToMake blockToKeep = blocksToSameAttacker.Find((BlockToMake currentBlock) => _deckHeuristic.ScoreCard(_gameState.GetCardById(currentBlock.BlockerId)) == blocksToSameAttacker.Min((BlockToMake otherBlock) => _deckHeuristic.ScoreCard(_gameState.GetCardById(otherBlock.BlockerId))));
					_turnInformation.blocksToAssign.RemoveAll((BlockToMake blockToRemove) => blockToRemove != blockToKeep);
				}
			}
		}
		if (_turnInformation.blocksToAssign.Count > 0)
		{
			BlockToMake blockToMake = _turnInformation.blocksToAssign[0];
			_turnInformation.blocksToAssign.RemoveAt(0);
			foreach (Blocker allBlocker2 in request.AllBlockers)
			{
				if (allBlocker2.BlockerInstanceId != blockToMake.BlockerId)
				{
					continue;
				}
				if (allBlocker2.AttackerInstanceIds.Contains(blockToMake.AttackerId))
				{
					if (allBlocker2.SelectedAttackerInstanceIds.Count < allBlocker2.MaxAttackers)
					{
						allBlocker2.SelectedAttackerInstanceIds.Add(blockToMake.AttackerId);
					}
					request.UpdateBlockers(allBlocker2);
					yield return new WaitForSeconds(UnityEngine.Random.Range(_minWaitTime, _maxWaitTime));
				}
				break;
			}
		}
		else
		{
			AttemptSubmitBlockers(request);
		}
		if (_turnInformation.blocksToAssign.Count == 0)
		{
			_turnInformation.blockersDeclared = true;
			yield return new WaitForSeconds(UnityEngine.Random.Range(_minWaitTime, _maxWaitTime));
		}
	}

	private IEnumerator CO_WaitBeforeHandleRequest(BaseUserRequest request, float waitTime)
	{
		_handledRequest = request;
		yield return new WaitForSeconds(waitTime);
		if (request is DeclareAttackerRequest request2)
		{
			yield return CO_HandleDeclareAttackerRequest(request2);
		}
		else if (request is DeclareBlockersRequest request3)
		{
			yield return CO_HandleAssignBlockerRequest(request3);
		}
		else
		{
			new UnknownRequestHandler(request).HandleRequest();
		}
		_handleRequestCoroutine = null;
		_handledRequest = null;
	}

	private IEnumerator CO_RequestTimer(BaseUserRequest request, float maxIdleTime)
	{
		yield return new WaitForSeconds(maxIdleTime);
		yield return new WaitForEndOfFrame();
		if (_gameState.DecidingPlayer != null && _gameState.DecidingPlayer.ClientPlayerEnum == GREPlayerNum.LocalPlayer)
		{
			request.AutoRespond();
		}
	}

	private int GetAvailableManaCount()
	{
		int num = 0;
		foreach (MtgCardInstance localPlayerBattlefieldCard in _gameState.LocalPlayerBattlefieldCards)
		{
			if (localPlayerBattlefieldCard.IsTapped)
			{
				continue;
			}
			foreach (AbilityPrintingData ability in localPlayerBattlefieldCard.Abilities)
			{
				if (ability.SubCategory == AbilitySubCategory.Mana)
				{
					num++;
				}
			}
		}
		return num;
	}

	private List<Attacker> GetAttackingCreatures(DeclareAttackerRequest request)
	{
		List<Attacker> list = new List<Attacker>();
		foreach (Attacker declaredAttacker in request.DeclaredAttackers)
		{
			if (declaredAttacker.SelectedDamageRecipient != null)
			{
				list.Add(declaredAttacker);
			}
		}
		return list;
	}

	private Attacker GetWeakestAttacker(IReadOnlyList<Attacker> attackers)
	{
		Attacker attacker = null;
		foreach (Attacker attacker2 in attackers)
		{
			if (attacker2.LegalDamageRecipients.Count > 0)
			{
				if (attacker == null)
				{
					attacker = attacker2;
				}
				else if (_deckHeuristic.ScoreCard(_gameState.GetCardById(attacker2.AttackerInstanceId)) < _deckHeuristic.ScoreCard(_gameState.GetCardById(attacker.AttackerInstanceId)))
				{
					attacker = attacker2;
				}
			}
		}
		return attacker;
	}

	private void AttemptSubmitAttackers(DeclareAttackerRequest request)
	{
		if (_turnInformation.numberOfCallsToGREAutoResponder > 10)
		{
			request.Concede();
			return;
		}
		if (_turnInformation.numberOfAttemptsSubmitAttackers > 10)
		{
			_turnInformation.numberOfCallsToGREAutoResponder++;
			request.AutoRespond();
			return;
		}
		foreach (AttackWarning attackWarning in request.AttackWarnings)
		{
			if (attackWarning.Type == AttackWarningType.MustAttack)
			{
				foreach (Attacker attacker in request.Attackers)
				{
					if (attacker.AttackerInstanceId == attackWarning.InstanceId && attacker.SelectedDamageRecipient == null && attacker.LegalDamageRecipients.Count > 0)
					{
						attacker.SelectedDamageRecipient = attacker.LegalDamageRecipients[0];
						request.UpdateAttacker(attacker);
						return;
					}
				}
			}
			else if (attackWarning.Type == AttackWarningType.MustAttackWithAtLeastOne)
			{
				Attacker weakestAttacker = GetWeakestAttacker(request.Attackers);
				if (weakestAttacker != null)
				{
					weakestAttacker.SelectedDamageRecipient = weakestAttacker.LegalDamageRecipients[0];
					request.UpdateAttacker(weakestAttacker);
					return;
				}
			}
		}
		int num = ((request.ManaRequirements.Count > 0) ? SumManaCosts(request.ManaRequirements) : 0);
		if (num > 0)
		{
			int num2 = GetAvailableManaCount();
			List<Attacker> attackingCreatures = GetAttackingCreatures(request);
			int num3 = 0;
			int count = attackingCreatures.Count;
			int num4 = num / count;
			for (int i = 0; i < count; i++)
			{
				num2 -= num4;
				if (num2 < 0)
				{
					num3++;
				}
			}
			List<Attacker> list = new List<Attacker>();
			while (num3 > 0)
			{
				Attacker weakestAttacker2 = GetWeakestAttacker(attackingCreatures);
				if (weakestAttacker2 == null)
				{
					break;
				}
				weakestAttacker2.SelectedDamageRecipient = null;
				attackingCreatures.Remove(weakestAttacker2);
				list.Add(weakestAttacker2);
				num3--;
			}
			if (list.Count > 0)
			{
				request.UpdateAttacker(list.ToArray());
				return;
			}
		}
		if (request.DeclaredAttackers.Exists((Attacker attacker) => attacker.SelectedDamageRecipient != null))
		{
			_turnInformation.numberOfTurnsWithoutAttacks = 0;
		}
		else
		{
			_turnInformation.numberOfTurnsWithoutAttacks++;
		}
		_turnInformation.numberOfAttemptsSubmitAttackers++;
		request.SubmitAttackers();
	}

	private void AttemptSubmitBlockers(DeclareBlockersRequest request)
	{
		if (_turnInformation.numberOfCallsToGREAutoResponder > 10)
		{
			request.Concede();
			return;
		}
		if (_turnInformation.numberOfAttemptsSubmitBlockers > 10)
		{
			_turnInformation.numberOfCallsToGREAutoResponder++;
			request.AutoRespond();
			return;
		}
		foreach (BlockWarning blockWarning in request.BlockWarnings)
		{
			if (blockWarning.Type != BlockWarningType.InsufficientBlockers)
			{
				continue;
			}
			List<Blocker> list = new List<Blocker>();
			foreach (Blocker allBlocker in request.AllBlockers)
			{
				if (allBlocker.SelectedAttackerInstanceIds.Count == 0)
				{
					list.Add(allBlocker);
				}
			}
			Blocker blocker = null;
			foreach (Blocker item in list)
			{
				if (blocker == null)
				{
					blocker = item;
				}
			}
			if (blocker == null)
			{
				continue;
			}
			uint num = 0u;
			foreach (uint attackerInstanceId in blocker.AttackerInstanceIds)
			{
				MtgCardInstance cardById = _gameState.GetCardById(attackerInstanceId);
				if (!cardById.Abilities.Exists((AbilityPrintingData ability) => ability.Id == 1026) && !cardById.AttachedWithIds.Exists((uint refId) => _gameState.TryGetCard(refId, out var card) && card.GrpId == 69989))
				{
					if (num == 0)
					{
						num = attackerInstanceId;
					}
					else if (_deckHeuristic.ScoreCard(_gameState.GetCardById(attackerInstanceId)) < _deckHeuristic.ScoreCard(_gameState.GetCardById(num)))
					{
						num = attackerInstanceId;
					}
				}
			}
			if (blocker.SelectedAttackerInstanceIds.Count < blocker.MaxAttackers)
			{
				blocker.SelectedAttackerInstanceIds.Add(num);
			}
			request.UpdateBlockers(blocker);
			return;
		}
		_turnInformation.numberOfAttemptsSubmitBlockers++;
		request.SubmitBlockers();
	}

	private int SumManaCosts(IReadOnlyList<ManaRequirement> manaCosts)
	{
		int num = 0;
		foreach (ManaRequirement manaCost in manaCosts)
		{
			num += manaCost.Count;
		}
		return num;
	}

	private void StartIdleTimer(BaseUserRequest request, float maxIdleTime)
	{
		if (!(_botFamiliar == null))
		{
			StopIdleTimer();
			_idleTimerCoroutine = _botFamiliar.StartCoroutine(CO_RequestTimer(request, maxIdleTime));
		}
	}

	private void StopIdleTimer()
	{
		if (_idleTimerCoroutine != null && !(_botFamiliar == null))
		{
			_botFamiliar.StopCoroutine(_idleTimerCoroutine);
		}
	}
}
