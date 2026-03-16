using System;
using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Network;
using GreClient.Rules;
using Newtonsoft.Json.Linq;
using Pooling;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

public class SetTest : BotBattleTest
{
	public class TestManager
	{
		public class SetTest
		{
			public class PrintingTest
			{
				public enum State
				{
					WishIt,
					PlayIt,
					PassIt,
					AbilityTest,
					Complete
				}

				public class AbilityTest
				{
					private readonly uint _id;

					private readonly uint _locId;

					private readonly IGreLocProvider _greLocProvider;

					public AbilityTest(AbilityPrintingData abilityPrintingData, IGreLocProvider greLocProvider)
					{
						_id = abilityPrintingData.Id;
						_locId = abilityPrintingData.TextId;
						_greLocProvider = greLocProvider;
					}

					public bool TryHandleRequest(ActionsAvailableRequest req)
					{
						Wotc.Mtgo.Gre.External.Messaging.Action action = req.Actions.Find((Wotc.Mtgo.Gre.External.Messaging.Action x) => x.AbilityGrpId == _id);
						if (action != null)
						{
							req.SubmitAction(action);
							return true;
						}
						return false;
					}

					public JObject ToJObject()
					{
						return new JObject
						{
							["Text"] = _greLocProvider.GetLocalizedText(_locId, "en-US", formatted: false),
							["Id"] = _id.ToString()
						};
					}
				}

				private const uint TOC_WISH_ABILITY_GRPID = 996u;

				private uint _grpId;

				private string _cardTitle = string.Empty;

				private List<AbilityTest> _testedAbilities = new List<AbilityTest>();

				private List<AbilityTest> _untestedAbilities = new List<AbilityTest>();

				private List<AbilityTest> _notCovered = new List<AbilityTest>();

				private State CurrentState;

				private MtgGameState _cachedGameState = new MtgGameState();

				public uint StallCount;

				public PrintingTest(CardPrintingData printingData, ICardDatabaseAdapter cardDatabase)
				{
					_grpId = printingData.GrpId;
					_cardTitle = cardDatabase.GreLocProvider.GetLocalizedText(printingData.TitleId);
					foreach (AbilityPrintingData ability in printingData.Abilities)
					{
						AbilityTest item = new AbilityTest(ability, cardDatabase.GreLocProvider);
						if (ability.Category == AbilityCategory.Activated && ability.RelevantZones.Count == 0)
						{
							_untestedAbilities.Add(item);
						}
						else
						{
							_notCovered.Add(item);
						}
					}
				}

				public void UpdateGameState(MtgGameState gameState)
				{
					if (CurrentState == State.PassIt && _cachedGameState.GameWideTurn != gameState.GameWideTurn)
					{
						CurrentState = ((_untestedAbilities.Count > 0) ? State.AbilityTest : State.Complete);
					}
					_cachedGameState = gameState;
				}

				public bool TryHandleRequest(ActionsAvailableRequest actionsAvailable)
				{
					switch (CurrentState)
					{
					case State.WishIt:
						if (!_cachedGameState.DecidingPlayer.IsLocalPlayer)
						{
							return false;
						}
						foreach (Wotc.Mtgo.Gre.External.Messaging.Action action in actionsAvailable.Actions)
						{
							if (action.AbilityGrpId == 996)
							{
								actionsAvailable.SubmitAction(action);
								return true;
							}
						}
						return false;
					case State.PlayIt:
						foreach (Wotc.Mtgo.Gre.External.Messaging.Action action2 in actionsAvailable.Actions)
						{
							if ((action2.ActionType == ActionType.Cast || action2.ActionType == ActionType.Play) && action2.GrpId == _grpId)
							{
								CurrentState = State.PassIt;
								actionsAvailable.SubmitAction(action2);
								return true;
							}
						}
						return false;
					case State.PassIt:
						if (actionsAvailable.CanPass)
						{
							actionsAvailable.SubmitPass();
							return true;
						}
						return false;
					case State.AbilityTest:
					{
						for (int i = 0; i < _untestedAbilities.Count; i++)
						{
							AbilityTest abilityTest = _untestedAbilities[i];
							if (abilityTest.TryHandleRequest(actionsAvailable))
							{
								_untestedAbilities.RemoveAt(i);
								_testedAbilities.Add(abilityTest);
								if (_untestedAbilities.Count == 0)
								{
									CurrentState = State.PassIt;
								}
								return true;
							}
						}
						return false;
					}
					case State.Complete:
						return false;
					default:
						return false;
					}
				}

