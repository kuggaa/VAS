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
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;

namespace VAS.Drawing.CanvasObjects.Dashboard
{
	/// <summary>
	/// Class for the TagButton View
	/// </summary>
	[View ("TagButtonView")]
	public class TagButtonView : DashboardButtonView, ICanvasObjectView<TagButtonVM>
	{
		static Image iconImage;

		public TagButtonView () : base ()
		{
			Toggle = true;
			SupportsLinks = false;
			if (iconImage == null) {
				iconImage = App.Current.ResourcesLocator.LoadImage (StyleConf.ButtonTagIcon);
			}
		}

		public override Image Icon {
			get {
				return iconImage;
			}
		}

		public override string Text {
			get {
				return ViewModel.Tag.Value;
			}
		}

		/// <summary>
		/// Gets or sets the view model.
		/// </summary>
		/// <value>The view model.</value>
		public TagButtonVM ViewModel {
			get {
				return ButtonVM as TagButtonVM;
			}
			set {
				ButtonVM = value;
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
