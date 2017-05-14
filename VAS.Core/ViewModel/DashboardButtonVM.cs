//
//  Copyright (C) 2016 Fluendo S.A.
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using Timer = VAS.Core.Store.Timer;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// ViewModel for <see cref="DashboardButton"/>.
	/// </summary>
	public class DashboardButtonVM : ViewModelBase<DashboardButton>
	{
		public DashboardButtonVM ()
		{
			ActionLinks = new CollectionViewModel<ActionLink, ActionLinkVM> ();
			HotKey = new HotKeyVM ();
		}

		/// <summary>
		/// Gets or sets the model (DashboardButton).
		/// </summary>
		/// <value>The model.</value>
		public override DashboardButton Model {
			get {
				return base.Model;
			}
			set {
				ActionLinks.Model = value.ActionLinks;
				HotKey.Model = value.HotKey;
				base.Model = value;
			}
		}

		/// <summary>
		/// Gets the DashboardButtonView.
		/// </summary>
		/// <value>The view.</value>
		public virtual string View {
			get;
		}

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		public virtual string Name {
			get {
				return Model.Name;
			}
			set {
				Model.Name = value;
			}
		}

		/// <summary>
		/// Gets or sets the position.
		/// </summary>
		/// <value>The position.</value>
		public Point Position {
			get {
				return Model.Position;
			}
			set {
				Model.Position = value;
			}
		}

		/// <summary>
		/// Gets or sets the width.
		/// </summary>
		/// <value>The width.</value>
		public int Width {
			get {
				return Model.Width;
			}
			set {
				Model.Width = value;
			}
		}

		/// <summary>
		/// Gets or sets the height.
		/// </summary>
		/// <value>The height.</value>
		public int Height {
			get {
				return Model.Height;
			}
			set {
				Model.Height = value;
			}
		}

		/// <summary>
		/// Gets or sets the color of the background.
		/// </summary>
		/// <value>The color of the background.</value>
		public Color BackgroundColor {
			get {
				return Model.BackgroundColor;
			}
			set {
				Model.BackgroundColor = value;
			}
		}

		/// <summary>
		/// Gets or sets the color of the text.
		/// </summary>
		/// <value>The color of the text.</value>
		public Color TextColor {
			get {
				return Model.TextColor;
			}
			set {
				Model.TextColor = value;
			}
		}

		/// <summary>
		/// Gets or sets the hot key.
		/// </summary>
		/// <value>The hot key.</value>
		public virtual HotKeyVM HotKey {
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets the background image.
		/// </summary>
		/// <value>The background image.</value>
		public virtual Image BackgroundImage {
			get {
				return Model.BackgroundImage;
			}
			set {
				Model.BackgroundImage = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating to showing or not the hotkey.
		/// </summary>
		/// <value><c>true</c> if show hotkey; otherwise, <c>false</c>.</value>
		public bool ShowHotkey {
			get {
				return Model.ShowHotkey;
			}
			set {
				Model.ShowHotkey = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating to showing or not the setting icon.
		/// </summary>
		/// <value><c>true</c> if show setting icon; otherwise, <c>false</c>.</value>
		public bool ShowSettingIcon {
			get {
				return Model.ShowSettingIcon;
			}
			set {
				Model.ShowSettingIcon = value;
			}
		}

		/// <summary>
		/// A list with all the outgoing links of this button
		/// </summary>
		public CollectionViewModel<ActionLink, ActionLinkVM> ActionLinks {
			get;
			set;
		}

		/// <summary>
		/// Gets the LightColor.
		/// </summary>
		/// <value>The color of the light.</value>
		public Color LightColor {
			get {
				return Model.LightColor;
			}
		}

		/// <summary>
		/// Gets the DarkColor.
		/// </summary>
		/// <value>The color of the dark.</value>
		public Color DarkColor {
			get {
				return Model.DarkColor;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating to showing or not the icon.
		/// </summary>
		/// <value><c>true</c> if show icon; otherwise, <c>false</c>.</value>
		public bool ShowIcon {
			get {
				return Model.ShowIcon;
			}
			set {
				Model.ShowIcon = value;
			}
		}
		/// <summary>
		/// Gets or sets the dashboard mode.
		/// </summary>
		/// <value>The mode.</value>
		[PropertyChanged.DoNotNotify]
		public DashboardMode Mode {
			get;
			set;
		}
	}

	/// <summary>
	/// Class for the TimedDashBoardButton ViewModel.
	/// </summary>
	public class TimedDashboardButtonVM : DashboardButtonVM, ITimed
	{
		Time currentTime;

		/// <summary>
		/// Gets or sets the model.
		/// </summary>
		/// <value>The model.</value>
		public new TimedDashboardButton Model {
			get {
				return (TimedDashboardButton)base.Model;
			}
			set {
				base.Model = value;
			}
		}

		/// <summary>
		/// Gets or sets the tag mode.
		/// </summary>
		/// <value>The tag mode.</value>
		public TagMode TagMode {
			get {
				return Model.TagMode;
			}
			set {
				Model.TagMode = value;
			}
		}

		/// <summary>
		/// Gets or sets the start time.
		/// </summary>
		/// <value>The start.</value>
		public Time Start {
			get {
				return Model.Start;
			}
			set {
				Model.Start = value;
			}
		}

		/// <summary>
		/// Gets or sets the stop time.
		/// </summary>
		/// <value>The stop.</value>
		public Time Stop {
			get {
				return Model.Stop;
			}
			set {
				Model.Stop = value;
			}
		}

		public Time RecordingStart {
			get;
			set;
		}

		[PropertyChanged.DoNotNotify]
		public Time CurrentTime {
			get {
				return currentTime;
			}
			set {
				if (value != null && Mode != DashboardMode.Edit
					&& RecordingStart != null && TagMode != TagMode.Predefined) {
					if (value.MSeconds + Constants.PLAYBACK_TOLERANCE < RecordingStart.MSeconds) {
						ButtonTime = null;
					}
					if (currentTime != null &&
						currentTime.TotalSeconds != value.TotalSeconds) {
						ButtonTime = value - RecordingStart;
					}
				}
				currentTime = value;
			}
		}

		public Time ButtonTime {
			get;
			set;
		}
	}

	/// <summary>
	/// Class for the EventButton ViewModel
	/// </summary>
	public class EventButtonVM : TimedDashboardButtonVM
	{
		/// <summary>
		/// Gets or sets the model.
		/// </summary>
		/// <value>The model.</value>
		public new EventButton Model {
			get {
				return (EventButton)base.Model;
			}
			set {
				base.Model = value;
				EventType = new EventTypeVM { Model = Model.EventType };
			}
		}

		/// <summary>
		/// Gets the type of the event.
		/// </summary>
		/// <value>The type of the event.</value>
		public EventTypeVM EventType {
			get;
			private set;
		}
	}

	/// <summary>
	/// Class for the AnalysisEventButton ViewModel.
	/// </summary>
	public class AnalysisEventButtonVM : EventButtonVM
	{
		public AnalysisEventButtonVM ()
		{
			SelectedTags = new List<Tag> ();
		}

		/// <summary>
		/// Gets or sets the model.
		/// </summary>
		/// <value>The model.</value>
		public new AnalysisEventButton Model {
			get {
				return (AnalysisEventButton)base.Model;
			}
			set {
				base.Model = value;
			}
		}

		/// <summary>
		/// Gets the subcategories of the event
		/// </summary>
		/// <value>The tags.</value>
		public IEnumerable<Tag> Tags {
			get {
				return Model.AnalysisEventType.Tags;
			}
		}

		/// <summary>
		/// Gets the view.
		/// </summary>
		/// <value>The view.</value>
		public override string View {
			get {
				return "AnalysisEventButtonView";
			}
		}

		/// <summary>
		/// Gets or sets a value indicating to showing or not the subcategories.
		/// </summary>
		/// <value><c>true</c> if show subcategories; otherwise, <c>false</c>.</value>
		public bool ShowSubcategories {
			get {
				return Model.ShowSubcategories;
			}
			set {
				Model.ShowSubcategories = value;
			}
		}

		/// <summary>
		/// Gets or sets the tags per row.
		/// </summary>
		/// <value>The tags per row.</value>
		public int TagsPerRow {
			get {
				return Model.TagsPerRow;
			}
			set {
				Model.TagsPerRow = value;
			}
		}

		public List<Tag> SelectedTags {
			get;
			set;
		}

		public void Click ()
		{
			if (Mode == DashboardMode.Edit) {
				return;
			}

			Time start, stop, eventTime;

			if (TagMode == TagMode.Predefined || RecordingStart == null) {
				start = CurrentTime - Start;
				stop = CurrentTime + Stop;
				eventTime = CurrentTime;
			} else {
				stop = CurrentTime;
				start = RecordingStart - Start;
				eventTime = RecordingStart;
			}
			var tags = new List<Tag> ();
			tags.AddRange (SelectedTags);

			App.Current.EventsBroker.Publish (
				new NewTagEvent {
					EventType = Model.EventType,
					Start = start,
					Stop = stop,
					EventTime = eventTime,
					Button = Model,
					Tags = tags
				}
			);

			this.SelectedTags.Clear ();
			RecordingStart = null;
			ButtonTime = null;
		}
	}

	/// <summary>
	/// Class for the TagButton ViewModel
	/// </summary>
	public class TagButtonVM : DashboardButtonVM
	{
		/// <summary>
		/// Gets or sets the model.
		/// </summary>
		/// <value>The model.</value>
		public new TagButton Model {
			get {
				return (TagButton)base.Model;
			}
			set {
				base.Model = value;
			}
		}

		/// <summary>
		/// Gets the view.
		/// </summary>
		/// <value>The view.</value>
		public override string View {
			get {
				return "TagButtonView";
			}
		}

		/// <summary>
		/// Gets or sets the tag.
		/// </summary>
		/// <value>The tag.</value>
		public Tag Tag {
			get {
				return Model.Tag;
			}
			set {
				Model.Tag = value;
			}
		}
	}

	/// <summary>
	/// Class for the TimerButton ViewModel.
	/// </summary>
	public class TimerButtonVM : DashboardButtonVM, ITimed
	{
		TimeNode currentNode;
		Time currentTime;

		public TimerButtonVM ()
		{
			currentTime = new Time (0);
		}

		/// <summary>
		/// Gets or sets the model.
		/// </summary>
		/// <value>The model.</value>
		public new TimerButton Model {
			get {
				return (TimerButton)base.Model;
			}
			set {
				base.Model = value;
			}
		}

		/// <summary>
		/// Gets the view.
		/// </summary>
		/// <value>The view.</value>
		public override string View {
			get {
				return "TimerButtonView";
			}
		}

		/// <summary>
		/// Gets or sets the timer.
		/// </summary>
		/// <value>The timer.</value>
		public Timer Timer {
			get {
				return Model.Timer;
			}
			set {
				Model.Timer = value;
			}
		}

		[PropertyChanged.DoNotNotify]
		public Time CurrentTime {
			get {
				return currentTime;
			}

			set {
				if (value != null && currentNode != null &&
					Mode != DashboardMode.Edit && currentTime != null) {
					if (currentTime + Constants.PLAYBACK_TOLERANCE < currentNode.Start) {
						TimerTime = null;
						currentNode = null;
					}
					if (currentTime.TotalSeconds != value.TotalSeconds) {
						TimerTime = value - currentNode.Start;
					}
				}
				currentTime = value;
			}
		}

		public Time TimerTime {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.ViewModel.TimerButtonVM"/> is active.
		/// </summary>
		/// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
		public bool Active { get; set; }

		public void Click (bool cancel)
		{
			//Cancel
			if (cancel || Mode == DashboardMode.Edit) {
				currentNode = null;
				TimerTime = null;
				return;
			}
			//Start
			if (!Start (null)) {
				Stop (null);
			}
		}

		/// <summary>
		/// Start the timer operation.
		/// </summary>
		/// <param name="buttons">Buttons.</param>
		public bool Start (List<DashboardButtonVM> buttons)
		{
			bool result = false;

			if (currentNode == null) {
				currentNode = new TimeNode { Name = Name, Start = CurrentTime };
				Active = true;
				TimerTime = new Time (0);
				result = true;
				App.Current.EventsBroker.Publish (new TimeNodeStartedEvent { 
					DashboardButtons = buttons, TimerButton = this, TimeNode = currentNode });
			}

			return result;
		}

		/// <summary>
		/// Stops the timer operation
		/// </summary>
		/// <param name="buttons">Buttons.</param>
		public void Stop (List<DashboardButtonVM> buttons)
		{
			if (currentNode.Start.MSeconds != CurrentTime.MSeconds) {
				currentNode.Stop = CurrentTime;
				Timer.Nodes.Add (currentNode);
			}

			Active = false;
			currentNode = null;
			TimerTime = null;
			App.Current.EventsBroker.Publish (new TimeNodeStoppedEvent { 
				DashboardButtons = buttons, TimerButton = this, TimeNode = currentNode });
		}
	}
}
