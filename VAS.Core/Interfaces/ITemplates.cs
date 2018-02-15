// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using VAS.Core.Common;

namespace VAS.Core.Interfaces
{
	public interface ITemplate : IStorable, INotifyPropertyChanged
	{
		/// <summary>
		/// Gets or sets the name of the template.
		/// </summary>
		/// <value>The name.</value>
		string Name { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.Interfaces.ITemplate"/> is not editable.
		/// </summary>
		/// <value><c>true</c> if not editable; otherwise, <c>false</c>.</value>
		bool Static { get; set; }

		/// <summary>
		/// Gets or sets the version of the data used to create this template.
		/// </summary>
		/// <value>The version.</value>
		int Version { get; set; }

		/// <summary>
		/// Gets or sets the template preview.
		/// </summary>
		/// <value>The preview template.</value>
		Image Preview { get; set; }

		/// <summary>
		/// Creates a deep copy of the recipe changing the ID's of the storables too.
		/// </summary>
		/// <param name="newName">New name.</param>
		ITemplate Copy (string newName);
	}

	public interface ITemplate<TChild> : ITemplate
	{
		/// <summary>
		/// Gets the list of <typeparamref name="TChild"/> children in the template.
		/// </summary>
		/// <value>The list.</value>
		RangeObservableCollection<TChild> List { get; }
	}

	public interface ITemplateProvider : IService
	{
		bool Exists (string name);

	}

	public interface ITemplateProvider<T> : INotifyCollectionChanged, ITemplateProvider where T : ITemplate
	{
		/// <summary>
		/// Gets a list of all the templates in this provider.
		/// </summary>
		/// <value>The templates.</value>
		List<T> Templates { get; }

		/// <summary>
		/// Loads a template from a file.
		/// </summary>
		/// <returns>The file.</returns>
		/// <param name="filename">Filename.</param>
		T LoadFile (string filename);

		/// <summary>
		/// Save a template adding it to the database or updating an existing one.
		/// </summary>
		/// <param name="template">The template to save.</param>
		void Save (T template);

		/// <summary>
		/// Delete an existing template.
		/// </summary>
		/// <param name="template">The template to delete.</param>
		void Delete (T template);

		/// <summary>
		/// Register a new template that is not part of the user database, like the default templates.
		/// </summary>
		/// <param name="template">The template to add.</param>
		void Register (T template);

		/// <summary>
		/// Add a new template to the provider.
		/// </summary>
		/// <param name="template">Template.</param>
		void Add (T template);

		/// <summary>
		/// Create a new template copying an existing one with a different name.
		/// </summary>
		/// <returns>The copied template.</returns>
		/// <param name="template">Template to copy.</param>
		/// <param name="copy">Name of the copy.</param>
		T Copy (T template, string copy);

		/// <summary>
		/// Creates a new template.
		/// </summary>
		/// <param name="templateName">Template name.</param>
		/// <param name="count">Number of child items to create.</param>
		T Create (string templateName, int count = 0);

		/// <summary>
		/// Gets or sets the active db.
		/// </summary>
		/// <value>The active db.</value>
		IStorage Storage { get; }
	}
}