				public bool TryHandleWishReq(SelectNRequest selectN)
				{
					if (CurrentState == State.WishIt)
					{
						CurrentState = State.PlayIt;
						selectN.SubmitSelection(_grpId);
						return true;
					}
					return false;
				}

				public bool IsStalled()
				{
					return StallCount > 5;
				}

				public bool IsComplete()
				{
					return CurrentState == State.Complete;
				}

				public JObject ToJObject()
				{
					JObject jObject = new JObject();
					jObject["CardTitle"] = _cardTitle;
					jObject["GrpId"] = _grpId;
					if (CurrentState != State.Complete || CurrentState != State.PassIt)
					{
						jObject["State"] = CurrentState.ToString();
					}
					jObject["Stalled"] = IsStalled();
					if (_testedAbilities.Count > 0)
					{
						JArray jArray = new JArray();
						foreach (AbilityTest testedAbility in _testedAbilities)
						{
							jArray.Add(testedAbility.ToJObject());
						}
						jObject["Tested"] = jArray;
					}
					if (_untestedAbilities.Count > 0)
					{
						JArray jArray2 = new JArray();
						foreach (AbilityTest untestedAbility in _untestedAbilities)
						{
							jArray2.Add(untestedAbility.ToJObject());
						}
						jObject["UnTested"] = jArray2;
					}
					if (_notCovered.Count > 0)
					{
						JArray jArray3 = new JArray();
						foreach (AbilityTest item in _notCovered)
						{
							jArray3.Add(item.ToJObject());
						}
						jObject["NotCovered"] = jArray3;
					}
					return jObject;
				}
			}

			private Queue<PrintingTest> _queuedTests = new Queue<PrintingTest>();

			private List<PrintingTest> _inProgressTests = new List<PrintingTest>();

			private List<PrintingTest> _completeTests = new List<PrintingTest>();

			public SetTest(List<CardPrintingData> printingData, ICardDatabaseAdapter cardDatabase)
			{
				foreach (CardPrintingData printingDatum in printingData)
				{
					_queuedTests.Enqueue(new PrintingTest(printingDatum, cardDatabase));
				}
			}

			public void UpdateGameState(MtgGameState mtgGameState)
			{
				foreach (PrintingTest queuedTest in _queuedTests)
				{
					queuedTest.UpdateGameState(mtgGameState);
				}
				foreach (PrintingTest inProgressTest in _inProgressTests)
				{
					inProgressTest.UpdateGameState(mtgGameState);
				}
			}

			public bool TryHandleActionsAvailable(ActionsAvailableRequest actionsAvailable)
			{
				for (int i = 0; i < _inProgressTests.Count; i++)
				{
					PrintingTest printingTest = _inProgressTests[i];
					if (printingTest.IsComplete())
					{
						_inProgressTests.RemoveAt(i);
						_completeTests.Add(printingTest);
						continue;
					}
					if (printingTest.TryHandleRequest(actionsAvailable))
					{
						printingTest.StallCount = 0u;
						return true;
					}
					printingTest.StallCount++;
				}
				while (_queuedTests.Count > 0)
				{
					PrintingTest printingTest2 = _queuedTests.Dequeue();
					_inProgressTests.Add(printingTest2);
					if (printingTest2.TryHandleRequest(actionsAvailable))
					{
						printingTest2.StallCount = 0u;
						return true;
					}
					printingTest2.StallCount++;
				}
				return false;
			}

			public bool TryHandleWish(SelectNRequest selectN)
			{
				for (int i = 0; i < _inProgressTests.Count; i++)
				{
					PrintingTest printingTest = _inProgressTests[i];
					if (printingTest.TryHandleWishReq(selectN))
					{
						printingTest.StallCount = 0u;
						return true;
					}
				}
				return false;
			}

			public bool IsStalled()
			{
				if (_queuedTests.Count == 0)
				{
					return _inProgressTests.TrueForAll((PrintingTest x) => x.IsStalled());
				}
				return false;
			}

			public bool IsComplete()
			{
				if (_queuedTests.Count == 0)
				{
					return _inProgressTests.Count == 0;
				}
				return false;
			}

