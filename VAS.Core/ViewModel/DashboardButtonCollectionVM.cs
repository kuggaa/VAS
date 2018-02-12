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
using System.Collections.Generic;
using System.Linq;
using VAS.Core.MVVMC;
using VAS.Core.Store;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// Collection of DashboardButtonVM's.
	/// </summary>
	public class DashboardButtonCollectionVM : CollectionViewModel<DashboardButton, DashboardButtonVM>
	{
		IDictionary<DashboardButton, DashboardButtonVM> cachedButtonVMs;

		public DashboardButtonCollectionVM ()
		{
			cachedButtonVMs = new Dictionary<DashboardButton, DashboardButtonVM> ();
		}

		protected override DashboardButtonVM CreateInstance (DashboardButton model)
		{
			// check if the view model has been already created (as dependency/link of another button)
			DashboardButtonVM buttonVM = null;
			if (modelToViewModel.ContainsKey (model)) {
				buttonVM = modelToViewModel [model];
			} else {
				buttonVM = base.CreateInstance (model);
			}

			foreach (ActionLinkVM link in buttonVM.ActionLinks) {
				if (model == link.Model.SourceButton) {
					link.SourceButton = buttonVM;
				} else {
					link.SourceButton = GetOrCreateViewModel (link.Model.SourceButton);
				}

				if (model == link.Model.DestinationButton) {
					link.DestinationButton = buttonVM;
				} else {
					link.DestinationButton = GetOrCreateViewModel (link.Model.DestinationButton);

					// update also instances of tags since the deserialization is done in different documents
					// and the destination buttons tags and the links tags have different instances ( which
					// causes that a name change in the button will not be updated in the link)
					var ab = link.DestinationButton.Model as AnalysisEventButton;
					if (ab != null) {
						var destinationTags = link.Model.DestinationTags.ToList ();
						link.Model.DestinationTags.IgnoreEvents = true;

						link.Model.DestinationTags.Clear ();
						foreach (var s in destinationTags) {
							var e = ab.AnalysisEventType.Tags.FirstOrDefault (x => x.Equals (s));
							link.Model.DestinationTags.Add (e);
						}

						link.Model.DestinationTags.IgnoreEvents = false;
					}
				}
			}

			return buttonVM;
		}
	}
}
