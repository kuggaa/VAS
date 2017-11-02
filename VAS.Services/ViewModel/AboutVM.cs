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
using VAS.Core.MVVMC;

namespace VAS.Services.ViewModel
{
	/// <summary>
	/// About ViewModel, is needed because of AboutState requires a ViewModel
	/// </summary>
	public class AboutVM : ViewModelBase
	{
		Version version = App.Current.Version;

		public AboutVM ()
		{
			ProgramName = App.Current.SoftwareName;
			Version = $"{version.Major}.{version.Minor}.{version.Build}";
			Copyright = App.Current.Copyright;
			Website = App.Current.Website;
			License = App.Current.License;
			Authors = new string [] { "Andoni Morales Alastruey", "Fluendo" };
			TranslatorCredits = App.Current.Translators;
			WrapLicense = true;
		}

		public string ProgramName { get; set; }
		public string Version { get; set; }
		public string Copyright { get; set; }
		public string Website { get; set; }
		public string License { get; set; }
		public string [] Authors { get; set; }
		public string TranslatorCredits { get; set; }
		public bool WrapLicense { get; set; }
	}
}
