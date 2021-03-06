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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Services.Controller
{
	public abstract class TaggingController : ControllerBase
	{
		protected ProjectVM project;
		protected VideoPlayerVM videoPlayer;
		IDictionary<HotKeyVM, KeyAction> categoriesActions;

		public TaggingController ()
		{
			categoriesActions = new Dictionary<HotKeyVM, KeyAction> ();
		}

		/// <summary>
		/// Gets or sets the video player view model
		/// </summary>
		/// <value>The video player.</value>
		protected VideoPlayerVM VideoPlayer {
			get {
				return videoPlayer;
			}
			set {
				if (videoPlayer != null) {
					videoPlayer.PropertyChanged -= HandleVideoPlayerPropertyChanged;
				}
				videoPlayer = value;
				if (videoPlayer != null) {
					videoPlayer.PropertyChanged += HandleVideoPlayerPropertyChanged;
				}
			}
		}

		/// <summary>
		/// Start this instance.
		/// </summary>
		public override async Task Start ()
		{
			await base.Start ();
			App.Current.EventsBroker.Subscribe<ClickedPCardEvent> (HandleClickedPCardEvent);
			App.Current.EventsBroker.Subscribe<NewTagEvent> (HandleNewTagEvent);
			App.Current.EventsBroker.Subscribe<CapturerTickEvent> (HandleCapturerTick);
			App.Current.EventsBroker.Subscribe<TagClickedEvent> (HandleTagClicked);

			foreach (DashboardButtonVM button in project.Dashboard.ViewModels) {
				button.PropertyChanged += HandlePropertyChanged;
			}
		}

		/// <summary>
		/// Stop this instance.
		/// </summary>
		public override async Task Stop ()
		{
			await base.Stop ();
			App.Current.EventsBroker.Unsubscribe<ClickedPCardEvent> (HandleClickedPCardEvent);
			App.Current.EventsBroker.Unsubscribe<NewTagEvent> (HandleNewTagEvent);
			App.Current.EventsBroker.Unsubscribe<CapturerTickEvent> (HandleCapturerTick);
			App.Current.EventsBroker.Unsubscribe<TagClickedEvent> (HandleTagClicked);

			foreach (DashboardButtonVM button in project.Dashboard.ViewModels) {
				button.PropertyChanged -= HandlePropertyChanged;
			}
		}

		/// <summary>
		/// Sets the view model.
		/// </summary>
		/// <param name="viewModel">View model.</param>
		public override void SetViewModel (IViewModel viewModel)
		{
			project = ((IAnalysisViewModel)viewModel).Project;
			VideoPlayer = ((IAnalysisViewModel)viewModel).VideoPlayer;
		}

		/// <summary>
		/// Gets the default key actions.
		/// </summary>
		/// <returns>The default key actions.</returns>
		public override IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			var keyActions = new List<KeyAction> ();
			//Add AnalysisEventButtons
			foreach (var button in project.Dashboard.ViewModels) {
				var analysisButton = button as AnalysisEventButtonVM;
				if (analysisButton != null) {
					KeyAction action = new KeyAction (new KeyConfig {
						Name = analysisButton.Name,
						Key = analysisButton.HotKey.Model
					}, () => HandleSubCategoriesTagging (analysisButton));
					keyActions.Add (action);
					categoriesActions.Add (analysisButton.HotKey, action);
				}
			}

			return keyActions;
		}

		/// <summary>
		/// Handles when a Participant Card is clicked.
		/// </summary>
		/// <param name="e">Event.</param>
		protected void HandleClickedPCardEvent (ClickedPCardEvent e)
		{
			if (e.ClickedPlayer != null) {
				if (e.Modifier == ButtonModifier.Control) {
					e.ClickedPlayer.Tagged = !e.ClickedPlayer.Locked;
					e.ClickedPlayer.Locked = !e.ClickedPlayer.Locked;
				} else {
					if (!e.ClickedPlayer.Locked) {
						e.ClickedPlayer.Tagged = !e.ClickedPlayer.Tagged;
					}
				}
			}

			// Without the Shift modifier, unselect the rest of players that are not locked.
			if (e.Modifier != ButtonModifier.Shift) {
				foreach (PlayerVM player in project.Players) {
					if (player != e.ClickedPlayer && !player.Locked) {
						player.Tagged = false;
					}
				}
			}

			// Right now we don't care about selections and moving pcards
		}

		protected void PCardAction (ButtonModifier modifier, PlayerVM player)
		{
			HandleClickedPCardEvent (new ClickedPCardEvent {
				ClickedPlayer = player,
				Modifier = modifier,
				Sender = player
			});
		}

		protected virtual void AddTeamsToEvent (TimelineEventVM eventVM)
		{
			var teams = project.Teams.Where (team => team.Tagged).Select (team => team.Model);
			eventVM.Model.Teams.AddRange (teams);
		}

		protected abstract TimelineEventVM CreateTimelineEventVM (EventType type, Time start, Time stop,
															  Time eventTime, Image miniature);

		void HandleNewTagEvent (NewTagEvent e)
		{
			if (project == null) {
				return;
			}

			var eventVM = CreateTimelineEventVM (e.EventType, e.Start, e.Stop, e.EventTime, null);
			eventVM.Model.Tags.AddRange (e.Tags);

			// add selected tag buttons
			eventVM.Model.Tags.AddRange (project.Dashboard.ViewModels.OfType<TagButtonVM> ().Where (x => x.Active).Select (x => x.Tag.Model));

			AddPlayersToEvent (eventVM);
			AddTeamsToEvent (eventVM);

			App.Current.EventsBroker.Publish (
				new NewDashboardEvent {
					TimelineEvent = eventVM,
					DashboardButton = e.Button,
					Edit = !project.Dashboard.DisablePopupWindow,
					DashboardButtons = null,
				}
			);
			Reset ();
		}

		void AddPlayersToEvent (TimelineEventVM eventVM)
		{
			var players = project.Players.Where (p => p.Tagged);
			foreach (var playerVM in players) {
				eventVM.Model.Players.Add (playerVM.Model);
			}
		}

		/// <summary>
		/// Resets all pCards.
		/// </summary>
		void Reset ()
		{
			foreach (PlayerVM player in project.Players) {
				if (!player.Locked) {
					player.Tagged = false;
				}
			}
			foreach (var team in project.Teams) {
				team.Tagged = false;
			}
		}

		void HandleVideoPlayerPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (videoPlayer.NeedsSync (e.PropertyName, nameof (videoPlayer.CurrentTime), sender, videoPlayer) &&
			    project.ProjectType == ProjectType.FileProject) {
				project.Dashboard.CurrentTime = VideoPlayer.AbsoluteCurrentTime;
			}
		}

		void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			var changedButton = sender as HotKeyVM;
			if (changedButton != null && changedButton.NeedsSync (e, nameof (changedButton.Model)) &&
				categoriesActions.ContainsKey (changedButton)) {
				categoriesActions [changedButton].KeyConfig.Key = changedButton.Model;
			}
		}

		void HandleSubCategoriesTagging (AnalysisEventButtonVM buttonVM, TagVM subcategoryTagged = null)
		{
			if (subcategoryTagged != null) {
				buttonVM.SelectedTags.Add (subcategoryTagged);
			}

			KeyTemporalContext tempContext = new KeyTemporalContext { };
			foreach (var subcategory in buttonVM.Tags) {
				KeyAction action = new KeyAction (new KeyConfig {
					Name = subcategory.Value,
					Key = subcategory.HotKey.Model
				}, () => HandleSubCategoriesTagging (buttonVM, subcategory));
				tempContext.AddAction (action);
			}

			var analysisEventButton = (buttonVM.Model as AnalysisEventButton);
			if (analysisEventButton != null && analysisEventButton.TagMode == TagMode.Free) {
				tempContext.ExpiredTimeAction = buttonVM.ToggleIsCategoryClicked;
			} else {
				tempContext.Duration = Constants.TEMP_TAGGING_DURATION;
				tempContext.ExpiredTimeAction = buttonVM.Click;
			}

			App.Current.KeyContextManager.AddContext (tempContext);
		}

		void HandleCapturerTick (CapturerTickEvent e)
		{
			project.Dashboard.CurrentTime = e.Time;
		}

		void HandleTagClicked (TagClickedEvent e)
		{
			if (e.ButtonVM.Active) {
				// unselect tags of the same group
				var relatedTags = project.Dashboard.ViewModels.OfType<TagButtonVM> ().Where (x => x.Tag.Group == e.ButtonVM.Tag.Group && x != e.ButtonVM).ToList ();
				foreach (var tag in relatedTags) {
					tag.Active = false;
				}
			}
		}
	}
}
