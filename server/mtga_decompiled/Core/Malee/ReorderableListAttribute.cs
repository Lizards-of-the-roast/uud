using System;
using UnityEngine;

namespace Malee;

public class ReorderableListAttribute : PropertyAttribute
{
	public bool add;

	public bool remove;

	public bool draggable;

	public bool singleLine;

	public bool paginate;

	public bool sortable;

	public int pageSize;

	public string elementNameProperty;

	public string elementNameOverride;

	public string elementIconPath;

	public Type surrogateType;

	public string surrogateProperty;

	public ReorderableListAttribute()
		: this(null)
	{
	}

	public ReorderableListAttribute(string elementNameProperty)
		: this(add: true, remove: true, draggable: true, elementNameProperty)
	{
	}

	public ReorderableListAttribute(string elementNameProperty, string elementIconPath)
		: this(add: true, remove: true, draggable: true, elementNameProperty, null, elementIconPath)
	{
	}

	public ReorderableListAttribute(string elementNameProperty, string elementNameOverride, string elementIconPath)
		: this(add: true, remove: true, draggable: true, elementNameProperty, elementNameOverride, elementIconPath)
	{
	}

	public ReorderableListAttribute(bool add, bool remove, bool draggable, string elementNameProperty = null, string elementIconPath = null)
		: this(add, remove, draggable, elementNameProperty, null, elementIconPath)
	{
	}

	public ReorderableListAttribute(bool add, bool remove, bool draggable, string elementNameProperty = null, string elementNameOverride = null, string elementIconPath = null)
	{
		this.add = add;
		this.remove = remove;
		this.draggable = draggable;
		this.elementNameProperty = elementNameProperty;
		this.elementNameOverride = elementNameOverride;
		this.elementIconPath = elementIconPath;
		sortable = true;
	}
}
