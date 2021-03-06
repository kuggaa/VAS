﻿//
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
using System.ComponentModel;
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
using VAS.Core.Store.Templates;
using VAS.Core.ViewModel;
using Predicate = VAS.Core.Filters.Predicate<VAS.Core.ViewModel.TimelineEventVM>;

namespace VAS.Services.Controller
{
	/// <summary>
	/// Events filter controller.
	/// This controller manages events filters and filtering for views where projects are used.
	/// </summary>
	public abstract class EventsFilterController : ControllerBase<TimelineVM>
	{
		protected List<IPredicate<TimelineEventVM>> previousPredicateList = new List<IPredicate<TimelineEventVM>> ();

		protected override void DisposeManagedResources ()
		{
			ViewModel.IgnoreEvents = true;
			base.DisposeManagedResources ();
			ViewModel = null;
		}

		protected bool DefaultActiveValue {
			get;
			set;
		} = true;

		public ProjectVM Project {
			get;
			set;
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
			Project = (viewModel as IProjectDealer)?.Project;
		}

		public override async Task Start ()
		{
			await base.Start ();
			if (!ViewModel.FiltersInitialized) {
				InitializePredicates ();
				ViewModel.FiltersInitialized = true;
			}
		}

		protected override void ConnectEvents ()
		{
			base.ConnectEvents ();
			ViewModel.EventTypesTimeline.ViewModels.CollectionChanged += UpdateEventTypesPredicates;
			ViewModel.Filters.PropertyChanged += HandleFiltersChanged;
		}

		protected override void DisconnectEvents ()
		{
			base.DisconnectEvents ();
			ViewModel.EventTypesTimeline.ViewModels.CollectionChanged -= UpdateEventTypesPredicates;
			ViewModel.Filters.PropertyChanged -= HandleFiltersChanged;
		}

		public override Task Stop ()
		{
			previousPredicateList.Clear ();
			return base.Stop ();
		}

		/// <summary>
		/// Initializes the predicates.
		/// This method is responsible for filling the predicates and adding the needed ones to 
		/// the Filters list in the ViewModel.
		/// </summary>
		protected abstract void InitializePredicates ();

		protected abstract void UpdateTeamsPredicates ();

		protected virtual void UpdatePeriodsPredicates ()
		{
			if (Project == null) {
				return;
			}

			bool oldIgnoreEvents = ViewModel.Filters.IgnoreEvents;
			ViewModel.Filters.IgnoreEvents = true;
			ViewModel.PeriodsPredicate.Clear ();

			List<IPredicate<TimelineEventVM>> listPredicates = new List<IPredicate<TimelineEventVM>> ();
			Expression<Func<TimelineEventVM, bool>> noPeriodExpression = ev => true;
			foreach (var period in Project.Periods) {
				noPeriodExpression = noPeriodExpression.And (ev => period.All (p => ev.Model.Intersect (p.Model) == null));
				listPredicates.Add (new Predicate {
					Name = period.Name,
					DisplayName = period.Name,
					Expression = ev => period.Any (p => ev.Model.Intersect (p.Model) != null)
				});
			}
			ViewModel.PeriodsPredicate.Add (new Predicate {
				Name = "No period",
				DisplayName = Catalog.GetString ("No period"),
				Expression = noPeriodExpression
			});
			ViewModel.PeriodsPredicate.AddRange (listPredicates);

			ViewModel.Filters.IgnoreEvents = oldIgnoreEvents;
			if (!ViewModel.Filters.IgnoreEvents) {
				ViewModel.Filters.EmitPredicateChanged ();
			}
		}

		protected virtual void UpdateTimersPredicates ()
		{
			if (Project == null) {
				return;
			}
			bool oldIgnoreEvents = ViewModel.Filters.IgnoreEvents;
			ViewModel.Filters.IgnoreEvents = true;
			ViewModel.TimersPredicate.Clear ();

			List<IPredicate<TimelineEventVM>> listPredicates = new List<IPredicate<TimelineEventVM>> ();
			Expression<Func<TimelineEventVM, bool>> noPeriodExpression = ev => true;
			foreach (var timer in Project.Timers) {
				noPeriodExpression = noPeriodExpression.And (ev => timer.All (p => ev.Model.Intersect (p.Model) == null));
				listPredicates.Add (new Predicate {
					Name = timer.Name,
					DisplayName = timer.Name,
					Expression = ev => timer.Any (p => ev.Model.Intersect (p.Model) != null)
				});
			}
			ViewModel.TimersPredicate.Add (new Predicate {
				Name = "No period",
				DisplayName = Catalog.GetString ("No period"),
				Expression = noPeriodExpression
			});
			ViewModel.TimersPredicate.AddRange (listPredicates);

			ViewModel.Filters.IgnoreEvents = oldIgnoreEvents;
			if (!ViewModel.Filters.IgnoreEvents) {
				ViewModel.Filters.EmitPredicateChanged ();
			}
		}