			public JObject ToJObject()
			{
				JObject jObject = new JObject();
				jObject["Complete"] = IsComplete();
				jObject["Stalled"] = IsStalled();
				if (_queuedTests.Count > 0)
				{
					JArray jArray = new JArray();
					foreach (PrintingTest queuedTest in _queuedTests)
					{
						jArray.Add(queuedTest.ToJObject());
					}
					jObject["PendingTests"] = jArray;
				}
				if (_inProgressTests.Count > 0)
				{
					JArray jArray2 = new JArray();
					foreach (PrintingTest inProgressTest in _inProgressTests)
					{
						jArray2.Add(inProgressTest.ToJObject());
					}
					jObject["InProgressTests"] = jArray2;
				}
				if (_completeTests.Count > 0)
				{
					JArray jArray3 = new JArray();
					foreach (PrintingTest completeTest in _completeTests)
					{
						jArray3.Add(completeTest.ToJObject());
					}
					jObject["CompleteTests"] = jArray3;
				}
				return jObject;
			}
		}

		private Queue<SetTest> _pendingSetTests = new Queue<SetTest>();

		private List<SetTest> _inProgressTests = new List<SetTest>();

		private List<SetTest> _completeSetTests = new List<SetTest>();

		public TestManager(List<List<CardPrintingData>> setsToTest, ICardDatabaseAdapter cardDatabase)
		{
			foreach (List<CardPrintingData> item in setsToTest)
			{
				_pendingSetTests.Enqueue(new SetTest(item, cardDatabase));
			}
		}

		public bool IsComplete()
		{
			if (_pendingSetTests.Count == 0)
			{
				return _inProgressTests.TrueForAll((SetTest x) => x.IsStalled());
			}
			return false;
		}

		public void UpdateGameState(MtgGameState mtgGameState)
		{
			foreach (SetTest pendingSetTest in _pendingSetTests)
			{
				pendingSetTest.UpdateGameState(mtgGameState);
			}
			foreach (SetTest inProgressTest in _inProgressTests)
			{
				inProgressTest.UpdateGameState(mtgGameState);
			}
		}

		public bool TryHandleRequest(ActionsAvailableRequest req)
		{
			if (IsComplete() && req.CanPass)
			{
				req.SubmitPass();
				return true;
			}
			for (int i = 0; i < _inProgressTests.Count; i++)
			{
				SetTest setTest = _inProgressTests[i];
				if (setTest.IsComplete())
				{
					_inProgressTests.RemoveAt(i);
					_completeSetTests.Add(setTest);
				}
				else if (setTest.TryHandleActionsAvailable(req))
				{
					return true;
				}
			}
			while (_pendingSetTests.Count > 0)
			{
				SetTest setTest2 = _pendingSetTests.Dequeue();
				_inProgressTests.Add(setTest2);
				if (setTest2.TryHandleActionsAvailable(req))
				{
					return true;
				}
			}
			return false;
		}

		public bool TryHandleWish(SelectNRequest selectN)
		{
			for (int i = 0; i < _inProgressTests.Count; i++)
			{
				if (_inProgressTests[i].TryHandleWish(selectN))
				{
					return true;
				}
			}
			return false;
		}

		public JObject ToJObject()
		{
			JObject jObject = new JObject();
			jObject["Complete"] = IsComplete();
			if (_pendingSetTests.Count > 0)
			{
				JArray jArray = new JArray();
				foreach (SetTest pendingSetTest in _pendingSetTests)
				{
					jArray.Add(pendingSetTest.ToJObject());
				}
				jObject["PendingSets"] = jArray;
			}
			if (_inProgressTests.Count > 0)
			{
				JArray jArray2 = new JArray();
				foreach (SetTest inProgressTest in _inProgressTests)
				{
					jArray2.Add(inProgressTest.ToJObject());
				}
				jObject["InProgressSets"] = jArray2;
			}
			if (_completeSetTests.Count > 0)
			{
				JArray jArray3 = new JArray();
				foreach (SetTest completeSetTest in _completeSetTests)
				{
					jArray3.Add(completeSetTest.ToJObject());
				}
				jObject["CompleteSets"] = jArray3;
			}
			return jObject;
		}
	}

	public Dictionary<uint, TestManager> _testsBySeatId = new Dictionary<uint, TestManager>();

	private Dictionary<RequestType, List<string>> _requests = new Dictionary<RequestType, List<string>>();

