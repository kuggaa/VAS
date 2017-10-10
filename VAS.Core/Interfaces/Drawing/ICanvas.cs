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
using VAS.Core.Common;
using VAS.Core.Handlers.Drawing;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Store.Drawables;

namespace VAS.Core.Interfaces.Drawing
{
	public interface ICanvas : IDisposable
	{
		void Draw (IContext context, Area area);

		void SetWidget (IWidget widget);
	}

	public interface ICanvasObject : IDisposable, IVisible
	{
		event CanvasHandler ClickedEvent;
		event RedrawHandler RedrawEvent;

		void Draw (IDrawingToolkit tk, Area area);

		string Description { set; get; }

		void ClickPressed (Point p, ButtonModifier modif, Selection selection);

		void ClickReleased ();
	}

	public interface ICanvasSelectableObject : ICanvasObject, IMovableObject
	{
	}

	public interface ICanvasDrawableObject : ICanvasSelectableObject
	{
		IBlackboardObject IDrawableObject {
			get;
			set;
		}
	}

	/// <summary>
	/// Interface for canvas widgets that are a View.
	/// </summary>
	public interface ICanvasView : ICanvas, IView
	{
	}

	/// <summary>
	/// Generic Interface for canvas widgets that are a View.
	/// </summary>
	public interface ICanvasView<TViewModel> : ICanvasView, IView<TViewModel>
		where TViewModel : IViewModel
	{
	}

	/// <summary>
	/// Interface for canvas objects that are a View.
	/// </summary>
	public interface ICanvasObjectView : ICanvasObject, IView
	{
	}

	/// <summary>
	/// Generic Interface for canvas objects that are a View.
	/// </summary>
	public interface ICanvasObjectView<TViewModel> : ICanvasObjectView, IView<TViewModel>
		where TViewModel : IViewModel
	{
	}

}

