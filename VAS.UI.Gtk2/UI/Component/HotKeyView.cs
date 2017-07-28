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
using Gtk;
using VAS.Core.Common;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;
using VAS.UI.Helpers;
using VAS.UI.Helpers.Bindings;

namespace VAS.UI.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class HotKeyView : Gtk.Bin, IView<HotKeyVM>
	{
		BindingContext ctx;
		Label hotkeyLabel;
		HotKeyVM viewModel;

		public HotKeyView ()
		{
			this.Build ();
			hotkeyLabel = new Label ();
			hotKeyButton.Add (hotkeyLabel);
			hotkeyLabel.Show ();
			Bind ();
			ButtonHelper.ApplyStyleDialog (hotKeyButton);
		}

		public override void Dispose ()
		{
			Dispose (true);
			base.Dispose ();
		}

		protected virtual void Dispose (bool disposing)
		{
			if (Disposed) {
				return;
			}
			if (disposing) {
				Destroy ();
			}
			Disposed = true;
		}

		protected override void OnDestroyed ()
		{
			Log.Verbose ($"Destroying {GetType ()}");

			ctx.Dispose ();
			ctx = null;
			ViewModel.Dispose ();
			ViewModel = null;
			hotkeyLabel.Dispose ();
			hotkeyLabel = null;

			base.OnDestroyed ();

			Disposed = true;
		}

		protected bool Disposed { get; private set; } = false;


		/// <summary>
		/// Gets or sets the view model.
		/// </summary>
		/// <value>The view model.</value>
		public HotKeyVM ViewModel {
			get {
				return viewModel;
			}
			set {
				viewModel = value;
				ctx.UpdateViewModel (viewModel);
				if (viewModel != null) {
					viewModel.Sync ();
				}
			}
		}

		/// <summary>
		/// Sets the view model.
		/// </summary>
		/// <param name="viewModel">View model.</param>
		public void SetViewModel (object viewModel)
		{
			ViewModel = (HotKeyVM)viewModel;
		}

		/// <summary>
		/// Bind this instance.
		/// </summary>
		void Bind ()
		{
			ctx = this.GetBindingContext ();
			ctx.Add (hotKeyButton.Bind (vm => ((HotKeyVM)vm).UpdateHotkeyCommand));
			ctx.Add (hotkeyLabel.Bind (vm => ((HotKeyVM)vm).Name));
		}
	}
}
