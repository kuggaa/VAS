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
using System.Collections.Generic;
using System.Threading.Tasks;
using Gtk;
using VAS.Core;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store;
using VAS.UI.Dialog;
using VAS.UI.Helpers;
using Image = VAS.Core.Common.Image;

namespace VAS.UI
{
	/// <summary>
	/// Gtk2 IDialogs implementation
	/// </summary>
	public class Dialogs : IDialogs
	{
		static readonly IDialogs instance = new Dialogs ();

		public static IDialogs Instance {
			get {
				return instance;
			}
		}

		Gtk.Window MainWindow {
			get {
				return GetParentWidget (null) as Gtk.Window;
			}
		}

		#region IDialogs implementation

		public void InfoMessage (string message, object parent = null)
		{
			MessagesHelpers.InfoMessage (GetParentWidget (parent), message);
		}

		public void WarningMessage (string message, object parent = null)
		{
			MessagesHelpers.WarningMessage (GetParentWidget (parent), message);
		}

		public void ErrorMessage (string message, object parent = null)
		{
			MessagesHelpers.ErrorMessage (GetParentWidget (parent), message);
		}

		public Task<bool> QuestionMessage (string question, string title, object parent = null)
		{
			bool res = MessagesHelpers.QuestionMessage (GetParentWidget (parent), question, title);
			return AsyncHelpers.Return (res);
		}

		public Task<string> QueryMessage (string key, string title = null, string value = "", object parent = null)
		{
			string res = MessagesHelpers.QueryMessage (GetParentWidget (parent), key, title, value);
			return AsyncHelpers.Return (res);
		}

		public Task<bool> NewVersionAvailable (Version currentVersion, Version latestVersion, string downloadURL, string changeLog, object parent = null)
		{
			bool res = MessagesHelpers.NewVersionAvailable (currentVersion, latestVersion, downloadURL,
						   changeLog, GetParentWidget (parent));
			return AsyncHelpers.Return (res);
		}

		public Task<object> ChooseOption (Dictionary<string, object> options, string title = null, object parent = null)
		{
			object res = null;
			Window parentWindow;
			ChooseOptionDialog dialog;

			if (title == null) {
				title = Catalog.GetString ("Choose option");
			}

			if (parent != null) {
				parentWindow = (parent as Widget).Toplevel as Gtk.Window;
			} else {
				parentWindow = MainWindow;
			}

			dialog = new ChooseOptionDialog (parentWindow);
			dialog.Options = options;
			dialog.Title = title;

			if (dialog.Run () == (int)ResponseType.Ok) {
				res = dialog.SelectedOption;
			}
			dialog.Destroy ();
			var task = Task.Factory.StartNew (() => res);
			return task;
		}

		public IBusyDialog BusyDialog (string message, object parent = null)
		{
			BusyDialog dialog;
			Window parentWindow;

			if (parent != null) {
				parentWindow = (parent as Widget).Toplevel as Gtk.Window;
			} else {
				parentWindow = MainWindow;
			}
			dialog = new BusyDialog (parentWindow);
			dialog.Message = message;
			return dialog;
		}

		public string SaveFile (string title, string defaultName, string defaultFolder, string filterName, string [] extensionFilter)
		{
			return FileChooserHelper.SaveFile (MainWindow, title, defaultName,
				defaultFolder, filterName, extensionFilter);
		}

		public string OpenFile (string title, string defaultName, string defaultFolder, string filterName = null, string [] extensionFilter = null)
		{
			return FileChooserHelper.OpenFile (MainWindow, title, defaultName, defaultFolder, filterName, extensionFilter);
		}

		public List<string> OpenFiles (string title, string defaultName, string defaultFolder, string filterName, string [] extensionFilter)
		{
			return FileChooserHelper.OpenFiles (MainWindow, title, defaultName, defaultFolder, filterName, extensionFilter);
		}

		public string SelectFolder (string title, string defaultName, string defaultFolder, string filterName, string [] extensionFilter)
		{
			return FileChooserHelper.SelectFolder (MainWindow, title, defaultName, defaultFolder, filterName, extensionFilter);
		}

		public Task<DateTime> SelectDate (DateTime date, object widget)
		{
			CalendarDialog dialog = new CalendarDialog (date);
			dialog.TransientFor = (widget as Widget).Toplevel as Gtk.Window;
			dialog.Run ();
			date = dialog.Date;
			dialog.Destroy ();
			var task = AsyncHelpers.Return (date);
			return task;
		}

		public int ButtonsMessage (string message, List<string> textButtons, int? focusIndex, object parent = null)
		{
			return MessagesHelpers.ButtonsMessage (GetParentWidget (parent), message, textButtons, focusIndex);
		}

		#endregion

		Widget GetParentWidget (object parent = null)
		{
			return ((GUIToolkitBase)App.Current.GUIToolkit).GetParentWidget (parent);
		}

		public MediaFile OpenMediaFile (object parent)
		{
			return Helpers.Misc.OpenFile (parent);
		}

		public Task<Image> OpenImage (object parent = null)
		{
			return Task.FromResult (Helpers.Misc.OpenImage (parent as Widget));
		}
	}
}

