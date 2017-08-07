//
//  Copyright (C) 2016 Fluendo S.A.
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
using System.IO;
using System.Threading.Tasks;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Plugins;
using VAS.Core.MVVMC;
using VAS.Core.Store;

namespace VAS.Plugins
{
	public abstract class ProjectExporterBase : IProjectExporter
	{
		public abstract string Name {
			get;
		}

		public abstract string Description {
			get;
		}

		public abstract string Format {
			get;
		}

		public abstract string Extension {
			get;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the exportation has been executed and completed.
		/// </summary>
		/// <value><c>true</c> if export done; otherwise, <c>false</c>.</value>
		public bool ExportDone { get; set; }

		protected abstract Task ExportProject (Project project, string filename);

		public async Task Export (Project project, bool moveFileSet = false)
		{
			IBusyDialog dialog;
			string filename = App.Current.Dialogs.SaveFile (Catalog.GetString ("Output file"),
															Utils.SanitizePath (project.ShortDescription + Extension),
															App.Current.Config.LastDir,
															Format, new [] { "*" + Extension });

			if (filename == null)
				return;

			filename = Path.ChangeExtension (filename, Extension);

			dialog = App.Current.Dialogs.BusyDialog (Catalog.GetString ("Exporting project..."));
			try {
				dialog.Show ();
				await ExportProject (project, filename);
				dialog.Destroy ();
				if (ExportDone) {
					App.Current.Dialogs.InfoMessage (Catalog.GetString ("Project exported successfully"));
				}
			} catch (Exception ex) {
				dialog.Destroy ();
				App.Current.Dialogs.ErrorMessage (Catalog.GetString ("Error exporting project"));
				Log.Exception (ex);
			}
		}
	}
}

