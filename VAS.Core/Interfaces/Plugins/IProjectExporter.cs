//
//  Copyright (C) 2016 Andoni Morales Alastruey
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
using System.Threading.Tasks;
using VAS.Addins.ExtensionPoints;
using VAS.Core.Store;

namespace VAS.Core.Interfaces.Plugins
{
	public interface IProjectExporter : IPlugin
	{
		/// <summary>
		/// Gets the description string used to display it as an option when there are several exporters.
		/// </summary>
		/// <value>The description.</value>
		string Description { get; }

		/// <summary>
		/// A unique identifier for the export format used to filter in the export event.
		/// </summary>
		/// <value>The format.</value>
		string Format { get; }

		/// <summary>
		/// Exports the project.
		/// </summary>
		/// <param name="project">Project.</param>
		Task Export (Project project, bool moveFileSet = false);
	}
}

