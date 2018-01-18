//
//  Copyright (C) 2014 Andoni Morales Alastruey
//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store.Drawables;

namespace VAS.Drawing
{

	/// <summary>
	/// A canvas object that acts as a container for other canvas objects taking care of forwarding events and calls
	/// to each of its children.
	/// </summary>
	public class CanvasContainer : CanvasObject, ICanvasSelectableObject, INotifyCollectionChanged,
	ICollection<ICanvasObject>, IEnumerable<ICanvasObject>
	{
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		RangeObservableCollection<ICanvasObject> children;

		public CanvasContainer ()
		{
			children = new RangeObservableCollection<ICanvasObject> ();
			children.CollectionChanged += HandleChildrenCollectionChanged;
		}

		protected override void DisposeManagedResources ()
		{
			Clear ();
		}

		public int Count => children.Count;

		public bool IsReadOnly => true;

		public ICanvasObject this [int index] => children [index];

		public IEnumerator<ICanvasObject> GetEnumerator () => children.GetEnumerator ();

		public int IndexOf (ICanvasObject child) => children.IndexOf (child);

		public bool Remove (ICanvasObject co) => RemoveChild (co, true);

		public bool Contains (ICanvasObject item) => children.Contains (item);

		public void CopyTo (ICanvasObject [] array, int arrayIndex) => children.CopyTo (array, arrayIndex);

		public void Add (ICanvasObject child)
		{
			children.Add (child);
			child.RedrawEvent += EmitRedrawEvent;
		}

		protected void Insert (int index, ICanvasObject child)
		{
			children.Insert (index, child);
			child.RedrawEvent += EmitRedrawEvent;
		}

		public void Clear ()
		{
			foreach (var child in children) {
				RemoveChild (child, false);
			}
			children.Clear ();
		}

		public virtual Selection GetSelection (Point point, double precision, bool inMotion = false)
		{
			Selection selection = null;

			foreach (ICanvasSelectableObject child in children.OfType<ICanvasSelectableObject> ()) {
				Selection tmp;
				if (!child.Visible)
					continue;
				tmp = child.GetSelection (point, precision);
				if (tmp == null) {
					continue;
				}
				if (tmp.Accuracy == 0) {
					selection = tmp;
					break;
				}
				if (selection == null || tmp.Accuracy < selection.Accuracy) {
					selection = tmp;
				}
			}
			return selection;
		}

		public virtual void Move (Selection s, Point dst, Point start)
		{
			throw new NotImplementedException ($"Move not supported for this {GetType ()}");
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			foreach (ICanvasObject co in children) {
				co.Draw (tk, area);
			}
		}

		IEnumerator IEnumerable.GetEnumerator () => children.GetEnumerator ();

		bool RemoveChild (ICanvasObject co, bool full)
		{
			bool ret = false;

			co.RedrawEvent -= EmitRedrawEvent;
			if (full) {
				ret = children.Remove (co);
			}
			co.Dispose ();
			return ret;
		}

		void HandleChildrenCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			CollectionChanged?.Invoke (this, e);
		}
	}
}