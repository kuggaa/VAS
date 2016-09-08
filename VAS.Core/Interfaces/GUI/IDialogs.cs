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

		Task<object> ChooseOption (Dictionary<string, object> options, string title = null, object parent = null);

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

