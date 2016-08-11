//
//   Copyright (C) 2016 Fluendo S.A.
//
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAS.Core.Interfaces.GUI;

namespace VAS.Core
{
	public interface IDialogs
	{
		/* Messages */
		void InfoMessage (string message, object parent = null);

		void WarningMessage (string message, object parent = null);

		void ErrorMessage (string message, object parent = null);

		Task<bool> QuestionMessage (string question, string title, object parent = null);

		Task<string> QueryMessage (string key, string title = null, string value = "", object parent = null);

		Task<bool> NewVersionAvailable (Version currentVersion, Version latestVersion,
		                                string downloadURL, string changeLog, object parent = null);

		Task<object> ChooseOption (Dictionary<string, object> options, object parent = null);

		IBusyDialog BusyDialog (string message, object parent = null);

		/* Files/Folders IO */
		string SaveFile (string title, string defaultName, string defaultFolder,
		                 string filterName, string[] extensionFilter);

		string OpenFile (string title, string defaultName, string defaultFolder,
		                 string filterName = null, string[] extensionFilter = null);

		List<string> OpenFiles (string title, string defaultName, string defaultFolder,
		                        string filterName, string[] extensionFilter);

		string SelectFolder (string title, string defaultName, string defaultFolder,
		                     string filterName, string[] extensionFilter);

		int ButtonsMessage (string message, List<string> textButtons, int? focusIndex, object parent = null);

		/* Utils */
		Task<DateTime> SelectDate (DateTime date, object widget);

	}
}

