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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Templates;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// Class for the Dashboard ViewModel.
	/// </summary>
	public class DashboardVM : TemplateViewModel<Dashboard, DashboardButton, DashboardButtonVM>
	{
		DashboardMode mode;
		Time currentTime;
		bool showLinks;

		public DashboardVM ()
		{
			AddButton = new Command<string> (
				(s) => App.Current.EventsBroker.Publish (new CreateDashboardButtonEvent { Name = s })) {
				Icon = App.Current.ResourcesLocator.LoadIcon ("vas-add", App.Current.Style.ButtonNormalWidth),
				Text = Catalog.GetString ("Add"),
			};

			DeleteButton = new Command<DashboardButtonVM> (
				(s) => App.Current.EventsBroker.Publish (new DeleteEvent<DashboardButtonVM> { Object = s })) {
				Icon = App.Current.ResourcesLocator.LoadIcon ("vas-delete", App.Current.Style.ButtonNormalWidth),
				Text = Catalog.GetString ("Delete"),
			};

			DuplicateButton = new Command<DashboardButtonVM> (
				(s) => App.Current.EventsBroker.Publish (new DuplicateEvent<DashboardButtonVM> { Object = s })) {
				Text = Catalog.GetString ("Duplicate"),
			};

			DeleteLink = new Command<ActionLinkVM> (
				(s) => App.Current.EventsBroker.Publish (new DeleteEvent<ActionLinkVM> { Object = s })) {
				Icon = App.Current.ResourcesLocator.LoadIcon ("vas-delete", App.Current.Style.ButtonNormalWidth),
				Text = Catalog.GetString ("Delete"),
			};

			ResetField = new Command<FieldPositionType> (
				(p) => App.Current.EventsBroker.Publish (new ResetDashboardFieldEvent { Field = p })) {
				Text = Catalog.GetString ("Reset"),
			};

			ChangeField = new Command<FieldPositionType> (
				(p) => App.Current.EventsBroker.Publish (new ReplaceDashboardFieldEvent { Field = p })) {
				Text = Catalog.GetString ("Change"),
			};

			ToggleActionLinks = new LimitationCommand<bool> (VASFeature.LinkingButtons.ToString (), (p) => ShowLinks = p) {
				Icon = App.Current.ResourcesLocator.LoadIcon ("vas-link-active", App.Current.Style.IconSmallWidth),
				IconInactive = App.Current.ResourcesLocator.LoadIcon ("vas-link-disabled", App.Current.Style.IconSmallWidth),
				ToolTipText = Catalog.GetString ("Edit action links"),
			};

			ChangeDashboardMode = new Command<DashboardMode> ((p) => Mode = p) {
				Icon = App.Current.ResourcesLocator.LoadIcon ("vas-dash-edit-active", App.Current.Style.IconSmallWidth),
				IconInactive = App.Current.ResourcesLocator.LoadIcon ("vas-dash-edit", App.Current.Style.IconSmallWidth),
				ToolTipText = Catalog.GetString ("Edit dashboard"),
			};

			TogglePopupWindow = new Command<bool> ((p) => DisablePopupWindow = !p) {
				Icon = App.Current.ResourcesLocator.LoadIcon ("vas-popup", App.Current.Style.IconSmallWidth),
				ToolTipText = Catalog.GetString ("Disable popup window"),
			};

			ChangeFitMode = new Command<FitMode> ((p) => FitMode = p);

			ViewModels.CollectionChanged += HandleCollectionChanged;
			CurrentTime = new Time ();
		}

		/// <summary>
		/// Get the command to add a new button.
		/// </summary>
		[PropertyChanged.DoNotNotify]
		public Command<string> AddButton {
			get;
			private set;
		}

		/// <summary>
		/// Gets the command to delete button.
		/// </summary>
		[PropertyChanged.DoNotNotify]
		public Command<DashboardButtonVM> DeleteButton {
			get;
			private set;
		}

		/// <summary>
		/// Gets the command to duplicate a button.
		/// </summary>
		[PropertyChanged.DoNotNotify]
		public Command<DashboardButtonVM> DuplicateButton {
			get;
			private set;
		}

		/// <summary>
		/// Gets the command to delete link.
		/// </summary>
		[PropertyChanged.DoNotNotify]
		public Command<ActionLinkVM> DeleteLink {
			get;
			private set;
		}

		/// <summary>
		/// Gets the command to reset a field background to its default value.
		/// </summary>
		[PropertyChanged.DoNotNotify]
		public Command<FieldPositionType> ResetField {
			get;
			private set;
		}

		/// <summary>
		/// Gets the command to change a field background.
		/// </summary>
		[PropertyChanged.DoNotNotify]
		public Command<FieldPositionType> ChangeField {
			get;
			private set;
		}

		/// <summary>
		/// Gets the command to change the dashboard mode.
		/// </summary>
		[PropertyChanged.DoNotNotify]
		public Command<DashboardMode> ChangeDashboardMode {
			get;
			private set;
		}

		/// <summary>
		/// Gets the command to toggle the actions links visiblity.
		/// </summary>
		[PropertyChanged.DoNotNotify]
		public LimitationCommand<bool> ToggleActionLinks {
			get;
			private set;
		}

		/// <summary>
		/// Gets the command to toggle the popup window.
		/// </summary>
		public Command<bool> TogglePopupWindow {
			get;
			private set;
		}

		/// <summary>
		/// Gets the command to change the dashboard fit mode.
		/// </summary>
		/// <value>The change fit mode.</value>
		public Command<FitMode> ChangeFitMode {
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets the icon.
		/// </summary>
		/// <value>The icon.</value>
		public override Image Icon {
			get {
				return Model.Image;
			}

			set {
				Model.Image = value;
			}
		}

		/// <summary>
		/// Gets or sets the preview of this dashboardVM.
		/// </summary>
		/// <value>The preview.</value>
		public Image Preview => Model.Preview;

		/// <summary>
		/// Gets or sets the mode.
		/// </summary>
		/// <value>The mode.</value>
		public DashboardMode Mode {
			get {
				return mode;
			}
			set {
				mode = value;
				foreach (var vm in ViewModels) {
					vm.Mode = mode;
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating to showing or not the links.
		/// </summary>
		/// <value><c>true</c> if show links; otherwise, <c>false</c>.</value>
		public bool ShowLinks {
			get {
				return showLinks && mode == DashboardMode.Edit;
			}
			set {
				showLinks = value;
			}
		}

		/// <summary>
		/// Gets or sets the current time.
		/// </summary>
		/// <value>The current time.</value>
		public Time CurrentTime {
			get {
				return currentTime;
			}
			set {
				currentTime = value;
				foreach (var timedVM in ViewModels.OfType<ITimed> ()) {
					timedVM.CurrentTime = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the fit mode.
		/// </summary>
		/// <value>The fit mode.</value>
		public FitMode FitMode {
			get;
			set;
		}

		/// <summary>
		/// Gets the width of the canvas.
		/// </summary>
		/// <value>The width of the canvas.</value>
		public int CanvasWidth {
			get {
				return Model.CanvasWidth;
			}
		}

		/// <summary>
		/// Gets the height of the canvas.
		/// </summary>
		/// <value>The height of the canvas.</value>
		public int CanvasHeight {
			get {
				return Model.CanvasHeight;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.ViewModel.DashboardVM"/> disable popup window.
		/// </summary>
		/// <value><c>true</c> if disable popup window; otherwise, <c>false</c>.</value>
		public bool DisablePopupWindow {
			get {
				return Model.DisablePopupWindow;
			}
			set {
				Model.DisablePopupWindow = value;
			}
		}

		/// <summary>
		/// Sets a value indicating whether this <see cref="T:VAS.Core.ViewModel.DashboardVM"/> can edit plays.
		/// </summary>
		/// <value><c>true</c> if can edit; otherwise, <c>false</c>.</value>
		public bool EditPlays {
			get;
			set;
		}

		public Image FieldBackground {
			get {
				return Model.FieldBackground;
			}
			set {
				Model.FieldBackground = value;
			}
		}

		public Image HalfFieldBackground {
			get {
				return Model.HalfFieldBackground;
			}
			set {
				Model.HalfFieldBackground = value;
			}
		}

		public Image GoalBackground {
			get {
				return Model.GoalBackground;
			}
			set {
				Model.GoalBackground = value;
			}
		}

		public ObservableCollection<string> GamePeriods {
			get {
				return Model.GamePeriods;
			}
			set {
				Model.GamePeriods = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:VAS.Core.ViewModel.DashboardVM"/> is the default dashboard.
		/// </summary>
		/// <value><c>true</c> if it's the default dashboard; otherwise, <c>false</c>.</value>
		public bool IsDefaultDashboard => Name == App.Current.Config.DefaultTemplate;

		/// <summary>
		/// Creates the sub view model.
		/// </summary>
		/// <returns>The sub view model.</returns>
		public override CollectionViewModel<DashboardButton, DashboardButtonVM> CreateSubViewModel ()
		{
			return new DashboardButtonCollectionVM ();
		}

		void HandleCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Add) {
				foreach (DashboardButtonVM buttonVM in e.NewItems) {
					buttonVM.Mode = Mode;
					if (buttonVM is TimedDashboardButtonVM) {
						((TimedDashboardButtonVM)buttonVM).CurrentTime = CurrentTime;
					}
				}
			}
		}

	}
}