	public SetTest(BotBattleDSConfig dsConfig, CardDatabase cardDatabase)
		: base(dsConfig, cardDatabase)
	{
		if (base.DsConfig.LocalPlayerCardsToTest.Count > 0)
		{
			List<List<CardPrintingData>> setsToTest = generatePrintingDataForTests(base.DsConfig.LocalPlayerCardsToTest);
			_testsBySeatId.Add(1u, new TestManager(setsToTest, _cardDatabase));
		}
		if (base.DsConfig.OpponentCardsToTest.Count > 0)
		{
			List<List<CardPrintingData>> setsToTest2 = generatePrintingDataForTests(base.DsConfig.OpponentCardsToTest);
			_testsBySeatId.Add(2u, new TestManager(setsToTest2, _cardDatabase));
		}
		IObjectPool objectPool = new ObjectPool();
		Random random = new Random();
		LocalPlayerStrategy = new BotBattle_SetTestStrategy(this, BotBattle_SetTestStrategy.CreateHandlers(this, cardDatabase, random, objectPool));
		OpponentStrategy = new BotBattle_SetTestStrategy(this, BotBattle_SetTestStrategy.CreateHandlers(this, cardDatabase, random, objectPool));
		List<List<CardPrintingData>> generatePrintingDataForTests(List<List<uint>> cardIds)
		{
			List<List<CardPrintingData>> list = new List<List<CardPrintingData>>();
			foreach (List<uint> cardId in cardIds)
			{
				if (cardId != null)
				{
					List<CardPrintingData> list2 = new List<CardPrintingData>();
					foreach (uint item in cardId)
					{
						CardPrintingData cardPrintingById = _cardDatabase.CardDataProvider.GetCardPrintingById(item);
						if (cardPrintingById != null)
						{
							list2.Add(cardPrintingById);
						}
					}
					list.Add(list2);
				}
			}
			return list;
		}
	}

	public void UpdateGameState(MtgGameState mtgGameState)
	{
		foreach (uint key in _testsBySeatId.Keys)
		{
			_testsBySeatId[key].UpdateGameState(mtgGameState);
		}
	}

	public void LogRequest(BaseUserRequest req)
	{
		RequestType type = req.Type;
		List<string> value = null;
		if (_requests.TryGetValue(type, out value))
		{
			if (value.Count < 3)
			{
				value.Add(req.ToString());
			}
		}
		else
		{
			_requests.Add(type, new List<string> { req.ToString() });
		}
	}

	public bool TryHandleSelectActionRequest(ActionsAvailableRequest req)
	{
		if (req.OriginalMessage.SystemSeatIds.Count == 1)
		{
			uint key = req.OriginalMessage.SystemSeatIds[0];
			TestManager value = null;
			if (_testsBySeatId.TryGetValue(key, out value))
			{
				return value.TryHandleRequest(req);
			}
		}
		return false;
	}

	public bool TryHandleWish(SelectNRequest req)
	{
		if (!req.IsWishSelection)
		{
			return false;
		}
		if (req.OriginalMessage.SystemSeatIds.Count == 1)
		{
			uint key = req.OriginalMessage.SystemSeatIds[0];
			if (_testsBySeatId.TryGetValue(key, out var value))
			{
				return value.TryHandleWish(req);
			}
		}
		return false;
	}

	public override JObject ToJObject()
	{
		JObject jObject = new JObject();
		if (_testsBySeatId.TryGetValue(1u, out var value))
		{
			jObject["LocalPlayer"] = value.ToJObject();
		}
		if (_testsBySeatId.TryGetValue(2u, out var value2))
		{
			jObject["Opponent"] = value2.ToJObject();
		}
		if (_requests.Count > 0)
		{
			JArray jArray = new JArray();
			foreach (RequestType key in _requests.Keys)
			{
				JObject jObject2 = new JObject();
				jObject2["Type"] = key.ToString();
				JArray jArray2 = new JArray();
				foreach (string item in _requests[key])
				{
					jArray2.Add(item);
				}
				jObject2["Logs"] = jArray2;
				jArray.Add(jObject2);
			}
			jObject["Interactions"] = jArray;
		}
		return jObject;
	}

	public override GreClient.Network.DeckConfig GetLocalPlayerDeck()
	{
		List<uint> list = new List<uint>();
		for (int i = 0; i < 60; i++)
		{
			list.Add(66505u);
		}
		return new GreClient.Network.DeckConfig(string.Empty, list, Array.Empty<uint>(), Array.Empty<uint>(), 0u);
	}

	public override GreClient.Network.DeckConfig GetOpponentDeck()
	{
		List<uint> list = new List<uint>();
		for (int i = 0; i < 60; i++)
		{
			list.Add(66505u);
		}
		return new GreClient.Network.DeckConfig(string.Empty, list, Array.Empty<uint>(), Array.Empty<uint>(), 0u);
	}

	public override bool IsComplete()
	{
		foreach (uint key in _testsBySeatId.Keys)
		{
			if (!_testsBySeatId[key].IsComplete())
			{
				return false;
			}
		}
		return true;
	}
}
