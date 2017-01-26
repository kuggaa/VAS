//
//  Copyright (C) 2017 Fluendo S.A.
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
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Templates;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// Class for the Dashboard ViewModel.
	/// </summary>
	public class DashboardVM : TemplateViewModel<Dashboard, DashboardButton, DashboardButtonVM>
	{
		DashboardMode mode;
		/// <summary>
		/// Gets or sets the icon.
		/// </summary>
		/// <value>The icon.</value>
		public override Image Icon {
			get {
				return Model.Image;
			}

			set {
				Model.Image = value;
			}
		}


		/// <summary>
		/// Gets or sets the mode.
		/// </summary>
		/// <value>The mode.</value>
		public DashboardMode Mode {
			get {
				return mode;
			}
			set {
				mode = value;
				foreach (var vm in ViewModels) {
					vm.Mode = mode;
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating to showing or not the links.
		/// </summary>
		/// <value><c>true</c> if show links; otherwise, <c>false</c>.</value>
		public bool ShowLinks {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the current time.
		/// </summary>
		/// <value>The current time.</value>
		public Time CurrentTime {
			get;
			set;
		} = new Time (0);

		/// <summary>
		/// Gets or sets the fit mode.
		/// </summary>
		/// <value>The fit mode.</value>
		public FitMode FitMode {
			get;
			set;
		}

		/// <summary>
		/// Gets the width of the canvas.
		/// </summary>
		/// <value>The width of the canvas.</value>
		public int CanvasWidth {
			get {
				return Model.CanvasWidth;
			}
		}

		/// <summary>
		/// Gets the height of the canvas.
		/// </summary>
		/// <value>The height of the canvas.</value>
		public int CanvasHeight {
			get {
				return Model.CanvasHeight;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.ViewModel.DashboardVM"/> disable popup window.
		/// </summary>
		/// <value><c>true</c> if disable popup window; otherwise, <c>false</c>.</value>
		public bool DisablePopupWindow {
			get {
				return Model.DisablePopupWindow;
			}
			set {
				Model.DisablePopupWindow = value;
			}
		}

		/// <summary>
		/// Sets a value indicating whether this <see cref="T:VAS.Core.ViewModel.DashboardVM"/> can edit plays.
		/// </summary>
		/// <value><c>true</c> if can edit; otherwise, <c>false</c>.</value>
		public bool EditPlays {
			get;
			set;
		}


		/// <summary>
		/// Creates the sub view model.
		/// </summary>
		/// <returns>The sub view model.</returns>
		public override CollectionViewModel<DashboardButton, DashboardButtonVM> CreateSubViewModel ()
		{
			return new DashboardButtonCollectionVM ();
		}
	}
}
