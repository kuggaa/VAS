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
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Templates;
using VAS.Core.ViewModel;

namespace VAS.Services.Controller
{
	public class TaggingController<TModel, TViewModel> : DisposableBase, IController
		where TModel : Team
		where TViewModel : TemplateViewModel<TModel>, new()
	{
		/// <summary>
		/// Gets or sets the view model.
		/// </summary>
		/// <value>The view model.</value>
		protected ProjectVM ViewModel {
			get;
			set;
		}

		/// <summary>
		/// Start this instance.
		/// </summary>
		public void Start ()
		{
			App.Current.EventsBroker.Subscribe<ClickedPCardEvent> (HandleClickedPCardEvent);
			App.Current.EventsBroker.Subscribe<NewTagEvent> (HandleNewTagEvent);
		}

		/// <summary>
		/// Stop this instance.
		/// </summary>
		public void Stop ()
		{
			App.Current.EventsBroker.Unsubscribe<ClickedPCardEvent> (HandleClickedPCardEvent);
			App.Current.EventsBroker.Unsubscribe<NewTagEvent> (HandleNewTagEvent);
		}

		/// <summary>
		/// Sets the view model.
		/// </summary>
		/// <param name="viewModel">View model.</param>
		public void SetViewModel (IViewModel viewModel)
		{
			ViewModel = (ProjectVM)(viewModel as dynamic);
		}

		/// <summary>
		/// Gets the default key actions.
		/// </summary>
		/// <returns>The default key actions.</returns>
		public IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			return Enumerable.Empty<KeyAction> ();
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
				foreach (PlayerVM player in ViewModel.Players) {
					if (player != e.ClickedPlayer && !player.Locked) {
						player.Tagged = false;
					}
				}
			}

			// Right now we don't care about selections and moving pcards
		}

		protected void HandleNewTagEvent (NewTagEvent e)
		{
			//FIXME: This is using the Model of the ViewModel, that method should be moved here
			// Reception of the event Button
			var play = ViewModel.Model.AddEvent (e.EventType, e.Start, e.Stop, e.EventTime, null, false) as TimelineEvent;

			//Here we can set the players if necessary, then send to events aggregator
			App.Current.EventsBroker.Publish (
				new NewDashboardEvent {
					TimelineEvent = play,
					DashboardButton = e.Button,
					Edit = false,
					DashboardButtons = null,
					ProjectId = ViewModel.Model.ID
				}
			);
			Reset ();
		}

		/// <summary>
		/// Helper method that fills data needed for a new tag event
		/// </summary>
		/// <param name="play">Play.</param>
		protected virtual void AddNewTagData (TimelineEvent play)
		{
			foreach (var playerVM in ViewModel.Players.Where (p => p.Tagged)) {
				play.Players.Add (playerVM.Model);
			}
		}

		/// <summary>
		/// Resets all pCards.
		/// </summary>
		void Reset ()
		{
			foreach (PlayerVM player in ViewModel.Players) {
				if (!player.Locked) {
					player.Tagged = false;
				}
			}
		}
	}
}
