//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store;
using Xamarin.Forms;

namespace VAS.UI.Forms
{
	public class FormsDialogs : IDialogs
	{
		Page CurrentPage(object parent)
		{
			Element element = parent as Element;
			if (element == null)
			{
				return FormsNavigation.Navigation.NavigationStack.LastOrDefault();
			}
			else
			{
				return element.CurrentPage();
			}
		}

		public async Task<object> ChooseOption(Dictionary<string, object> options, string title = null, object parent = null)
		{
			object ret = null;
			if (title == null)
			{
				title = Catalog.GetString("Choose option");
			}
			string[] optionKeys = new string[options.Count];
			options.Keys.CopyTo(optionKeys, 0);
			string key = await CurrentPage(parent).DisplayActionSheet(title, Catalog.GetString("Cancel"), null, optionKeys);
			if (key != null)
			{
				options.TryGetValue(key, out ret);
			}
			return ret;
		}

		public void InfoMessage(string message, object parent = null)
		{
			CurrentPage(parent).DisplayAlert("Information", message, "OK");
		}

		public void WarningMessage(string message, object parent = null)
		{
			CurrentPage(parent).DisplayAlert("Warning", message, "OK");
		}

		public void ErrorMessage(string message, object parent = null)
		{
			CurrentPage(parent).DisplayAlert("Error", message, "OK");
		}

		public async Task<bool> QuestionMessage(string message, string title, object parent = null)
		{
			return await CurrentPage(parent).DisplayAlert(title, message, "Accept", "Cancel");
		}

		public int ButtonsMessage(string message, List<string> textButtons, int? focusIndex, object parent = null)
		{
			throw new NotImplementedException();
		}

		public async Task<string> QueryMessage(string key, string title = null, string value = "", object parent = null)
		{
			throw new NotImplementedException();
			//IQueryMessage query = DependencyService.Get<IQueryMessage>();
			//query.Parent = parent;
			//query.Key = key;
			//query.Title = title;
			//query.Value = value;
			//return await query.Run();
		}

		public async Task<DateTime> SelectDate(DateTime date, object widget)
		{
			throw new NotImplementedException();
			//IDateSelector query = DependencyService.Get<IDateSelector>();
			//query.Date = date;
			//return await query.Run();
		}

		public virtual IBusyDialog BusyDialog(string message, object parent = null)
		{
			throw new NotImplementedException();
			//Page currentPage = CurrentPage(parent);
			//if (currentPage == null)
			//{
			//	throw new ArgumentException("The parent page can't be null");
			//}
			//return new BusyDialog(currentPage);
		}

		public virtual Task<bool> NewVersionAvailable(Version currentVersion, Version latestVersion, string downloadURL, string changeLog, object parent = null)
		{
			throw new NotImplementedException();
		}

		public virtual string SaveFile(string title, string defaultName, string defaultFolder, string filterName, string[] extensionFilter)
		{
			throw new NotImplementedException();
		}

		public virtual string OpenFile(string title, string defaultName, string defaultFolder, string filterName = null, string[] extensionFilter = null)
		{
			throw new NotImplementedException();
		}

		public virtual List<string> OpenFiles(string title, string defaultName, string defaultFolder, string filterName, string[] extensionFilter)
		{
			throw new NotImplementedException();
		}

		public virtual string SelectFolder(string title, string defaultName, string defaultFolder, string filterName, string[] extensionFilter)
		{
			throw new NotImplementedException();
		}

		public virtual MediaFile OpenMediaFile(object parent = null)
		{
			throw new NotImplementedException();
		}

		public virtual Task<VAS.Core.Common.Image> OpenImage(object parent = null)
		{
			throw new NotImplementedException();
		}
	}
}
