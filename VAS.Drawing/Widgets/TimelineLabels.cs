//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using System.Linq;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.ViewModel;
using VAS.Drawing.CanvasObjects.Timeline;

namespace VAS.Drawing.Widgets
{

	public abstract class TimelineLabels : SelectionCanvas
	{
		protected Dictionary<EventTypeVM, LabelView> eventTypeToLabel;
		protected int labelWidth, labelHeight, eventTypesStartIndex;
		IAnalysisViewModel viewModel;

		public TimelineLabels (IWidget widget) : base (widget)
		{
			eventTypeToLabel = new Dictionary<EventTypeVM, LabelView> ();
			SelectionMode = MultiSelectionMode.Single;
		}

		public TimelineLabels () : this (null)
		{
		}

		public double Scroll {
			set {
				foreach (var o in Objects) {
					LabelView cl = o as LabelView;
					cl.Scroll = value;
				}
			}
		}

		public IAnalysisViewModel ViewModel {
			get {
				return viewModel;
			}
			set {
				if (viewModel != null) {
					viewModel.Project.EventTypes.ViewModels.CollectionChanged -= HandleCollectionChanged;
					viewModel.Project.EventTypes.PropertyChanged -= HandleEventTypesPropertyChanged;
				}
				viewModel = value;
				if (viewModel != null) {
					viewModel.Project.EventTypes.ViewModels.CollectionChanged += HandleCollectionChanged;
					viewModel.Project.EventTypes.PropertyChanged += HandleEventTypesPropertyChanged;
				}
			}
		}

		public virtual void SetViewModel (object viewModel)
		{
			ViewModel = (IAnalysisViewModel)viewModel;
		}

		protected virtual void AddEventType (EventTypeVM eventTypeVM, int i)
		{
			IView view = App.Current.ViewLocator.Retrieve ("EventTypeLabelView");
			view.SetViewModel (eventTypeVM);
			var l = view as LabelView;
			l.Width = labelWidth;
			l.Height = labelHeight;
			l.OffsetY = i * labelHeight;
			AddLabel (l, eventTypeVM);
			eventTypeToLabel [eventTypeVM] = l;
		}

		protected virtual void AddLabel (LabelView label, object obj)
		{
			AddObject (label);
		}

		protected void RemoveEventTypeLabel (EventTypeVM viewModel)
		{
			RemoveObject (eventTypeToLabel [viewModel]);
			eventTypeToLabel.Remove (viewModel);
		}

		protected virtual void FillCanvas (ref int i)
		{
			LabelView l;

			foreach (var timerViewModel in ViewModel.Project.Timers) {
				IView view = App.Current.ViewLocator.Retrieve ("TimerLabelView");
				view.SetViewModel (timerViewModel);
				l = view as LabelView;
				l.Width = labelWidth;
				l.Height = labelHeight;
				l.OffsetY = i * labelHeight;
				AddLabel (l, timerViewModel);
				i++;
			}
			eventTypesStartIndex = i;
			foreach (var eventTypeVM in ViewModel.Project.EventTypes) {
				AddEventType (eventTypeVM, i);
				i++;
			}
			UpdateLabelOffsets ();
		}

		protected virtual void UpdateLabelOffsets ()
		{
			int i = eventTypesStartIndex;
			foreach (EventTypeVM eventTypeVM in ViewModel.Project.EventTypes) {
				LabelView label = eventTypeToLabel [eventTypeVM];
				if (label.Visible) {
					label.OffsetY = i * label.Height;
					label.BackgroundColor = Utils.ColorForRow (i);
					i++;
				}
			}
			widget?.ReDraw ();
		}

		protected virtual void HandleCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add: {
					int i = eventTypesStartIndex + e.NewStartingIndex;
					foreach (EventTypeVM viewModel in e.NewItems.OfType<EventTypeVM> ()) {
						AddEventType (viewModel, i);
						i++;
					}
					break;
				}
			case NotifyCollectionChangedAction.Remove: {
					foreach (EventTypeVM viewModel in e.OldItems.OfType<EventTypeVM> ()) {
						RemoveEventTypeLabel (viewModel);
					}
					break;
				}
			case NotifyCollectionChangedAction.Reset: {
					foreach (EventTypeVM viewModel in eventTypeToLabel.Keys) {
						RemoveEventTypeLabel (viewModel);
					}
					break;
				}
			}
			UpdateLabelOffsets ();
		}

		void HandleEventTypesPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName != "Visible") {
				return;
			}
			UpdateLabelOffsets ();
		}
	}
}
