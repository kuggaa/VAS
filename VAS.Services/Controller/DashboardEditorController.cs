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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Services.Controller
{
	/// <summary>
	/// A controller to edit a <see cref="Dashboard"/>.
	/// </summary>
	public class DashboardEditorController : ControllerBase<DashboardVM>
	{
		List<AnalysisEventType> eventTypes;

		public DashboardEditorController()
		{
			eventTypes = new List<AnalysisEventType> ();
		}

		public override async Task Start ()
		{
			await base.Start ();
			App.Current.EventsBroker.Subscribe<CreateDashboardButtonEvent> (HandleCreateButton);
			App.Current.EventsBroker.SubscribeAsync<DeleteEvent<DashboardButtonVM>> (HandleDeleteButton);
			App.Current.EventsBroker.SubscribeAsync<DeleteEvent<ActionLinkVM>> (HandleRemoveLink);
			App.Current.EventsBroker.SubscribeAsync<ReplaceDashboardFieldEvent> (HandleReplaceField);
			App.Current.EventsBroker.Subscribe<ResetDashboardFieldEvent> (HandleResetField);
			App.Current.EventsBroker.Subscribe<DuplicateEvent<DashboardButtonVM>> (HandleDuplicateButton);
		}

		public override async Task Stop ()
		{
			App.Current.EventsBroker.Unsubscribe<CreateDashboardButtonEvent> (HandleCreateButton);
			App.Current.EventsBroker.UnsubscribeAsync<DeleteEvent<DashboardButtonVM>> (HandleDeleteButton);
			App.Current.EventsBroker.UnsubscribeAsync<DeleteEvent<ActionLinkVM>> (HandleRemoveLink);
			App.Current.EventsBroker.UnsubscribeAsync<ReplaceDashboardFieldEvent> (HandleReplaceField);
			App.Current.EventsBroker.Unsubscribe<ResetDashboardFieldEvent> (HandleResetField);
			App.Current.EventsBroker.Unsubscribe<DuplicateEvent<DashboardButtonVM>> (HandleDuplicateButton);
			await base.Stop ();
		}

		protected override void ConnectEvents ()
		{
			base.ConnectEvents ();
			ConnectEventTypes ();
		}

		protected override void DisconnectEvents ()
		{
			base.DisconnectEvents ();
			DisconnectEventTypes ();
		}

		public override IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			yield return new KeyAction (App.Current.HotkeysService.GetByName (GeneralUIHotkeys.DELETE),
			                            () => ViewModel.DeleteButton.Execute (ViewModel.Selection.FirstOrDefault ()), 10);
		}

		public override void SetViewModel (IViewModel viewModel)
		{
			ViewModel = ((IDashboardDealer)viewModel).Dashboard;
		}

		void ConnectEventTypes ()
		{
			foreach (var vm in ViewModel.ViewModels) {
				var eventType = (vm.Model as AnalysisEventButton)?.AnalysisEventType;
				if (eventType != null) {
					eventTypes.Add (eventType);
					eventType.Tags.CollectionChanged += HandleTagsCollectionChanged;
				}
			}
		}

		void DisconnectEventTypes()
		{
			foreach (var eventType in eventTypes) {
				if (eventType != null) {
					eventType.Tags.CollectionChanged -= HandleTagsCollectionChanged;
				}
			}
			eventTypes.Clear ();
		}

		void RemoveActionLinks (DashboardButtonVM button)
		{
			//Remove source ActionLinks
			button.ActionLinks.ViewModels.RemoveRange (button.ActionLinks.ViewModels);
			//Remove Dest ActionLinks
			foreach (var b in ViewModel.ViewModels) {
				var linksToRemove = b.ActionLinks.Where (al => al.DestinationButton == button);
				if (linksToRemove.Any ()) {
					b.ActionLinks.ViewModels.RemoveRange (linksToRemove);
				}
			}
		}

		void RemoveActionLinks (Tag tag)
		{
			foreach (var b in ViewModel.ViewModels) {
				var linksToRemove = b.ActionLinks.Where (al => ContainsSameReferenceTags (al.SourceTags, tag) ||
				                                         ContainsSameReferenceTags (al.DestinationTags, tag));
				if (linksToRemove.Any ()) {
					b.ActionLinks.ViewModels.RemoveRange (linksToRemove);
				}
			}
		}

		/// <summary>
		/// Compare a Tag with list of Tags by Reference to know if it is contained inside the list.
		/// We need a way to compare Tags by same reference, since <see cref="Tag"/> object
		/// overrides Equals and compare just for value and group. 
		/// </summary>
		/// <returns><c>true</c>, if tag references is contained in the list of tags, <c>false</c> otherwise.</returns>
		/// <param name="tags">Tags.</param>
		/// <param name="tag">Tag.</param>
		bool ContainsSameReferenceTags (IEnumerable<Tag> tags, Tag tag)
		{
			foreach (var t in tags) {
				if (ReferenceEquals (t, tag)) {
					return true;
				}
			}

			return false;
		}

		protected override void HandleViewModelChanged (object sender, PropertyChangedEventArgs e)
		{
			if (ViewModel.NeedsSync (e, nameof (ViewModel.Model))) {
				DisconnectEventTypes ();
				ConnectEventTypes ();
			}
		}

		protected async Task HandleReplaceField (ReplaceDashboardFieldEvent arg)
		{
			Image background = await App.Current.Dialogs.OpenImage ();
			if (background == null) {
				return;
			}

			background.ScaleInplace (Constants.MAX_BACKGROUND_WIDTH, Constants.MAX_BACKGROUND_HEIGHT);
			ReplaceField (arg.Field, background);
		}

		protected void HandleResetField (ResetDashboardFieldEvent arg)
		{
			ReplaceField (arg.Field);
		}

		protected async Task HandleDeleteButton (DeleteEvent<DashboardButtonVM> evt)
		{
			DashboardButtonVM buttonVM = evt.Object;
			if (buttonVM == null) {
				return;
			}

			string msg = Catalog.GetString ("Do you want to delete: ") + buttonVM.Name + "?";
			if (await App.Current.Dialogs.QuestionMessage (msg, null, this)) {
				RemoveActionLinks (evt.Object);
				ViewModel.ViewModels.Remove (evt.Object);
			}
		}

		protected async Task HandleRemoveLink (DeleteEvent<ActionLinkVM> evt)
		{
			ActionLinkVM link = evt.Object;
			if (link == null) {
				return;
			}

			string msg = string.Format ("{0} {1} ?",
										Catalog.GetString ("Do you want to delete: "), link.Name);
			if (await App.Current.Dialogs.QuestionMessage (msg, null, this)) {
				link.SourceButton.ActionLinks.ViewModels.Remove  (link);
			}
		}

		protected virtual void HandleCreateButton (CreateDashboardButtonEvent evt)
		{
			DashboardButton button = CreateButton (evt.Name);

			if (button != null) {
				button.Position = new Point (ViewModel.CanvasWidth, 0);
				ViewModel.Model.List.Add (button);
			}
		}

		protected virtual void HandleDuplicateButton (DuplicateEvent<DashboardButtonVM> arg)
		{
			var newButton = arg.Object.Model.Clone ();
			if (newButton is EventButton) {
				((EventButton)newButton).EventType.ID = Guid.NewGuid ();
			}
			newButton.Position.X += 50;
			newButton.Position.Y += 50;

			for (int i = 0; i < newButton.ActionLinks.Count (); i++) {
				newButton.ActionLinks [i].SourceButton = newButton;
				newButton.ActionLinks [i].DestinationButton =
							 arg.Object.Model.ActionLinks [i].DestinationButton;
			}

			ViewModel.Model.List.Add (newButton);
			arg.ReturnValue = ViewModel.ViewModels.Last ();
			ViewModel.Select (arg.ReturnValue);
		}

		protected virtual DashboardButton CreateButton (string buttonType)
		{
			DashboardButton button = null;

			if (buttonType == "Tag") {
				button = new TagButton { Tag = new Tag ("Tag", "") };
			} else if (buttonType == "Category") {
				button = ViewModel.Model.CreateDefaultItem (ViewModel.Model.List.Count);
			}

			return button;
		}

		void ReplaceField (FieldPositionType fieldPosition, Image background = null)
		{
			switch (fieldPosition) {
			case FieldPositionType.Field:
				ViewModel.FieldBackground = background ?? App.Current.FieldBackground;
				break;
			case FieldPositionType.HalfField:
				ViewModel.HalfFieldBackground = background ?? App.Current.HalfFieldBackground;
				break;
			case FieldPositionType.Goal:
				ViewModel.GoalBackground = background ?? App.Current.GoalBackground;
				break;
			}
		}

		void HandleTagsCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Remove) {
				foreach (Tag t in e.OldItems.OfType<Tag> ()) {
					RemoveActionLinks (t);
				}
			}
		}
	}
}
