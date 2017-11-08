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

namespace VAS.Services
{
	public abstract class TemplatesProvider<T> : ITemplateProvider<T>
		where T : ITemplate
	{
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		readonly MethodInfo methodDefaultTemplate;
		protected List<T> systemTemplates;
		protected IStorage storage;

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
				foreach (T stemplate in systemTemplates.Except (templates)) {
					T clonedTemplate = stemplate.Clone ();
					clonedTemplate.Static = true;
					templates.Add (clonedTemplate);
				}
				return templates;
			}
		}

		public int Level {
			get {
				return 30;
			}
		}

		public string Name {
			get {
				return $"{typeof (T).Name} Template Provider";
			}
		}

		public IStorage Storage => storage;

		public virtual bool Start ()
		{
			return true;
		}

		public virtual bool Stop ()
		{
			return true;
		}

		public T LoadFile (string filename)
		{
			Log.Information ("Loading template file " + filename);
			T template = App.Current.DependencyRegistry.
							Retrieve<IFileStorage> (InstanceType.Default, null).RetrieveFrom<T> (filename);
			return template;
		}

		public virtual void Save (T template)
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

		public virtual void Add (T template)
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

			template = (T)template.Copy (newName);
			template.Static = false;
			Add (template);
			return template;
		}

		public virtual void Delete (T template)
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

		public T Create (string templateName, int count = 0)
		{
			Log.Information (String.Format ("Creating default {0} template", typeof (T)));
			T t = CreateDefaultTemplate (count);
			t.Name = templateName;
			return t;
		}

		/// <summary>
		/// Converts to system template.
		/// </summary>
		/// <param name="template">Template.</param>
		protected void ConvertToSystemTemplate (T template)
		{
			Log.Information ("Setting new system template " + template.Name);
			template.Static = true;
			systemTemplates.Add (template);
		}

		protected abstract T CreateDefaultTemplate (int count);

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
