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
using System.ComponentModel;
using VAS.Core;
using VAS.Core.Resources;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;
using VAS.Core.Resources;

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
				iconImage = App.Current.ResourcesLocator.LoadImage (Images.ButtonTag);
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

		public override bool Active {
			get {
				return ViewModel.Active;
			}
			set {
				base.Active = value;
				ViewModel.Active = value;
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

		public override void ClickReleased ()
		{
			ViewModel.Toggle.Execute ();
			base.ClickReleased ();
		}

		protected override void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			base.HandlePropertyChanged (sender, e);
			if (ViewModel.NeedsSync (e.PropertyName, nameof (ViewModel.Active), sender, ButtonVM)) {
				ReDraw ();
			}
		}

	}
}
