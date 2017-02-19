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
using System.Threading.Tasks;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Services.Controller
{
	/// <summary>
	/// A controller to edit a <see cref="Dashboard"/>.
	/// </summary>
	public class DashboardEditorController : ControllerBase
	{
		DashboardVM dashboardVM;

		public override void Start ()
		{
			base.Start ();
			App.Current.EventsBroker.Subscribe<CreateDashboardButtonEvent> (HandleCreateButton);
			App.Current.EventsBroker.SubscribeAsync<DeleteEvent<DashboardButtonVM>> (HandleDeleteButton);
			App.Current.EventsBroker.SubscribeAsync<ReplaceDashboardFieldEvent> (HandleReplaceField);
			App.Current.EventsBroker.Subscribe<ResetDashboardFieldEvent> (HandleResetField);
		}

		public override void Stop ()
		{
			App.Current.EventsBroker.Unsubscribe<CreateDashboardButtonEvent> (HandleCreateButton);
			App.Current.EventsBroker.UnsubscribeAsync<DeleteEvent<DashboardButtonVM>> (HandleDeleteButton);
			App.Current.EventsBroker.UnsubscribeAsync<ReplaceDashboardFieldEvent> (HandleReplaceField);
			App.Current.EventsBroker.Unsubscribe<ResetDashboardFieldEvent> (HandleResetField);
			base.Stop ();
		}

		public override void SetViewModel (IViewModel viewModel)
		{
			dashboardVM = (DashboardVM)(viewModel as dynamic);
		}

		protected virtual async Task HandleReplaceField (ReplaceDashboardFieldEvent arg)
		{
			Image background = await App.Current.Dialogs.OpenImage ();
			if (background == null) {
				return;
			}

			background.ScaleInplace (Constants.MAX_BACKGROUND_WIDTH, Constants.MAX_BACKGROUND_HEIGHT);
			ReplaceField (arg.Field, background);
		}

		protected virtual void HandleResetField (ResetDashboardFieldEvent arg)
		{
			ReplaceField (arg.Field);
		}

		protected virtual async Task HandleDeleteButton (DeleteEvent<DashboardButtonVM> evt)
		{
			DashboardButtonVM buttonVM = evt.Object;
			if (buttonVM == null) {
				return;
			}

			string msg = Catalog.GetString ("Do you want to delete: ") + buttonVM.Name + "?";
			if (await App.Current.Dialogs.QuestionMessage (msg, null, this)) {
				dashboardVM.ViewModels.Remove (evt.Object);
			}
		}

		protected virtual async Task HandleRemoveLink (DeleteEvent<ActionLink> evt)
		{
			ActionLink link = evt.Object;
			if (link == null) {
				return;
			}

			string msg = string.Format ("{0} {1} ?",
										Catalog.GetString ("Do you want to delete: "), link);
			if (await App.Current.Dialogs.QuestionMessage (msg, null, this)) {
				link.SourceButton.ActionLinks.Remove (link);
			}
		}

		protected virtual void HandleCreateButton (CreateDashboardButtonEvent evt)
		{
			DashboardButton button = CreateButton (evt.Name);

			if (button != null) {
				button.Position = new Point (dashboardVM.CanvasWidth, 0);
				dashboardVM.Model.List.Add (button);
			}
		}

		protected virtual DashboardButton CreateButton (string buttonType)
		{
			DashboardButton button = null;

			if (buttonType == "Tag") {
				button = new TagButton { Tag = new Tag ("Tag", "") };
			} else if (buttonType == "Category") {
				button = dashboardVM.Model.CreateDefaultItem (dashboardVM.Model.List.Count);
			}

			return button;
		}

		void ReplaceField (FieldPositionType fieldPosition, Image background = null)
		{
			switch (fieldPosition) {
			case FieldPositionType.Field:
				dashboardVM.FieldBackground = background ?? App.Current.FieldBackground;
				break;
			case FieldPositionType.HalfField:
				dashboardVM.HalfFieldBackground = background ?? App.Current.HalfFieldBackground;
				break;
			case FieldPositionType.Goal:
				dashboardVM.GoalBackground = background ?? App.Current.GoalBackground;
				break;
			}
		}

	}
}