		protected virtual void UpdateCommonTagsPredicates ()
		{
			if (Project == null) {
				return;
			}
			bool oldIgnoreEvents = ViewModel.Filters.IgnoreEvents;
			ViewModel.Filters.IgnoreEvents = true;
			ViewModel.CommonTagsPredicate.Clear ();

			var tags = Project.Dashboard.Model.CommonTagsByGroup;

			var listPredicates = new List<IPredicate<TimelineEventVM>> ();
			Expression<Func<TimelineEventVM, bool>> noTagsExpression = ev => true;
			foreach (var tagGroup in tags) {
				var tagsPredicates = new List<IPredicate<TimelineEventVM>> ();
				noTagsExpression = noTagsExpression.And (ev => !tagGroup.Value.Intersect (ev.Model.Tags).Any ());

				Expression<Func<TimelineEventVM, bool>> tagGroupExpression = ev => ev.Model.Tags.Any (tag => tag.Group == tagGroup.Key);
				var tagGroupPredicate = new OrPredicate<TimelineEventVM> {
					Name = string.IsNullOrEmpty (tagGroup.Key) ? "General tags" : tagGroup.Key,
					DisplayName = string.IsNullOrEmpty (tagGroup.Key) ? Catalog.GetString ("General tags") : tagGroup.Key,
				};

				tagsPredicates.Add (new Predicate {
					Name = "None",
					DisplayName = Catalog.GetString ("None"),
					Expression = ev => !ev.Model.Tags.Any (tag => tag.Group == tagGroup.Key)
				});

				foreach (var tag in tagGroup.Value) {
					tagsPredicates.Add (new Predicate {
						Name = tag.Value,
						DisplayName = tag.Value,
						Expression = tagGroupExpression.And (ev => ev.Model.Tags.Contains (tag))
					});
				}
				tagGroupPredicate.AddRange (tagsPredicates);
				listPredicates.Add (tagGroupPredicate);
			}

			ViewModel.CommonTagsPredicate.AddRange (listPredicates);
			ViewModel.Filters.IgnoreEvents = oldIgnoreEvents;
			if (!ViewModel.Filters.IgnoreEvents) {
				ViewModel.Filters.EmitPredicateChanged ();
			}
		}

		protected void UpdatePreviousPredicatesList (CompositePredicate<TimelineEventVM> compositePredicate)
		{
			foreach (var predicate in compositePredicate) {
				if (!previousPredicateList.Exists (p => p.Name == predicate.Name)) {
					previousPredicateList.Add (predicate);
				} else {
					previousPredicateList.FirstOrDefault (p => p.Name == predicate.Name).Active = predicate.Active;
				}
			}
		}

		protected bool GetPreviousActiveValue (string predicateName)
		{
			var previous = previousPredicateList.FirstOrDefault (p => p.Name == predicateName);
			if (previous != null) {
				return previous.Active;
			}
			return DefaultActiveValue;
		}

		protected virtual void UpdateEventTypesPredicates (object sender = null, NotifyCollectionChangedEventArgs e = null)
		{
			bool oldIgnoreEvents = ViewModel.Filters.IgnoreEvents;
			ViewModel.Filters.IgnoreEvents = true;
			UpdatePreviousPredicatesList (ViewModel.EventTypesPredicate);
			ViewModel.EventTypesPredicate.Clear ();
			var predicates = new List<IPredicate<TimelineEventVM>> ();

			foreach (var eventType in ViewModel.EventTypesTimeline) {
				IPredicate<TimelineEventVM> predicate;
				List<Tag> tagList = new List<Tag> ();
				Expression<Func<TimelineEventVM, bool>> eventTypeExpression = ev => ev.Model.EventType.Equals (eventType.Model);

				var analysisEventType = eventType.Model as AnalysisEventType;
				if (analysisEventType != null && analysisEventType.Tags.Any ()) {
					CompositePredicate<TimelineEventVM> composedEventTypePredicate;
					var composedPredicates = new List<IPredicate<TimelineEventVM>> ();
					predicate = composedEventTypePredicate = new OrPredicate<TimelineEventVM> {
						Name = eventType.EventTypeVM.Name,
						DisplayName = eventType.EventTypeVM.Name,
					};

					// We want subcategories to be flat, regardless of the group.
					foreach (var tagGroup in analysisEventType.TagsByGroup) {
						Expression<Func<TimelineEventVM, bool>> tagGroupExpression = ev => ev.Model.Tags.Any (tag => tag.Group == tagGroup.Key);
						foreach (var tag in tagGroup.Value) {
							composedPredicates.Add (new Predicate {
								Name = tag.Value,
								DisplayName = tag.Value,
								Expression = eventTypeExpression.And (tagGroupExpression.And (ev => ev.Model.Tags.Contains (tag))),
								Active = GetPreviousActiveValue (tag.Value)
							});
							tagList.Add (tag);
						}
					}

					composedPredicates.Add (new Predicate {
						Name = "No subcategories",
						DisplayName = Catalog.GetString ("No subcategories"),
						Expression = eventTypeExpression.And (ev => !ev.Model.Tags.Intersect (tagList).Any ()),
						Active = GetPreviousActiveValue ("No subcategories")
					});
					composedEventTypePredicate.AddRange (composedPredicates);
				} else {
					predicate = new Predicate {
						Name = eventType.EventTypeVM.Name,
						DisplayName = eventType.EventTypeVM.Name,
						Expression = eventTypeExpression,
						Active = GetPreviousActiveValue (eventType.EventTypeVM.Name)
					};
				}
				predicates.Add (predicate);
			}
			ViewModel.EventTypesPredicate.AddRange (predicates);
			ViewModel.Filters.IgnoreEvents = oldIgnoreEvents;
			if (!ViewModel.Filters.IgnoreEvents) {
				ViewModel.Filters.EmitPredicateChanged ();
			}
		}

		void HandleFiltersChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == $"Collection_{nameof (ViewModel.Filters.Elements)}" ||
				e.PropertyName == nameof (ViewModel.Filters.Active)) {
				HandleFiltersChanged ();
			}
		}

		void HandleFiltersChanged ()
		{
			foreach (var eventVM in ViewModel.FullTimeline) {
				eventVM.Visible = ViewModel.Filters.Filter (eventVM);
			}
		}
	}
}
