//
//  Copyright (C) 2016 Fluendo S.A.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using VAS.Core.Common;
using VAS.Core.Filters;
using VAS.Core.Interfaces;
using VAS.Core.Store.Templates;
using VAS.DB;

namespace VAS.Services
{
	public class TemplatesProvider<T> : ITemplateProvider<T>
		where T : ITemplate<T>
	{
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		readonly MethodInfo methodDefaultTemplate;
		protected List<T> systemTemplates;
		IStorage storage;

		public TemplatesProvider (IStorage storage)
		{
			methodDefaultTemplate = typeof (T).GetMethod ("DefaultTemplate");
			systemTemplates = new List<T> ();
			this.storage = storage;
		}

		public bool Exists (string name)
		{
			if (systemTemplates.Any (t => t.Name == name)) {
				return true;
			}
			QueryFilter filter = new QueryFilter ();
			filter.Add ("Name", name);
			return storage.Retrieve<T> (filter).Any ();
		}

		/// <summary>
		/// Gets the templates.
		/// </summary>
		/// <value>The templates.</value>
		public List<T> Templates {
			get {
				List<T> templates = storage.RetrieveAll<T> ().OrderBy (t => t.Name).ToList ();
				// Now add the system templates, use a copy to prevent modification of system templates.
				foreach (T stemplate in systemTemplates) {
					T clonedTemplate = stemplate.Clone ();
					clonedTemplate.Static = true;
					templates.Add (clonedTemplate);
				}
				return templates;
			}
		}

		public T LoadFile (string filename)
		{
			Log.Information ("Loading template file " + filename);
			T template = FileStorage.RetrieveFrom<T> (filename);
			return template;
		}

		public void Save (T template)
		{
			bool isNew = false;

			CheckInvalidChars (template.Name);
			Log.Information ("Saving template " + template.Name);
			if (storage.Retrieve<T> (template.ID) == null) {
				isNew = true;
			}
			try {
				storage.Store<T> (template, true);
				if (isNew && CollectionChanged != null) {
					CollectionChanged (this,
						new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, template));
				}
			} catch (StorageException ex) {
				App.Current.Dialogs.ErrorMessage (ex.Message);
			}
		}

		public void Register (T template)
		{
			Log.Information ("Registering new template " + template.Name);
			template.Static = true;
			systemTemplates.Add (template);
			if (CollectionChanged != null) {
				CollectionChanged (this,
					new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, template));
			}
		}

		public void Add (T template)
		{
			Log.Information ("Adding new template " + template.Name);
			try {
				NotifyCollectionChangedAction action = NotifyCollectionChangedAction.Add;

				if (storage.Retrieve<T> (template.ID) != null) {
					action = NotifyCollectionChangedAction.Replace;
				}
				storage.Store (template, true);
				if (CollectionChanged != null) {
					CollectionChanged (this,
						new NotifyCollectionChangedEventArgs (action, template));
				}
			} catch (StorageException ex) {
				App.Current.Dialogs.ErrorMessage (ex.Message);
			}
		}

		public T Copy (T template, string newName)
		{
			CheckInvalidChars (newName);
			Log.Information (String.Format ("Copying template {0} to {1}", template.Name, newName));

			template = template.Copy (newName);
			Add (template);
			return template;
		}

		public void Delete (T template)
		{
			Log.Information ("Deleting template " + template.Name);
			if (systemTemplates.Contains (template)) {
				// System templates can't be deleted
				throw new TemplateNotFoundException<T> (template.Name);
			}
			try {
				storage.Delete<T> (template);
				if (CollectionChanged != null) {
					CollectionChanged (this,
						new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, template));
				}
			} catch (StorageException ex) {
				App.Current.Dialogs.ErrorMessage (ex.Message);
			}
		}

		public T Create (string templateName, params object [] list)
		{
			/* Some templates don't need a count as a parameter but we include
			 * so that all of them match the same signature */
			if (list.Length == 0)
				list = new object [] { 0 };
			Log.Information (String.Format ("Creating default {0} template", typeof (T)));
			T t = (T)methodDefaultTemplate.Invoke (null, list);
			t.Name = templateName;
			return t;
		}

		void CheckInvalidChars (string name)
		{
			List<char> invalidChars;

			invalidChars = name.Intersect (Path.GetInvalidFileNameChars ()).ToList ();
			if (invalidChars.Count > 0) {
				throw new InvalidTemplateFilenameException (invalidChars);
			}
		}
	}
}
