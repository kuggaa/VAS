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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.MVVMC;
using VAS.Core.Serialization;

namespace VAS.Core.Store
{
	[Serializable]
	public class DashboardButton : BindableBase
	{

		public DashboardButton ()
		{
			Name = "";
			Position = new Point (0, 0);
			Position.IsChanged = false;
			Width = Constants.BUTTON_WIDTH;
			Height = Constants.BUTTON_HEIGHT;
			BackgroundColor = Color.Red.Copy (true);
			if (Utils.OS == OperatingSystemID.Android || Utils.OS == OperatingSystemID.iOS) {
				TextColor = App.Current.Style.PaletteBackground.Copy (true);
			} else {
				TextColor = App.Current.Style.PaletteBackgroundLight.Copy (true);
			}
			HotKey = new HotKey { IsChanged = false };
			ActionLinks = new RangeObservableCollection<ActionLink> ();
			ShowIcon = false;
			ShowHotkey = false;
			ShowSettingIcon = false;
		}

		public virtual string Name {
			get;
			set;
		}

		public Point Position {
			get;
			set;
		}

		public int Width {
			get;
			set;
		}

		public int Height {
			get;
			set;
		}

		public virtual Color BackgroundColor {
			get;
			set;
		}

		public Color TextColor {
			get;
			set;
		}

		public virtual HotKey HotKey {
			get;
			set;
		}

		public virtual Image BackgroundImage {
			get;
			set;
		}

		[DefaultValue (true)]
		[JsonProperty (DefaultValueHandling = DefaultValueHandling.Populate)]
		public bool ShowIcon {
			get;
			set;
		}

		public bool ShowHotkey {
			get;
			set;
		}

		public bool ShowSettingIcon {
			get;
			set;
		}

		/// <summary>
		/// A list with all the outgoing links of this button
		/// </summary>
		public RangeObservableCollection<ActionLink> ActionLinks {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public Color LightColor {
			get {
				YCbCrColor c = YCbCrColor.YCbCrFromColor (BackgroundColor);
				byte y = c.Y;
				c.Y = (byte)(Math.Min (y + 50, 255));
				return c.RGBColor ();
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public Color DarkColor {
			get {
				YCbCrColor c = YCbCrColor.YCbCrFromColor (BackgroundColor);
				byte y = c.Y;
				c.Y = (byte)(Math.Max (y - 50, 0));
				return c.RGBColor ();
			}
		}

		public void AddActionLink (ActionLink link)
		{
			link.SourceButton = this;
			ActionLinks.Add (link);
		}
	}

	[Serializable]
	public class TimedDashboardButton : DashboardButton
	{
		public TimedDashboardButton ()
		{
			TagMode = TagMode.Predefined;
			Start = new Time { TotalSeconds = 10 };
			if (Start != null) {
				Start.IsChanged = false;
			}
			Stop = new Time { TotalSeconds = 10 };
			if (Stop != null) {
				Stop.IsChanged = false;
			}
		}

		public TagMode TagMode {
			get;
			set;
		}

		public virtual Time Start {
			get;
			set;
		}

		public virtual Time Stop {
			get;
			set;
		}
	}

	[Serializable]
	public class TagButton : DashboardButton
	{
		public TagButton ()
		{
			BackgroundColor = StyleConf.ButtonTagColor.Copy (true);
		}

		public Tag Tag {
			get;
			set;
		}

		[CloneIgnoreAttribute]
		public override HotKey HotKey {
			get {
				return Tag != null ? Tag.HotKey : null;
			}
			set {
				if (Tag != null) {
					Tag.HotKey = value;
				}
			}
		}

		[CloneIgnoreAttribute]
		public override string Name {
			get {
				return Tag != null ? Tag.Value : null;
			}
			set {
				if (Tag != null) {
					Tag.Value = value;
				}
			}
		}
	}

	[Serializable]
	public class TimerButton : DashboardButton
	{
		public TimerButton ()
		{
			BackgroundColor = StyleConf.ButtonTimerColor.Copy (true);
		}

		virtual public Timer Timer {
			get;
			set;
		}

		[CloneIgnoreAttribute]
		public override string Name {
			get {
				return Timer != null ? Timer.Name : null;
			}
			set {
				if (Timer != null) {
					Timer.Name = value;
				}
			}
		}
	}

	[Serializable]
	public class EventButton : TimedDashboardButton
	{
		public EventType EventType {
			get;
			set;
		}

		[CloneIgnoreAttribute]
		public override string Name {
			get {
				return EventType != null ? EventType.Name : null;
			}
			set {
				if (EventType != null) {
					EventType.Name = value;
				}
			}
		}

		[CloneIgnoreAttribute]
		public override Color BackgroundColor {
			get {
				return EventType != null ? EventType.Color : null;
			}
			set {
				if (EventType != null) {
					EventType.Color = value;
				}
			}
		}

		[OnDeserialized ()]
		internal void OnDeserializedMethod (StreamingContext context)
		{
			if (EventType != null) {
				EventType.IsChanged = false;
			}
		}
	}

	[Serializable]
	public class AnalysisEventButton : EventButton
	{
		public AnalysisEventButton ()
		{
			TagsPerRow = 2;
			ShowSubcategories = true;
		}

		public bool ShowSubcategories {
			get;
			set;
		}

		public int TagsPerRow {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public AnalysisEventType AnalysisEventType {
			get {
				return EventType as AnalysisEventType;
			}
		}

		[OnDeserialized ()]
		internal new void OnDeserializedMethod (StreamingContext context)
		{
			base.OnDeserializedMethod (context);

			// update source action links with the tags instances in the button to spread changes
			// in both elements at the same time, without this when a tag is edited link is lost 
			// because it was not updated and it is considered a different one
			foreach (var link in ActionLinks) {
				var sourceTags = link.SourceTags.ToList ();
				link.SourceTags.IgnoreEvents = true;

				link.SourceTags.Clear ();
				foreach (var s in sourceTags) {
					var e = AnalysisEventType.Tags.FirstOrDefault (x => x.Equals (s));
					link.SourceTags.Add (e);
				}

				link.SourceTags.IgnoreEvents = false;
			}
		}
	}
}

