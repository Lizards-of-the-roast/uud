using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wotc.Mtga.CardParts;

[Serializable]
public class BattlefieldRegionDefinition
{
	public enum EAlignment
	{
		Left,
		Center,
		Right
	}

	[Serializable]
	public class LayoutVariant
	{
		[Tooltip("Dev-friendly name of the layout, for debugging purposes.")]
		public string Name;

		[Tooltip("The boundaries of the whole parent layout variant.")]
		public Rect Bounds = new Rect(-15f, -5f, 30f, 10f);

		[Tooltip("Does this region use paging once its cards don't fit inside its bounds?")]
		public bool UsesPaging;

		[Tooltip("The left paging arrow boundaries.")]
		public Rect LeftPagingArrow = new Rect(15f, -4f, 2f, 8f);

		[Tooltip("The right paging arrow boundaries.")]
		public Rect RightPagingArrow = new Rect(-17f, -4f, 2f, 8f);

		[Tooltip("Only use this layout variant if all the scaffold shapes supported in the sub regions are in the region.")]
		public bool RequiresAllSubRegionShapes;

		[Tooltip("Should each sub region be treated as a separate page when using paging?")]
		public bool TreatSubRegionsAsPages;

		[Tooltip("The ordered sub regions inside of this parent based on certain card shapes.")]
		[SerializeField]
		private List<SubRegion> _subRegions = new List<SubRegion>
		{
			new SubRegion()
		};

		private Dictionary<ScaffoldShape, SubRegion> _subRegionsDictionary;

		private int? _rowCount;

		public IReadOnlyList<SubRegion> SubRegions => _subRegions;

		public ICollection<ScaffoldShape> SupportedShapes
		{
			get
			{
				InitDictionary();
				return _subRegionsDictionary.Keys;
			}
		}

		public int RowCount
		{
			get
			{
				if (!_rowCount.HasValue)
				{
					_rowCount = _subRegions.Sum((SubRegion l) => l.RowCount);
				}
				return _rowCount.Value;
			}
		}

		public bool TryGetSingleLayout(out SubRegion layout)
		{
			if (_subRegions.Count == 1)
			{
				layout = _subRegions[0];
				return true;
			}
			layout = null;
			return false;
		}

		public bool TryGetLayout(ScaffoldShape key, out SubRegion layout)
		{
			InitDictionary();
			if (_subRegionsDictionary.TryGetValue(key, out layout))
			{
				return true;
			}
			if (key != ScaffoldShape.None && _subRegionsDictionary.TryGetValue(ScaffoldShape.None, out layout))
			{
				return true;
			}
			layout = null;
			return false;
		}

		private void InitDictionary()
		{
			if (_subRegionsDictionary != null)
			{
				return;
			}
			_subRegionsDictionary = new Dictionary<ScaffoldShape, SubRegion>();
			foreach (SubRegion subRegion in _subRegions)
			{
				if (!_subRegionsDictionary.ContainsKey(subRegion.Shape))
				{
					_subRegionsDictionary.Add(subRegion.Shape, subRegion);
				}
			}
		}
	}

	[Serializable]
	public class SubRegion
	{
		[Tooltip("Dev-friendly name of the layout, for debugging purposes.")]
		[SerializeField]
		private string _name = "Sub Region";

		[Tooltip("The card shape that this layout will be used for. (Using \"None\" means any shape can use this layout)")]
		[SerializeField]
		private ScaffoldShape _shape;

		[Tooltip("Maximum number of stacks that will fit in this region. [-1 Infinite]")]
		[SerializeField]
		private int _stackCountLimit = -1;

		[Tooltip("Will be added to the parent bounds to calculate the final child bounds.  If left at 0, it will use the parent bounds.")]
		[SerializeField]
		private Rect _boundsOffset = new Rect(0f, 0f, 0f, 0f);

		[Tooltip("Horizontal alignment of the cards in this layout.")]
		[SerializeField]
		private EAlignment _alignment = EAlignment.Center;

		[Tooltip("Number of horizontal rows that this region is broken into. Cards are balanced between rows.")]
		[SerializeField]
		private int _rowCount = 1;

		[Tooltip("Scale of CDCs that are housed in this region.")]
		[SerializeField]
		private float _cardScale = 1f;

		[Tooltip("Space between columns.")]
		[SerializeField]
		private float _horizontalGutter = 0.1f;

		[Tooltip("Space between rows.")]
		[SerializeField]
		private float _verticalGutter = 0.1f;

		[Tooltip("Minimum Stack Width (in units of CardWidth)")]
		[SerializeField]
		private float _minStackWidth = 1f;

		[Tooltip("Maximum Stack Width (in units of CardWidth)")]
		[SerializeField]
		private float _stackWidth = 1.5f;

		[Tooltip("Maximum number of cards that are shown stacked. [-1 Infinite]")]
		[SerializeField]
		private int _stackLimit = -1;

		[Tooltip("Minimum number of cards in a stack to show the [x00] indicator.")]
		[SerializeField]
		private int _stackCountDisplay = 2;

		[HideInInspector]
		[SerializeField]
		private string _key = Guid.NewGuid().ToString();

		[HideInInspector]
		public bool UsesSharedSubRegion;

		public string Name => _name;

		public ScaffoldShape Shape => _shape;

		public int StackCountLimit => _stackCountLimit;

		public Rect BoundsOffset => _boundsOffset;

		public EAlignment Alignment => _alignment;

		public int RowCount => _rowCount;

		public float CardScale => _cardScale;

		public float HorizontalGutter => _horizontalGutter;

		public float VerticalGutter => _verticalGutter;

		public float MinStackWidth => _minStackWidth;

		public float StackWidth => _stackWidth;

		public int StackLimit => _stackLimit;

		public int StackCountDisplay => _stackCountDisplay;

		public string Key
		{
			get
			{
				return _key;
			}
			set
			{
				_key = value;
			}
		}

		public Rect CalcBounds(Rect parentBounds)
		{
			return new Rect(BoundsOffset.x + parentBounds.x, BoundsOffset.y + parentBounds.y, BoundsOffset.width + parentBounds.width, BoundsOffset.height + parentBounds.height);
		}

		public void CopyFrom(SubRegion original)
		{
			_name = original.Name;
			_shape = original.Shape;
			_stackCountLimit = original.StackCountLimit;
			_boundsOffset = original.BoundsOffset;
			_alignment = original.Alignment;
			_rowCount = original.RowCount;
			_cardScale = original.CardScale;
			_horizontalGutter = original.HorizontalGutter;
			_verticalGutter = original.VerticalGutter;
			_minStackWidth = original._minStackWidth;
			_stackWidth = original.StackWidth;
			_stackLimit = original._stackLimit;
			_stackCountDisplay = original._stackCountDisplay;
			_key = original._key;
		}
	}

	public GREPlayerNum Owner;

	public BattlefieldRegionType Type;

	[Tooltip("Ordered list of layout variants.")]
	public LayoutVariant[] LayoutVariants = new LayoutVariant[0];

	[Tooltip("A set of sub regions that can be shared across multiple variants.")]
	[SerializeField]
	private List<SubRegion> _sharedSubRegions = new List<SubRegion>();

	public IReadOnlyList<SubRegion> SharedSubRegions => _sharedSubRegions;
}
