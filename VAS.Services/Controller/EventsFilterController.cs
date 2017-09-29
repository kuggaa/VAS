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
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Filters;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
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

		public override async Task Start ()
		{
			await base.Start ();
			UpdatePredicates ();
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
			UpdatePeriodsPredicates ();
			UpdateEventTypesPredicates ();
		}

		protected virtual void UpdateTeamsPredicates ()
		{

		}

		protected virtual void UpdatePeriodsPredicates ()
		{
			bool oldIgnoreEvents = ViewModel.Filters.IgnoreEvents;
			ViewModel.Filters.IgnoreEvents = true;
			ViewModel.PeriodsPredicate.Clear ();

			List<IPredicate<TimelineEventVM>> listPredicates = new List<IPredicate<TimelineEventVM>> ();
			Expression<Func<TimelineEventVM, bool>> noPeriodExpression = ev => true;
			foreach (var period in ViewModel.Project.Periods) {
				noPeriodExpression = noPeriodExpression.And (ev => period.All (p => !ev.IsInside (p)));
				listPredicates.Add (new Predicate {
					Name = period.Name,
					Expression = ev => period.Any (p => ev.IsInside (p))
				});
			}
			ViewModel.PeriodsPredicate.Add (new Predicate {
				Name = Catalog.GetString ("No period"),
				Expression = noPeriodExpression
			});
			foreach (var predicate in listPredicates) {
				ViewModel.PeriodsPredicate.Add (predicate);
			}
			ViewModel.Filters.IgnoreEvents = oldIgnoreEvents;
			ViewModel.Filters.EmitPredicateChanged ();
		}

		protected virtual void UpdateEventTypesPredicates (object sender = null, NotifyCollectionChangedEventArgs e = null)
		{
			ViewModel.Filters.IgnoreEvents = true;
			ViewModel.CategoriesPredicate.Clear ();

			foreach (var eventType in ViewModel.EventTypesTimeline) {
				IPredicate<TimelineEventVM> predicate;

				Expression<Func<TimelineEventVM, bool>> eventTypeExpression = ev => ev.Model.EventType == eventType.Model;

				var analysisEventType = eventType.Model as AnalysisEventType;
				if (analysisEventType != null && analysisEventType.Tags.Any ()) {
					CompositePredicate<TimelineEventVM> composedEventTypePredicate;
					predicate = composedEventTypePredicate = new AndPredicate<TimelineEventVM> {
						Name = eventType.EventTypeVM.Name
					};

					foreach (var tagGroup in analysisEventType.TagsByGroup) {
						Expression<Func<TimelineEventVM, bool>> tagGroupExpression = ev => ev.Model.Tags.Any (tag => tag.Group == tagGroup.Key);
						var tagGroupPredicate = new OrPredicate<TimelineEventVM> {
							Name = tagGroup.Key,
						};

						tagGroupPredicate.Add (new Predicate {
							Name = Catalog.GetString ("None"),
							Expression = eventTypeExpression.And (ev => !ev.Model.Tags.Any (tag => tag.Group == tagGroup.Key))
						});

						foreach (var tag in tagGroup.Value) {
							tagGroupPredicate.Add (new Predicate {
								Name = tag.Value,
								Expression = eventTypeExpression.And (tagGroupExpression.And (ev => ev.Model.Tags.Contains (tag)))
							});
						}
						composedEventTypePredicate.Add (tagGroupPredicate);
					}
				} else {
					predicate = new Predicate {
						Name = eventType.EventTypeVM.Name,
						Expression = eventTypeExpression
					};
				}
				ViewModel.CategoriesPredicate.Add (predicate);
			}
			ViewModel.Filters.IgnoreEvents = false;
			ViewModel.Filters.EmitPredicateChanged ();
		}
	}
}
