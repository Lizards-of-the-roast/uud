using System.Collections;
using System.Collections.Generic;
using Core.Code.Decks;
using Core.Shared.Code;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga;
using Wotc.Mtgo.Gre.External.Messaging;

public class DeckCostsSummary : MonoBehaviour
{
	private class Bucket
	{
		public readonly Image Item;

		public uint TotalQuantity;

		public Bucket(Image item)
		{
			Item = item;
		}
	}

	public Image OneOrLessItem;

	public Image TwoItem;

	public Image ThreeItem;

	public Image FourItem;

	public Image FiveItem;

	public Image SixOrGreaterItem;

	private Coroutine _runningCoroutine;

	private GlobalCoroutineExecutor _globalCoroutine;

	private GlobalCoroutineExecutor GlobalCoroutine
	{
		get
		{
			if (!(_globalCoroutine != null))
			{
				return _globalCoroutine = Pantry.Get<GlobalCoroutineExecutor>();
			}
			return _globalCoroutine;
		}
	}

	private DeckBuilderModelProvider ModelProvider => Pantry.Get<DeckBuilderModelProvider>();

	private DeckBuilderVisualsUpdater VisualsUpdater => Pantry.Get<DeckBuilderVisualsUpdater>();

	public void OnEnable()
	{
		VisualsUpdater.PreUpdateAllDeckVisuals += OnPreUpdateAllDeckVisuals;
	}

	public void OnDisable()
	{
		VisualsUpdater.PreUpdateAllDeckVisuals -= OnPreUpdateAllDeckVisuals;
	}

	public void OnPreUpdateAllDeckVisuals()
	{
		SetDeck(ModelProvider.Model.GetFilteredMainDeck());
	}

	public void SetDeck(IReadOnlyList<CardPrintingQuantity> deck)
	{
		if (_runningCoroutine != null)
		{
			GlobalCoroutine.StopCoroutine(_runningCoroutine);
		}
		_runningCoroutine = GlobalCoroutine.StartCoroutine(SetDeckAfterActive(deck));
	}

	private IEnumerator SetDeckAfterActive(IReadOnlyList<CardPrintingQuantity> deck)
	{
		yield return new WaitUntil(() => base.gameObject != null && base.gameObject.activeInHierarchy);
		Dictionary<int, Bucket> dictionary = new Dictionary<int, Bucket>
		{
			{
				1,
				new Bucket(OneOrLessItem)
			},
			{
				2,
				new Bucket(TwoItem)
			},
			{
				3,
				new Bucket(ThreeItem)
			},
			{
				4,
				new Bucket(FourItem)
			},
			{
				5,
				new Bucket(FiveItem)
			},
			{
				6,
				new Bucket(SixOrGreaterItem)
			}
		};
		uint num = 0u;
		foreach (CardPrintingQuantity item in deck)
		{
			bool flag = false;
			bool flag2 = false;
			foreach (CardType type in item.Printing.Types)
			{
				if (type == CardType.Creature)
				{
					flag = true;
				}
				if (type == CardType.Land)
				{
					flag2 = true;
				}
			}
			if (!flag2 || flag)
			{
				Bucket bucket = dictionary[Mathf.Clamp((int)item.Printing.ConvertedManaCost, 1, 6)];
				bucket.TotalQuantity += item.Quantity;
				if (num < bucket.TotalQuantity)
				{
					num = bucket.TotalQuantity;
				}
			}
		}
		foreach (Bucket value in dictionary.Values)
		{
			value.Item.fillAmount = ((num == 0) ? 0f : ((float)value.TotalQuantity / (float)num));
		}
	}
}
