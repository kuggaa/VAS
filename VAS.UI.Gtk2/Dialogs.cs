//
//   Copyright (C) 2016 Fluendo S.A.
//
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gtk;
using VAS.Core;
using VAS.Core.Interfaces.GUI;
using VAS.UI.Dialog;
using VAS.UI.Helpers;

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
				return ((GUIToolkit)App.Current.GUIToolkit).GetParentWidget (null) as Gtk.Window;
			}
		}

		#region IDialogs implementation

		public void InfoMessage (string message, object parent = null)
		{
			MessagesHelpers.InfoMessage (((GUIToolkit)App.Current.GUIToolkit).GetParentWidget (parent), message);
		}

		public void WarningMessage (string message, object parent = null)
		{
			MessagesHelpers.WarningMessage (((GUIToolkit)App.Current.GUIToolkit).GetParentWidget (parent), message);
		}

		public void ErrorMessage (string message, object parent = null)
		{
			MessagesHelpers.ErrorMessage (((GUIToolkit)App.Current.GUIToolkit).GetParentWidget (parent), message);
		}

		public Task<bool> QuestionMessage (string question, string title, object parent = null)
		{
			bool res = MessagesHelpers.QuestionMessage (((GUIToolkit)App.Current.GUIToolkit).GetParentWidget (parent), question, title);
			return AsyncHelpers.Return (res);
		}

		public Task<string> QueryMessage (string key, string title = null, string value = "", object parent = null)
		{
			string res = MessagesHelpers.QueryMessage (((GUIToolkit)App.Current.GUIToolkit).GetParentWidget (parent), key, title, value);
			return AsyncHelpers.Return (res);
		}

		public Task<bool> NewVersionAvailable (Version currentVersion, Version latestVersion, string downloadURL, string changeLog, object parent = null)
		{
			bool res = MessagesHelpers.NewVersionAvailable (currentVersion, latestVersion, downloadURL,
				           changeLog, ((GUIToolkit)App.Current.GUIToolkit).GetParentWidget (parent));
			return AsyncHelpers.Return (res);
		}

		public Task<object> ChooseOption (Dictionary<string, object> options, object parent = null)
		{
			object res = null;
			Window parentWindow;
			ChooseOptionDialog dialog; 

			if (parent != null) {
				parentWindow = (parent as Widget).Toplevel as Gtk.Window;
			} else {
				parentWindow = MainWindow;
			}

			dialog = new ChooseOptionDialog (parentWindow);
			dialog.Options = options;

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

		public string SaveFile (string title, string defaultName, string defaultFolder, string filterName, string[] extensionFilter)
		{
			return FileChooserHelper.SaveFile (MainWindow, title, defaultName,	
				defaultFolder, filterName, extensionFilter);
		}

		public string OpenFile (string title, string defaultName, string defaultFolder, string filterName = null, string[] extensionFilter = null)
		{
			return FileChooserHelper.OpenFile (MainWindow, title, defaultName,	
				defaultFolder, filterName, extensionFilter);
		}

		public List<string> OpenFiles (string title, string defaultName, string defaultFolder, string filterName, string[] extensionFilter)
		{
			return FileChooserHelper.OpenFiles (MainWindow,	title, defaultName,	
				defaultFolder, filterName, extensionFilter);
		}

		public string SelectFolder (string title, string defaultName, string defaultFolder, string filterName, string[] extensionFilter)
		{
			return FileChooserHelper.SelectFolder (MainWindow, title, defaultName,	
				defaultFolder, filterName, extensionFilter);
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
			return MessagesHelpers.ButtonsMessage (((GUIToolkit)App.Current.GUIToolkit).GetParentWidget (parent), 
				message, textButtons, focusIndex);
		}

		#endregion
	}
}

