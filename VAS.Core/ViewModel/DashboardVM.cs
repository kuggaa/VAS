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
using System.Collections.ObjectModel;
using VAS.Core.Common;
using VAS.Core.Events;
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

		public DashboardVM ()
		{
			AddButton = new Command<string> (
				(s) => App.Current.EventsBroker.Publish (new CreateDashboardButtonEvent { Name = s })) {
				Icon = Resources.LoadIcon ("longomatch-add", App.Current.Style.ButtonNormalWidth),
				Text = Catalog.GetString ("Add"),
			};

			DeleteButton = new Command<DashboardButtonVM> (
				(s) => App.Current.EventsBroker.Publish (new DeleteEvent<DashboardButtonVM> { Object = s })) {
				Icon = Resources.LoadIcon ("longomatch-delete", App.Current.Style.ButtonNormalWidth),
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

			ToggleActionLinks = new Command<bool> ((p) => ShowLinks = p) {
				Icon = Resources.LoadIcon ("longomatch-link-disabled", App.Current.Style.ButtonNormalWidth),
				IconInactive = Resources.LoadIcon ("longomatch-link-active", App.Current.Style.ButtonNormalWidth),
			};

			ChangeDashboardMode = new Command<DashboardMode> ((p) => Mode = p) {
				Icon = Resources.LoadIcon ("longomatch-dash-edit_active", App.Current.Style.ButtonNormalWidth),
				IconInactive = Resources.LoadIcon ("longomatch-dash-edit", App.Current.Style.ButtonNormalWidth),
			};

			TogglePopupWindow = new Command<bool> ((p) => DisablePopupWindow = !p) {
				Icon = Resources.LoadIcon ("longomatch-popup", App.Current.Style.ButtonNormalWidth),
				ToolTipText = Catalog.GetString ("Disable popup window"),
			};

			ChangeFitMode = new Command<FitMode> ((p) => FitMode = p);
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
		/// Gets the command to delete link.
		/// </summary>
		[PropertyChanged.DoNotNotify]
		public Command<ActionLink> DeleteLink {
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
		public Command<bool> ToggleActionLinks {
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
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the current time.
		/// </summary>
		/// <value>The current time.</value>
		public Time CurrentTime {
			get;
			set;
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

		public string GamePeriods {
			get {
				return string.Join ("-", Model.GamePeriods);
			}
			set {
				try {
					Model.GamePeriods = new ObservableCollection<string> (value.Split ('-'));
				} catch {
					App.Current.Dialogs.ErrorMessage (Catalog.GetString ("Could not parse game periods."));
				}
			}
		}

		/// <summary>
		/// Creates the sub view model.
		/// </summary>
		/// <returns>The sub view model.</returns>
		public override CollectionViewModel<DashboardButton, DashboardButtonVM> CreateSubViewModel ()
		{
			return new DashboardButtonCollectionVM ();
		}
	}
}
