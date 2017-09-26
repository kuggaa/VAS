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
//
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using VAS.Core;
using VAS.Core.Filters;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;
using Predicate = VAS.Core.Filters.Predicate<VAS.Core.ViewModel.TimelineEventVM>;

namespace VAS.Services.Controller
{
	/// <summary>
	/// Events filter controller.
	/// This controller manages events filters and filtering for views where projects are used.
	/// </summary>
	public class EventsFilterController : ControllerBase<TimelineVM>
	{
		protected override void DisposeManagedResources ()
		{
			ViewModel.IgnoreEvents = true;
			base.DisposeManagedResources ();
			ViewModel = null;
		}

		/// <summary>
		/// Gets or sets the view model.
		/// </summary>
		/// <value>The view model.</value>
		public override TimelineVM ViewModel {
			get {
				return base.ViewModel;
			}
			set {
				base.ViewModel = value;
				if (base.ViewModel != null) {
					UpdatePredicates ();
				}
			}
		}

		/// <summary>
		/// Gets the default key actions.
		/// </summary>
		/// <returns>The default key actions.</returns>
		public override IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			return Enumerable.Empty<KeyAction> ();
		}

		/// <summary>
		/// Sets the view model.
		/// </summary>
		/// <param name="viewModel">View model.</param>
		public override void SetViewModel (IViewModel viewModel)
		{
			ViewModel = ((ITimelineDealer)viewModel).Timeline;
		}

		protected override void ConnectEvents ()
		{
			if (ViewModel != null) {
				ViewModel.EventTypesTimeline.ViewModels.CollectionChanged += UpdateEventTypesPredicates;
			}
		}

		protected override void DisconnectEvents ()
		{
			if (ViewModel != null) {
				ViewModel.EventTypesTimeline.ViewModels.CollectionChanged -= UpdateEventTypesPredicates;
			}
		}

		protected virtual void UpdatePredicates ()
		{
			UpdateTeamsPredicates ();
			UpdateEventTypesPredicates ();
		}

		protected virtual void UpdateTeamsPredicates ()
		{

		}

		protected virtual void UpdateEventTypesPredicates (object sender = null, NotifyCollectionChangedEventArgs e = null)
		{
			ViewModel.Filters.IgnoreEvents = true;
			ViewModel.CategoriesPredicate.Clear ();

			foreach (var eventType in ViewModel.EventTypesTimeline) {
				var predicate = new Predicate {
					Name = eventType.EventTypeVM.Name,
					Expression = ev => ev.Model.EventType.Name == eventType.Model.Name
				};
				ViewModel.CategoriesPredicate.Add (predicate);
			}
			ViewModel.Filters.IgnoreEvents = false;
			ViewModel.Filters.EmitPredicateChanged ();
		}
	}
}
