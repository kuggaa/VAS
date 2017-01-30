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
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Drawing.CanvasObjects.Dashboard
{
	/// <summary>
	/// Class for the TagButton View
	/// </summary>
	[ViewAttribute ("TagButtonView")]
	public class TagButtonView : DashboardButtonView, ICanvasObjectView<TagButtonVM>
	{
		static Image iconImage;
		TagButton tagButton;
		TagButtonVM viewModel;

		public TagButtonView () : base ()
		{
			Toggle = true;
			SupportsLinks = false;
			if (iconImage == null) {
				iconImage = Resources.LoadImage (StyleConf.ButtonTagIcon);
			}
		}

		/// <summary>
		/// Gets or sets the button.
		/// </summary>
		/// <value>The tag button.</value>
		public TagButton TagButton {
			get {
				return tagButton;
			}
			set {
				tagButton = value;
				Button = value;
			}
		}

		public override Image Icon {
			get {
				return iconImage;
			}
		}

		public override string Text {
			get {
				return TagButton.Tag.Value;
			}
		}

		/// <summary>
		/// Gets or sets the view model.
		/// </summary>
		/// <value>The view model.</value>
		public TagButtonVM ViewModel {
			get {
				return viewModel;
			}
			set {
				viewModel = value;
				if (viewModel != null) {
					TagButton = viewModel.Model;
				}
			}
		}

		/// <summary>
		/// Sets the view model.
		/// </summary>
		/// <param name="viewModel">View model.</param>
		public void SetViewModel (object viewModel)
		{
			ViewModel = (TagButtonVM)viewModel;
		}
	}
}
