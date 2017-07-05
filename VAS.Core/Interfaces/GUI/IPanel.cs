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
using VAS.Core.Interfaces.MVVMC;

namespace VAS.Core.Interfaces.GUI
{
	public interface IPanel : IView, IKeyContext
	{
		/// <summary>
		/// Gets the title of the panel.
		/// </summary>
		/// <value>The title.</value>
		string Title { get; }

		/// <summary>
		/// Called when the IPanel is loaded and going to be presented on screen.
		/// </summary>
		void OnLoad ();

		/// <summary>
		/// Called when the IPanel is presented on the screen and is going to be removed.
		/// </summary>
		void OnUnload ();
	}

	public interface IPanel<TViewModel> : IPanel, IView<TViewModel> where TViewModel : IViewModel
	{
	}

	public interface IPanelTab : IPanel
	{
		string StateName { get; }
	}

	public interface IPanelTab<TViewModel> : IPanelTab, IPanel<TViewModel> where TViewModel : IViewModel
	{
	}
}
