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
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;
using VAS.Services.State;
using VAS.Services.ViewModel;
using VAS.UI.Helpers.Bindings;

namespace VAS.UI.Dialog
{
	/// <summary>
	/// About dialog that shows information such as ProgramName, Copyright, Version, Authors, TranslatorCredits
	/// and allows to view your current License and program Website 
	/// </summary>
	[ViewAttribute (AboutState.NAME)]
	public class AboutDialog : Gtk.AboutDialog, IPanel<AboutVM>
	{
		BindingContext bindingContext;
		AboutVM viewModel;

		public AboutDialog ()
		{
			Response += HandleResponse;
			SetBindings ();
			Show ();
		}

		/// <summary>
		/// Gets or sets the view model.
		/// </summary>
		/// <value>The about view model.</value>
		public AboutVM ViewModel {
			get {
				return viewModel;
			}

			set {
				viewModel = value;
				if (viewModel != null) {
					bindingContext.UpdateViewModel (viewModel);
					SetUrlHook ((dialog, url) => {
						try {
							System.Diagnostics.Process.Start (url);
						} catch (Exception exception) {
							Log.Error ($"There was an error opening {url} on the browser. {exception}");
						}
					});
				}
			}
		}
		/// <summary>
		/// Gets the key context.
		/// </summary>
		/// <returns>The key context.</returns>
		public KeyContext GetKeyContext ()
		{
			return new KeyContext ();
		}

		public void OnLoad ()
		{

		}

		public void OnUnload ()
		{

		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (AboutVM)viewModel;
		}

		void SetBindings ()
		{
			bindingContext = this.GetBindingContext ();

			bindingContext.Add (this.Bind (vm => ((AboutVM)vm).ProgramName, v => ((AboutDialog)v).ProgramName));
			bindingContext.Add (this.Bind (vm => ((AboutVM)vm).Version, v => ((AboutDialog)v).Version));
			bindingContext.Add (this.Bind (vm => ((AboutVM)vm).Copyright, v => ((AboutDialog)v).Copyright));
			bindingContext.Add (this.Bind (vm => ((AboutVM)vm).Website, v => ((AboutDialog)v).Website));
			bindingContext.Add (this.Bind (vm => ((AboutVM)vm).License, v => ((AboutDialog)v).License));
			bindingContext.Add (this.Bind (vm => ((AboutVM)vm).Authors, v => ((AboutDialog)v).Authors));
			bindingContext.Add (this.Bind (vm => ((AboutVM)vm).TranslatorCredits, v => ((AboutDialog)v).TranslatorCredits));
		}

		void HandleResponse (object o, ResponseArgs args)
		{
			if (args.ResponseId == ResponseType.Cancel || args.ResponseId == ResponseType.Close) {
				Destroy ();
			}
		}
	}
}
